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

        #region Constructores y carga
        public FrmDigitalSignature() : this(Guid.Empty, new List<int>()) { }

        public FrmDigitalSignature(Guid requestId, List<int> documentIds)
        {
            InitializeComponent();
            ConfigurarLogger();

            _requestId = requestId;
            _documentIds = documentIds;
        }

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

                    // 3. Llenar los controles de texto (Cabecera)
                    txtRequestId.Text = apiResponse.RequestId.ToString();
                    txtEstadoSolicitud.Text = apiResponse.Status;
                    txtFechaCreacion.Text = apiResponse.DateCreated.ToString("dd/MM/yyyy HH:mm");

                    // 4. Llenar la grilla
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
        #endregion

        #region ClickEvent
        private async void btnIniciarFirma_Click(object sender, EventArgs e)
        {
            logger.Info("btnIniciarFirma_Click - Inicio.");

            try
            {
                if (!HayFilasSeleccionadas())
                {
                    MessageBox.Show("No hay filas seleccionadas.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (rbTokenUSB.Checked)
                {
                    bool procesado = await ProcesarTokenUSB();
                    if (!procesado)
                        return;
                }

                if (rbArchivoPfx.Checked)
                {
                    logger.Info("btnIniciarFirma_Click - Modo archivo PFX.");

                    using (var frmUploadFiles = new FrmUploadFiles())
                    {
                        var dialogResult = frmUploadFiles.ShowDialog();

                        if (dialogResult == DialogResult.OK)
                        {
                            var certificado = frmUploadFiles.Certificado;
                            var password = frmUploadFiles.Password;

                            await ProcesarArchivoPFXConContrasenia(certificado, password);
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
            var filas = ObtenerFilasSeleccionadas();

            if (filas.Count == 0)
            {
                logger.Warn("No hay filas con el checkbox marcado en la grilla.");
                MessageBox.Show("Por favor, seleccione al menos un documento marcando su casilla.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            // El certificado se elige una sola vez para todo el lote
            X509Certificate2 cert = SeleccionarCertificadoToken();
            if (cert == null)
                return false; // sin certificado o el usuario canceló

            int firmados = 0;

            foreach (var row in filas)
            {
                string tocId = row.Cells["id"]?.Value?.ToString() ?? string.Empty;
                string fullPath = row.Cells["colFullPath"]?.Value?.ToString() ?? string.Empty;
                string signedFullPath = row.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;

                string resultado = null;
                string error = null;

                try
                {
                    resultado = FirmaTokenUSB(fullPath, signedFullPath, cert);
                    firmados++;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error al firmar el documento {tocId} con token.");
                    error = ex.Message;
                }

                await RegistrarResultadoAsync(tocId, signedFullPath, resultado, error);
            }

            CargarGrilla(); // una sola vez, al terminar

            MessageBox.Show($"Documentos firmados: {firmados} de {filas.Count}.",
                "Resultado", MessageBoxButtons.OK,
                firmados == filas.Count ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            return true;
        }

        /// <summary>
        /// Muestra solo los certificados disponibles para firma y valida el acceso
        /// a la clave privada (aquí el driver puede pedir el PIN).
        /// Devuelve null si no hay certificados o el usuario cancela.
        /// </summary>
        private X509Certificate2 SeleccionarCertificadoToken()
        {
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

                X509Certificate2 cert;

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

                if (cert == null)
                    return null;

                using (RSA rsa = cert.GetRSAPrivateKey())
                {
                    if (rsa == null)
                    {
                        MessageBox.Show("No se pudo obtener la clave privada.");
                        return null;
                    }
                }

                return cert;
            }
        }

        /// <summary>
        /// Firma un PDF con el certificado indicado. Devuelve el JSON con los datos
        /// del certificado, o lanza una excepción si algo falla.
        /// </summary>
        private string FirmaTokenUSB(string rutaOrigen, string rutaDestino, X509Certificate2 cert)
        {
            if (string.IsNullOrWhiteSpace(rutaOrigen))
                throw new ArgumentException("La ruta del PDF de entrada está vacía.");

            if (string.IsNullOrWhiteSpace(rutaDestino))
                throw new ArgumentException("La ruta del PDF destino está vacía.");

            if (!System.IO.File.Exists(rutaOrigen))
                throw new FileNotFoundException("El PDF de entrada no existe.", rutaOrigen);

            string directorioDestino = Path.GetDirectoryName(rutaDestino);

            if (!string.IsNullOrWhiteSpace(directorioDestino) && !Directory.Exists(directorioDestino))
                Directory.CreateDirectory(directorioDestino);

            Firma.SignHashed(
                rutaOrigen,
                rutaDestino,
                cert,
                "Valor Legal",
                "DigitalSoft",
                false,
                false,
                "");

            if (!System.IO.File.Exists(rutaDestino))
                throw new FileNotFoundException("La operación terminó, pero no se encontró el PDF destino.", rutaDestino);

            return ConstruirResultadoFirma(cert);
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
        private async Task ProcesarArchivoPFXConContrasenia(string certificado, string password)
        {
            logger.Info("ProcesarArchivoPFXConContrasenia inicio.");

            var filas = ObtenerFilasSeleccionadas();

            if (filas.Count == 0)
            {
                logger.Warn("No hay filas con el checkbox marcado en la grilla.");
                MessageBox.Show("Por favor, seleccione al menos un documento marcando su casilla.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int firmados = 0;

            foreach (var row in filas)
            {
                string tocId = row.Cells["Id"]?.Value?.ToString() ?? string.Empty;
                string fullPath = row.Cells["colFullPath"]?.Value?.ToString() ?? string.Empty;
                string signedFullPath = row.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;

                string resultado = null;
                string error = null;

                try
                {
                    resultado = FirmaCertificadoConContrasenia(fullPath, signedFullPath, certificado, password);

                    if (resultado == null)
                        error = "No se encontró la clave privada en el certificado.";
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error al firmar el documento {tocId} con PFX.");
                    error = ex.Message;
                }

                if (error == null)
                    firmados++;

                await RegistrarResultadoAsync(tocId, signedFullPath, resultado, error);
            }

            CargarGrilla(); // una sola vez, al terminar

            logger.Info($"Documentos firmados con PFX: {firmados} de {filas.Count}.");

            MessageBox.Show($"Documentos firmados: {firmados} de {filas.Count}.",
                "Resultado", MessageBoxButtons.OK,
                firmados == filas.Count ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Firma un PDF con un archivo PFX. Devuelve el JSON con los datos del certificado,
        /// o null si el PFX no contiene clave privada.
        /// </summary>
        private string FirmaCertificadoConContrasenia(string fullPath, string signedFullPath, string certificado, string password)
        {
            PdfReader pdfReader = null;
            FileStream salida = null;

            try
            {
                pdfReader = new PdfReader(fullPath);

                Org.BouncyCastle.Pkcs.Pkcs12Store pfxKeyStore = new Org.BouncyCastle.Pkcs.Pkcs12StoreBuilder().Build();

                using (Stream stream = new FileStream(certificado, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    pfxKeyStore.Load(stream, password.ToCharArray());
                }

                string alias = pfxKeyStore.Aliases.Cast<string>()
                    .FirstOrDefault(entryAlias => pfxKeyStore.IsKeyEntry(entryAlias));

                // Sin clave privada no se firma: se sale antes de crear el PDF destino
                if (alias == null)
                    return null;

                Org.BouncyCastle.X509.X509Certificate bcCert = pfxKeyStore.GetCertificate(alias).Certificate;

                string directorioDestino = Path.GetDirectoryName(signedFullPath);

                if (!string.IsNullOrWhiteSpace(directorioDestino) && !Directory.Exists(directorioDestino))
                    Directory.CreateDirectory(directorioDestino);

                salida = new FileStream(signedFullPath, FileMode.Create);

                PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, salida, '\0', null, true);

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

                // Se convierte a X509Certificate2 para que el JSON sea igual al del token
                X509Certificate2 cert = new X509Certificate2(DotNetUtilities.ToX509Certificate(bcCert));

                return ConstruirResultadoFirma(cert);
            }
            finally
            {
                if (salida != null)
                    salida.Dispose();

                if (pdfReader != null)
                    pdfReader.Close();
            }
        }
        #endregion

        #region Utilidades compartidas
        /// <summary>
        /// Arma el JSON con los datos del certificado usado para firmar.
        /// Se usa tanto para el token USB como para el archivo PFX.
        /// </summary>
        private static string ConstruirResultadoFirma(X509Certificate2 cert)
        {
            var resultado = new
            {
                nombreFedatario = cert.GetNameInfo(X509NameType.SimpleName, false), // CN del titular
                fechaDesde = cert.NotBefore.ToString(),
                fechaHasta = cert.NotAfter.ToString(),
                emitidoPor = cert.GetNameInfo(X509NameType.SimpleName, true),       // CN del emisor
                versionDigitalSignature = cert.Version,
                algoritmo = cert.SignatureAlgorithm.FriendlyName,
                asunto = cert.Subject
            };

            return JsonConvert.SerializeObject(resultado);
        }

        private List<DataGridViewRow> ObtenerFilasSeleccionadas()
        {
            var filas = new List<DataGridViewRow>();

            foreach (DataGridViewRow row in dgvDocumentos.Rows)
            {
                if (row == null || row.IsNewRow) continue;

                var chk = row.Cells[0] as DataGridViewCheckBoxCell;
                if (chk != null && Convert.ToBoolean(chk.Value ?? false))
                    filas.Add(row);
            }

            return filas;
        }

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
                        continue;

                    // Intentar por la celda esperada en la columna 0
                    var cellCheckBox = row.Cells.Count > 0 ? row.Cells[0] as DataGridViewCheckBoxCell : null;

                    if (cellCheckBox != null)
                    {
                        if (Convert.ToBoolean(cellCheckBox.Value ?? false))
                            return true;
                    }
                    else
                    {
                        // Si no hay checkbox en la posición 0, buscar cualquier celda tipo checkbox
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell is DataGridViewCheckBoxCell &&
                                Convert.ToBoolean(cell.Value ?? false))
                            {
                                return true;
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
        #endregion

        #region API
        /// <summary>
        /// Informa a la API el resultado de la firma de un documento (SIGNED o ERROR).
        /// </summary>
        private async Task RegistrarResultadoAsync(
            string tocId, string signedFullPath, string signatureResult, string errorMessage)
        {
            bool ok = signatureResult != null && errorMessage == null;

            var dto = new UpdateSignatureDocumentDto
            {
                SignedFileName = Path.GetFileName(signedFullPath),
                SignedFullPath = signedFullPath,
                Status = ok ? "SIGNED" : "ERROR",
                ErrorCode = ok ? null : "SIGN_FAILED",
                ErrorMessage = ok ? null : errorMessage,
                SignatureResult = ok ? signatureResult : null
            };

            var response = await ActualizarDocumentoFirmadoAsync(Convert.ToInt32(tocId), dto);

            if (!response.Success)
                logger.Warn($"La API rechazó la actualización del doc {tocId}: {response.StatusCode} - {response.Message}");
        }

        private async Task<UpdateSignatureDocumentResponse> ActualizarDocumentoFirmadoAsync(int tocId, UpdateSignatureDocumentDto request)
        {
            var resultado = new UpdateSignatureDocumentResponse();

            try
            {
                string json = JsonConvert.SerializeObject(request);

                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
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
        #endregion
    }
}