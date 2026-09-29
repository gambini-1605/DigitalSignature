using DigitalSignature.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Newtonsoft.Json;
using NLog;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace DigitalSignature
{
    public partial class FrmDigitalSignature : Form
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient client = new HttpClient();
        private string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"] ?? "https://localhost:7010/api/signature/status";
        private Guid _requestId;
        private List<int> _documentIds;

        private void FrmDigitalSignature_Load(object sender, EventArgs e)
        {
            CargarGrilla();
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
                //btnConsultar.Enabled = false;
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

        private void btnIniciarFirma_Click(object sender, EventArgs e)
        {
            try
            {

                if (rbTokenUSB.Checked)
                {

                }

                if (rbArchivoPfx.Checked)
                {
                    MostrarFilasSeleccionadas();
                }
                return;

                // Validar si la grilla tiene elementos
                if (dgvDocumentos.Rows.Count == 0)
                {
                    MessageBox.Show("No hay documentos en la grilla para procesar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    logger.Warn("Se intentó iniciar la firma sin documentos en la grilla.");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Listado de documentos en la grilla:");
                sb.AppendLine("--------------------------------------------------\n");

                // Recorrer cada fila del DataGridView
                foreach (DataGridViewRow row in dgvDocumentos.Rows)
                {
                    // Omitir la fila vacía de nueva inserción (si la grilla la tuviera habilitada)
                    if (row.IsNewRow) continue;

                    // Obtener los valores de las celdas (asegúrate de que los nombres de las columnas 
                    // coincidan con las propiedades mapeadas desde tu API)
                    var id = row.Cells["Id"]?.Value?.ToString() ?? string.Empty;
                    var nombreDoc = row.Cells["colName"]?.Value?.ToString() ?? row.Cells["colName"]?.Value?.ToString() ?? string.Empty;
                    var archivo = row.Cells["ColFileName"]?.Value?.ToString() ?? row.Cells["ColFileName"]?.Value?.ToString() ?? string.Empty;
                    //var estado = row.Cells["Estado"]?.Value?.ToString() ?? row.Cells[3]?.Value?.ToString() ?? string.Empty;
                    var fullPath = row.Cells["colFullPath"]?.Value?.ToString() ?? row.Cells["colFullPath"]?.Value?.ToString() ?? string.Empty;
                    var SignedFullPath = row.Cells["colSignedFullPath"]?.Value?.ToString() ?? row.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;

                    sb.AppendLine($"• ID: {id}");
                    sb.AppendLine($"  Nombre: {nombreDoc}");
                    sb.AppendLine($"  Archivo: {archivo}");
                    sb.AppendLine($"  fullPath: {fullPath}");
                    sb.AppendLine($"  SignedFullPath: {SignedFullPath}");
                    //sb.AppendLine($"  Estado: {estado}");
                    sb.AppendLine("--------------------------------------------------");
                }

                logger.Info("Se recorrió la grilla de documentos exitosamente desde el botón Iniciar firma.");

                // Mostrar el mensaje con todos los campos recopilados
                MessageBox.Show(sb.ToString(), "Documentos a Firmar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al recorrer la grilla: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.Error(ex, "Ocurrió una excepción al procesar los documentos de la grilla en el botón Iniciar firma.");
            }
        }

        private void FirmaToken()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void FirmaContrasenia(string fullPath, string signedFullPath)
        {
            try
            {
                //string pdfFilePath = "C:\\Users\\USUARIO\\Downloads\\FirmaDigital_Desa-20260925T155354Z-1-001\\FirmaDigital_Desa\\Certificado\\documento_demo.pdf";
                PdfReader pdfReader = new PdfReader(fullPath);

                string pfxFilePath = "C:\\Users\\USUARIO\\Downloads\\FirmaDigital_Desa-20260925T155354Z-1-001\\FirmaDigital_Desa\\2026\\clVtZzM5ZnZHbFFhYVc0Nw==.p12";
                string pfxPassword = "7uXSmFJsyq+o[+tJ";

                //Pkcs12Store pfxKeyStore = new Pkcs12Store(new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read), pfxPassword.ToCharArray());
                Org.BouncyCastle.Pkcs.Pkcs12Store pfxKeyStore = new Org.BouncyCastle.Pkcs.Pkcs12StoreBuilder().Build();

                using (System.IO.Stream stream = new System.IO.FileStream(pfxFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                {
                    pfxKeyStore.Load(stream, pfxPassword.ToCharArray());

                    PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, new FileStream(signedFullPath, FileMode.Create), '\0', null, true);

                    PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
                    signatureAppearance.Reason = "Digital Signature Reason";
                    signatureAppearance.Location = "Digital Signature Location";

                    // Set the signature appearance location (in points)
                    float x = 360;
                    float y = 130;
                    signatureAppearance.Acro6Layers = false;

                    signatureAppearance.Reason = "Este documento está assinado digitalmente pelo Estado Peru";
                    signatureAppearance.Location = "Lima, Peru";
                    signatureAppearance.SignDate = DateTime.Now;

                    signatureAppearance.Layer4Text = PdfSignatureAppearance.questionMark;
                    signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(x, y, x + 150, y + 50), 1, "signature");

                    string alias = pfxKeyStore.Aliases.Cast<string>().FirstOrDefault(entryAlias => pfxKeyStore.IsKeyEntry(entryAlias));

                    if (alias != null)
                    {
                        ICipherParameters privateKey = pfxKeyStore.GetKey(alias).Key;
                        IExternalSignature pks = new PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);
                        MakeSignature.SignDetached(signatureAppearance, pks, new
                        Org.BouncyCastle.X509.X509Certificate[] { pfxKeyStore.GetCertificate(alias).Certificate },
                        null, null, null, 0, CryptoStandard.CMS);
                    }
                    else
                    {
                        Console.WriteLine("Private key not found in the PFX certificate.");
                    }

                    pdfStamper.Close();

                } // 

                pdfReader.Close();
                pdfReader = null;

                pfxKeyStore = null;
            }
            catch (Exception)
            {

                throw;
            }
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

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void MostrarFilasSeleccionadas()
        {
            try
            {
                var filasSeleccionadas = new List<DataGridViewRow>();

                // Recorremos todas las filas de la grilla para verificar el estado del CheckBox
                foreach (DataGridViewRow row in dgvDocumentos.Rows)
                {
                    if (row == null || row.IsNewRow) continue;

                    // IMPORTANTE: Asegúrate de que el índice [0] o el nombre de la columna 
                    // corresponde a tu columna de tipo CheckBox. 
                    // Si tiene nombre, puedes usar: row.Cells["NombreDeTuColumnaCheckBox"]
                    var cellCheckBox = row.Cells[0] as DataGridViewCheckBoxCell;

                    if (cellCheckBox != null)
                    {
                        // Obtenemos el valor del checkbox de forma segura (manejando posibles nulos)
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

                var sb = new StringBuilder();
                sb.AppendLine("Listado de documentos seleccionados:");
                sb.AppendLine("--------------------------------------------------");

                foreach (var row in filasSeleccionadas)
                {
                    var id = row.Cells["Id"]?.Value?.ToString() ?? string.Empty;
                    var nombreDoc = row.Cells["colName"]?.Value?.ToString() ?? string.Empty;
                    var archivo = row.Cells["ColFileName"]?.Value?.ToString() ?? string.Empty;
                    var fullPath = row.Cells["colFullPath"]?.Value?.ToString() ?? string.Empty;
                    var signedFullPath = row.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;

                    FirmaContrasenia(fullPath, signedFullPath);
                }

                logger.Info($"Se procesaron {filasSeleccionadas.Count} filas seleccionadas mediante el CheckBox.");
                //MessageBox.Show(sb.ToString(), "Documentos Seleccionados", MessageBoxButtons.OK, MessageBoxIcon.Information);

                MessageBox.Show($"Se procesaron {filasSeleccionadas.Count} filas seleccionadas mediante el CheckBox.", "Documentos Seleccionados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ocurrió una excepción en MostrarFilasSeleccionadas.");
                MessageBox.Show($"Ocurrió un error al procesar las filas seleccionadas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
