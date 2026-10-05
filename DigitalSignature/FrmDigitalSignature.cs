#region using
using DigitalSignature.Models;
using iText.Signatures;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Newtonsoft.Json;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IExternalSignature = iTextSharp.text.pdf.security.IExternalSignature;
#endregion

namespace DigitalSignature
{
    public partial class FrmDigitalSignature : Form
    {
        #region Variables
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient client = new HttpClient();
        private string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"] ?? "https://localhost:7010/api/signature/status";
        private Guid _requestId;
        private List<int> _documentIds;

        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]),
            Timeout = TimeSpan.FromSeconds(30)
        };
        #endregion
        private void FrmDigitalSignature_Load(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            if (_requestId != Guid.Empty && _documentIds != null && _documentIds.Count > 0)
            {
                LoadForm(_requestId, _documentIds);
            }
            else
            {
                logger.Warn("El formulario se abrió sin un RequestId o lista de documentos válidos.");
                MessageBox.Show("No se recibieron los parámetros necesarios para la consulta.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public FrmDigitalSignature() : this(Guid.Empty, new List<int>()) { }

        public FrmDigitalSignature(Guid requestId, List<int> documentIds)
        {
            InitializeComponent();
            ConfigurarLogger();

            _requestId = requestId;
            _documentIds = documentIds;
        }

        private async void LoadForm(Guid requestId, List<int> documentIds)
        {

            try
            {
                logger.Info("Iniciando consulta de firma a la API.");

                // 1. Armar el Request
                var requestData = new SignatureApiRequest
                {
                    RequestId = requestId,
                    DocumentIds = documentIds
                };

                string jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                logger.Info($"Enviando RequestId: {requestData.RequestId} con {requestData.DocumentIds.Count} documentos.");
                logger.Info($"ApiUrl: {ApiUrl}");

                // 2. Llamar a la API 
                HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<SignatureApiResponse>(jsonResponse);

                    // 4. Llenar los controles de texto (Cabecera)
                    txtRequestId.Text = apiResponse.RequestId.ToString();
                    txtEstadoSolicitud.Text = apiResponse.Status;
                    txtFechaCreacion.Text = apiResponse.DateCreated.ToString("dd/MM/yyyy HH:mm");

                    // 5. Llenar la grilla
                    dgvDocumentos.DataSource = apiResponse.Documents;
                    dgvDocumentos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    logger.Info($"Consulta exitosa. Estado: {apiResponse.Status}, Documentos recibidos: {apiResponse.Documents?.Count}");
                }
                else
                {
                    logger.Warn($"La API devolvió un estado no exitoso: {response.StatusCode} - {response.ReasonPhrase}");
                    MessageBox.Show($"Error al consultar API: {response.StatusCode} - {response.ReasonPhrase}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.Error(ex, "Ocurrió una excepción durante la llamada a la API.");
            }
            finally
            {
                //btnConsultar.Enabled = true;
                logger.Info("Finalizó el proceso del botón consultar.");
            }
        }

        private void ConfigurarLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            string rutaLog = ConfigurationManager.AppSettings["RutaLogs"] ?? "${basedir}/logs/SignatureLog-${shortdate}.txt";

            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = rutaLog,
                Layout = "${longdate} | ${uppercase:${level}} | ${message} ${exception:format=tostring}"
            };

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        #region ClickEvent
        private async void btnIniciarFirma_Click(object sender, EventArgs e)
        {

            logger.Info("btnIniciarFirma_Click - Inicio.");
            try
            {
                if (HayFilasSeleccionadas() == false)
                {
                    MessageBox.Show("No hay filas seleccionadas.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (rbTokenUSB.Checked)
                {
                    var flowControl = await ProcesarTokenUSB();
                    if (!flowControl)
                    {
                        return;
                    }
                }

                if (rbArchivoPfx.Checked)
                {
                    logger.Info("btnIniciarFirma_Click:", rbArchivoPfx.Checked);

                    using (var frmUploadFiles = new FrmUploadFiles())
                    {
                        var dialogResult = frmUploadFiles.ShowDialog();

                        if (dialogResult == DialogResult.OK)
                        {
                            var certificado = frmUploadFiles.Certificado;
                            var password = frmUploadFiles.Password;

                            ProcesarArchivoPFXConContrasenia(certificado, password);
                        }
                        else
                        {
                            logger.Info("El usuario canceló la selección de archivos en FrmUploadFiles.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al recorrer la grilla: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.Error(ex, "Ocurrió una excepción al procesar los documentos de la grilla en el botón Iniciar firma.");
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }
        #endregion

        #region ProcesarTokenUSB
        private async Task<bool> ProcesarTokenUSB()
        {
            try
            {
                var filasSeleccionadas = new List<DataGridViewRow>();

                foreach (DataGridViewRow row in dgvDocumentos.Rows)
                {
                    if (row == null || row.IsNewRow) continue;

                    var cellCheckBox = row.Cells[0] as DataGridViewCheckBoxCell;

                    if (cellCheckBox != null)
                    {
                        bool isChecked = Convert.ToBoolean(cellCheckBox.Value ?? false);

                        if (isChecked)
                        {
                            filasSeleccionadas.Add(row);
                        }
                    }
                }

                if (filasSeleccionadas.Count == 0)
                {
                    logger.Warn("No hay filas con el checkbox marcado en la grilla.");
                    MessageBox.Show("Por favor, seleccione al menos un documento marcando su casilla.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                foreach (var row in filasSeleccionadas)
                {

                    var tocId = row.Cells["id"]?.Value?.ToString() ?? string.Empty;
                    var fullPath = row.Cells["colFullPath"]?.Value?.ToString() ?? string.Empty;
                    var signedFullPath = row.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;
                    var signedFileName = row.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;//ColFileName

                    var signatureResult = FirmaTokenUSB(fullPath, signedFullPath, tocId);

                    var updateSignatureDocumentRequest = new UpdateSignatureDocumentRequest
                    {
                        SignedFullPath = signedFullPath,
                        Status = "SIGNED",
                        SignatureResult = signatureResult
                    };


                    var updateSignatureDocumentDto = new UpdateSignatureDocumentDto
                    {
                        SignedFullPath = updateSignatureDocumentRequest.SignedFullPath,
                        Status = updateSignatureDocumentRequest.Status,
                        SignatureResult = updateSignatureDocumentRequest.SignatureResult
                    };

                    var response = await ActualizarDocumentoFirmadoAsync(Convert.ToInt32(tocId), updateSignatureDocumentDto);

                    CargarGrilla();
                }
            }
            catch (Exception)
            {

                throw;
            }

            return false;
        }

        private string FirmaTokenUSB(string rutaOrigen, string rutaDestino, string tocId)
        {
            try
            {
                // ==========================================
                // 1. OBTENER CERTIFICADO (SOLO LOS DISPONIBLES PARA FIRMA)
                // ==========================================
                X509Certificate2 cert = null;

                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);

                    var disponibles = new X509Certificate2Collection();

                    foreach (X509Certificate2 c in store.Certificates)
                    {
                        if (EsCertificadoDisponibleParaFirma(c))
                            disponibles.Add(c);
                    }

                    if (disponibles.Count == 0)
                    {
                        MessageBox.Show(
                            "No hay certificados disponibles para firma.\n\n" +
                            "Verifique que el token esté conectado, que el certificado\n" +
                            "no esté vencido y que tenga clave privada.");
                        return null;
                    }

                    if (disponibles.Count == 1)
                    {
                        cert = disponibles[0];
                    }
                    else
                    {
                        var col = X509Certificate2UI.SelectFromCollection(
                            disponibles, "Certificados", "Seleccione el certificado",
                            X509SelectionFlag.SingleSelection);

                        cert = col.Count > 0 ? col[0] : null;
                    }
                }

                if (cert == null)
                    return null;

                // ==========================================
                // 2. OBTENER RUTAS DEL PDF
                // ==========================================

                string pdfEntrada = rutaOrigen;
                string pdfDestino = rutaDestino;

                if (string.IsNullOrWhiteSpace(pdfEntrada))
                {
                    MessageBox.Show("Ingrese la ruta del PDF de entrada.");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(pdfDestino))
                {
                    MessageBox.Show("Ingrese la ruta del PDF destino.");
                    return null;
                }

                if (!System.IO.File.Exists(pdfEntrada))
                {
                    MessageBox.Show("El PDF de entrada no existe:\n\n" + pdfEntrada);
                    return null;
                }

                // ==========================================
                // 3. ACCESO A LA CLAVE PRIVADA
                // ==========================================
                // Aquí CryptoID / Windows puede mostrar la ventana del PIN.

                using (RSA rsa = cert.GetRSAPrivateKey())
                {
                    if (rsa == null)
                    {
                        MessageBox.Show("No se pudo obtener la clave privada.");
                        return null;
                    }
                }

                // ==========================================
                // 4. CREAR DIRECTORIO DESTINO SI NO EXISTE
                // ==========================================

                string directorioDestino = Path.GetDirectoryName(pdfDestino);

                if (!string.IsNullOrWhiteSpace(directorioDestino) &&
                    !Directory.Exists(directorioDestino))
                {
                    Directory.CreateDirectory(directorioDestino);
                }

                // ==========================================
                // 5. FIRMAR PDF
                // ==========================================

                Firma.SignHashed(
                    pdfEntrada,
                    pdfDestino,
                    cert,
                    "Valor Legal",
                    "DigitalSoft",
                    false,
                    false,
                    "");

                // ==========================================
                // 6. VALIDAR RESULTADO
                // ==========================================

                if (System.IO.File.Exists(pdfDestino))
                {
                    FileInfo info = new FileInfo(pdfDestino);

                    var asuntoCertificado = cert.Subject;
                    var serial = cert.SerialNumber;
                    var pdfEntradaOut = pdfEntrada;
                    var nombreFedatario_ = ExtractCN2(cert.Subject.ToString(), "CN");
                    var inicioValidez = string.Empty;
                    var finValidez = string.Empty;

                    string detalleVigenciaFirmas = string.Empty;

                    // Lectura del PDF firmado usando iText7 para extraer la vigencia de las firmas
                    // Si usas iTextSharp (v5.5.x) en lugar de iText 7:
                    using (PdfReader pdfReader = new PdfReader(pdfDestino))
                    {
                        AcroFields fields = pdfReader.AcroFields;
                        var signatureNames = fields.GetSignatureNames();

                        foreach (string name in signatureNames)
                        {
                            iTextSharp.text.pdf.security.PdfPKCS7 pkcs7 = fields.VerifySignature(name);
                            var signingCert = pkcs7.SigningCertificate;

                            inicioValidez = signingCert.NotBefore.ToString();
                            finValidez = signingCert.NotAfter.ToString();

                            detalleVigenciaFirmas += $"\n- Campo '{name}':\n  * Desde: {inicioValidez}\n  * Hasta: {finValidez}";
                        }
                    }
                    var versionCertificado = cert.Version;
                    var algoritmoFirma = cert.SignatureAlgorithm.FriendlyName;
                    var emitidoPor_ = ExtractCN2(cert.Issuer, "CN");

                    //MessageBox.Show(
                    //"FIRMA PDF OK\n\n" +

                    //"Asunto Certificado:\n" +
                    //asuntoCertificado +

                    //"\n\nSerial:\n" +
                    // serial +

                    //"\n\nPDF entrada:\n" +
                    //pdfEntradaOut +

                    //"\n\nPDF firmado:\n" +
                    //pdfDestino +

                    //"\n\nTamaño:\n" +
                    //info.Length +
                    //" bytes" +

                    //"\n\nNombre Fedatario:\n" +
                    //nombreFedatario_ +

                    //"\n\ndetalleVigenciaFirmas:\n" +
                    //detalleVigenciaFirmas +

                    //"\n\nVersion Certificado:\n" +
                    //versionCertificado +

                    //"\n\nAlgoritmo Firma:\n" +
                    //algoritmoFirma+

                    //"\n\nEmitido Por:\n" +
                    //emitidoPor_,

                    //"Firma correcta",
                    //MessageBoxButtons.OK,
                    //MessageBoxIcon.Information);


                    var resultado = new
                    {
                        nombreFedatario = nombreFedatario_,
                        fechaDesde = inicioValidez,
                        fechaHasta = finValidez,
                        emitidoPor = emitidoPor_,
                        versionDigitalSignature = versionCertificado,
                        algoritmo = algoritmoFirma,
                        asunto = asuntoCertificado,

                        //signatureAlgorithm = cert.SignatureAlgorithm.FriendlyName,
                        //signatureAlgorithmOid = cert.SignatureAlgorithm.Value,
                        //subject = cert.Subject,
                        //issuer = cert.Issuer
                    };

                    return JsonConvert.SerializeObject(resultado);
                }
                else
                {
                    MessageBox.Show(
                        "La operación terminó, pero no se encontró el PDF destino.",
                        "Advertencia",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool EsCertificadoDisponibleParaFirma(X509Certificate2 c)
        {
            try
            {
                // 1. Vigencia
                DateTime ahora = DateTime.Now;
                if (ahora < c.NotBefore || ahora > c.NotAfter)
                    return false;

                // 2. Clave privada (no solicita PIN)
                if (!c.HasPrivateKey)
                    return false;

                // 3. Uso de clave: debe permitir firma digital o no repudio
                foreach (X509Extension ext in c.Extensions)
                {
                    var ku = ext as X509KeyUsageExtension;
                    if (ku != null)
                    {
                        bool puedeFirmar =
                            (ku.KeyUsages & X509KeyUsageFlags.DigitalSignature) != 0 ||
                            (ku.KeyUsages & X509KeyUsageFlags.NonRepudiation) != 0;

                        if (!puedeFirmar)
                            return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ProcesarArchivoPFXConContrasenia
        private async void ProcesarArchivoPFXConContrasenia(string certificado, string password)
        {
            try
            {

                logger.Info("ProcesarArchivoPFXConContrasenia inicio:");

                var filasSeleccionadas = new List<DataGridViewRow>();

                foreach (DataGridViewRow row in dgvDocumentos.Rows)
                {
                    if (row == null || row.IsNewRow) continue;

                    var cellCheckBox = row.Cells[0] as DataGridViewCheckBoxCell;

                    if (cellCheckBox != null)
                    {
                        bool isChecked = Convert.ToBoolean(cellCheckBox.Value ?? false);

                        if (isChecked)
                        {
                            filasSeleccionadas.Add(row);
                        }
                    }
                }

                if (filasSeleccionadas.Count == 0)
                {
                    logger.Warn("No hay filas con el checkbox marcado en la grilla.");
                    MessageBox.Show("Por favor, seleccione al menos un documento marcando su casilla.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (var row in filasSeleccionadas)
                {
                    var tocId = row.Cells["Id"]?.Value?.ToString() ?? string.Empty;
                    var nombreDoc = row.Cells["colName"]?.Value?.ToString() ?? string.Empty;
                    var archivo = row.Cells["ColFileName"]?.Value?.ToString() ?? string.Empty;
                    var fullPath = row.Cells["colFullPath"]?.Value?.ToString() ?? string.Empty;
                    var signedFullPath = row.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;

                    var signatureResult = FirmaCertificadoConContrasenia(fullPath, signedFullPath, certificado, password);

                    var updateSignatureDocumentRequest = new UpdateSignatureDocumentRequest
                    {
                        SignedFullPath = signedFullPath,
                        Status = "SIGNED",
                        SignatureResult = signatureResult
                    };


                    var updateSignatureDocumentDto = new UpdateSignatureDocumentDto
                    {
                        SignedFullPath = updateSignatureDocumentRequest.SignedFullPath,
                        Status = updateSignatureDocumentRequest.Status,
                        SignatureResult = updateSignatureDocumentRequest.SignatureResult
                    };

                    var response = await ActualizarDocumentoFirmadoAsync(Convert.ToInt32(tocId), updateSignatureDocumentDto);

                    CargarGrilla();
                }

                logger.Info($"Se procesaron {filasSeleccionadas.Count} filas seleccionadas mediante el CheckBox.");
                MessageBox.Show($"Se procesaron {filasSeleccionadas.Count} filas seleccionadas mediante el CheckBox.", "Documentos Seleccionados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ocurrió una excepción en MostrarFilasSeleccionadas.");
                MessageBox.Show($"Ocurrió un error al procesar las filas seleccionadas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FirmaCertificadoConContrasenia(string fullPath, string signedFullPath, string certificado, string password)
        {
            PdfReader pdfReader = null;

            try
            {
                pdfReader = new PdfReader(fullPath);

                string pfxFilePath = certificado;
                string pfxPassword = password;

                Org.BouncyCastle.Pkcs.Pkcs12Store pfxKeyStore = new Org.BouncyCastle.Pkcs.Pkcs12StoreBuilder().Build();

                string resultadoJson = null;

                using (Stream stream = new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    pfxKeyStore.Load(stream, pfxPassword.ToCharArray());

                    string alias = pfxKeyStore.Aliases.Cast<string>()
                        .FirstOrDefault(entryAlias => pfxKeyStore.IsKeyEntry(entryAlias));

                    // Sin clave privada no se firma: se sale antes de crear el PDF destino
                    if (alias == null)
                        return null;

                    Org.BouncyCastle.X509.X509Certificate bcCert = pfxKeyStore.GetCertificate(alias).Certificate;

                    PdfStamper pdfStamper = PdfStamper.CreateSignature(
                        pdfReader, new FileStream(signedFullPath, FileMode.Create), '\0', null, true);

                    PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;

                    float x = 360;
                    float y = 130;
                    signatureAppearance.Acro6Layers = false;

                    signatureAppearance.Reason = "Este documento está assignado digitalmente para Estado Peru";
                    signatureAppearance.Location = "Lima, Peru";
                    signatureAppearance.SignDate = DateTime.Now;

                    signatureAppearance.Layer4Text = PdfSignatureAppearance.questionMark;
                    signatureAppearance.SetVisibleSignature(
                        new iTextSharp.text.Rectangle(x, y, x + 150, y + 50), 1, "signature");

                    ICipherParameters privateKey = pfxKeyStore.GetKey(alias).Key;
                    IExternalSignature pks = new iTextSharp.text.pdf.security.PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);

                    MakeSignature.SignDetached(
                        signatureAppearance, pks,
                        new Org.BouncyCastle.X509.X509Certificate[] { bcCert },
                        null, null, null, 0, CryptoStandard.CMS);

                    pdfStamper.Close();

                    // Se convierte a X509Certificate2 para que el JSON sea igual al de FirmaToken
                    X509Certificate2 cert = new X509Certificate2(DotNetUtilities.ToX509Certificate(bcCert));

                    var nombreFedatario_ = ExtractCN2(cert.Subject.ToString(), "CN");
                    var emitidoPor_ = ExtractCN2(cert.Issuer, "CN");
                    var algoritmoFirma = cert.SignatureAlgorithm.FriendlyName;
                    var versionCertificado = cert.Version;
                    var inicioValidez = string.Empty;
                    var finValidez = string.Empty;
                    var asuntoCertificado = cert.Subject;

                    using (PdfReader pdfReader2 = new PdfReader(signedFullPath))
                    {
                        AcroFields fields = pdfReader2.AcroFields;
                        var signatureNames = fields.GetSignatureNames();

                        foreach (string name in signatureNames)
                        {
                            iTextSharp.text.pdf.security.PdfPKCS7 pkcs7 = fields.VerifySignature(name);
                            var signingCert = pkcs7.SigningCertificate;

                            inicioValidez = signingCert.NotBefore.ToString();
                            finValidez = signingCert.NotAfter.ToString();
                        }
                    }

                    var resultado = new
                    {
                        nombreFedatario = nombreFedatario_,
                        fechaDesde = inicioValidez,
                        fechaHasta = finValidez,
                        emitidoPor = emitidoPor_,
                        versionDigitalSignature = versionCertificado,
                        algoritmo = algoritmoFirma,
                        asunto = asuntoCertificado
                    };

                    resultadoJson = JsonConvert.SerializeObject(resultado);
                }

                return resultadoJson;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (pdfReader != null)
                    pdfReader.Close();
            }
        }
        #endregion

        private bool HayFilasSeleccionadas()
        {
            try
            {
                if (dgvDocumentos == null)
                {
                    logger.Warn("Intento de validar filas seleccionadas pero dgvDocumentos es null.");
                    return false;
                }

                foreach (DataGridViewRow row in dgvDocumentos.Rows)
                {
                    if (row == null || row.IsNewRow)
                    {
                        continue;
                    }

                    // Intentar por la celda esperada en la columna 0 (estilo existente en el formulario)
                    var cellCheckBox = row.Cells.Count > 0 ? row.Cells[0] as DataGridViewCheckBoxCell : null;

                    if (cellCheckBox != null)
                    {
                        bool isChecked = Convert.ToBoolean(cellCheckBox.Value ?? false);
                        if (isChecked)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // Si no hay checkbox en la posición 0, buscar cualquier celda tipo checkbox en la fila
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell is DataGridViewCheckBoxCell)
                            {
                                bool isChecked = Convert.ToBoolean(cell.Value ?? false);
                                if (isChecked)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ocurrió una excepción al validar si hay filas seleccionadas.");
                return false;
            }
        }

        private async Task<UpdateSignatureDocumentResponse> ActualizarDocumentoFirmadoAsync(int tocId, UpdateSignatureDocumentDto request)
        {
            var resultado = new UpdateSignatureDocumentResponse();

            try
            {
                string json = JsonConvert.SerializeObject(request);

                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                //using (HttpResponseMessage resp = await _http.PutAsync("api/Signature/document/" + tocId, content))
                using (HttpResponseMessage resp = await _http.PutAsync("document/" + tocId.ToString(), content))
                {
                    string body = await resp.Content.ReadAsStringAsync();

                    resultado.StatusCode = (int)resp.StatusCode;
                    resultado.Success = resp.IsSuccessStatusCode;
                    resultado.RawBody = body;

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        try
                        {
                            var parsed = JsonConvert.DeserializeObject<UpdateSignatureDocumentResponse>(body);
                            if (parsed != null) resultado.Message = parsed.Message;
                        }
                        catch (JsonException)
                        {
                            // La API devolvió texto plano u otro formato
                            resultado.Message = body;
                        }
                    }

                    if (!resp.IsSuccessStatusCode && string.IsNullOrEmpty(resultado.Message))
                        resultado.Message = resp.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                resultado.Success = false;
                resultado.Message = "No se pudo conectar con la API: " + ex.Message;
            }

            return resultado;
        }

        public class UpdateSignatureDocumentDto
        {
            public string SignedFileName { get; set; }
            public string SignedFullPath { get; set; }
            public string Status { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
            public string SignatureResult { get; set; }
        }

        private string ExtractCN2(string dn, string strValue)
        {
            string[] parts = dn.Split(new char[] { ',' });

            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i];
                var elems = p.Split(new char[] { '=' });
                var t = elems[0].Trim().ToUpper();
                var v = elems[1].Trim();
                if (t == strValue)
                {
                    return v;
                }
            }
            return null;
        }
    }
}