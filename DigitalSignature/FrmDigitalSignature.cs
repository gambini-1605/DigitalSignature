#region using
using DigitalSignature.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Newtonsoft.Json;
using NLog;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

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
        private bool _cabeceraMarcada;
        private bool _actualizandoChecks;

        private Panel pnlLoader;
        private Label lblLoader;
        private ProgressBar pbLoader;

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
            CrearLoader();
            CrearCheckCabecera();

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
                MostrarLoader(true, "Consultando documentos...");
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
                MostrarLoader(false);
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

        #region Loader
        private void CrearLoader()
        {
            pnlLoader = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };

            lblLoader = new Label
            {
                AutoSize = false,
                Width = 300,
                Height = 25,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                Text = "Cargando..."
            };

            pbLoader = new ProgressBar
            {
                Width = 300,
                Height = 20,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };

            pnlLoader.Controls.Add(lblLoader);
            pnlLoader.Controls.Add(pbLoader);
            pnlLoader.Resize += delegate { CentrarLoader(); };

            this.Controls.Add(pnlLoader);
            pnlLoader.BringToFront();
        }

        private void CentrarLoader()
        {
            int x = (pnlLoader.Width - pbLoader.Width) / 2;
            int y = (pnlLoader.Height - (lblLoader.Height + pbLoader.Height + 10)) / 2;

            lblLoader.Location = new Point(x, y);
            pbLoader.Location = new Point(x, y + lblLoader.Height + 10);
        }

        private void MostrarLoader(bool mostrar, string mensaje = "Cargando...")
        {
            if (pnlLoader == null || IsDisposed) return;

            lblLoader.Text = mensaje;

            if (mostrar)
            {
                CentrarLoader();
                pnlLoader.BringToFront();
            }

            pnlLoader.Visible = mostrar;
            pnlLoader.Refresh();
        }
        #endregion

        #region Check de cabecera de la grilla
        private void CrearCheckCabecera()
        {
            // La columna 0 no debe ordenar al hacer clic en la cabecera
            if (dgvDocumentos.Columns.Count > 0)
            {
                dgvDocumentos.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvDocumentos.Columns[0].HeaderText = string.Empty;
            }

            // Se desuscribe antes por si el evento ya estaba enlazado en el Designer
            dgvDocumentos.CellPainting -= dgvDocumentos_CellPainting;
            dgvDocumentos.CellPainting += dgvDocumentos_CellPainting;

            dgvDocumentos.ColumnHeaderMouseClick -= dgvDocumentos_ColumnHeaderMouseClick;
            dgvDocumentos.ColumnHeaderMouseClick += dgvDocumentos_ColumnHeaderMouseClick;

            dgvDocumentos.CurrentCellDirtyStateChanged -= dgvDocumentos_CurrentCellDirtyStateChanged;
            dgvDocumentos.CurrentCellDirtyStateChanged += dgvDocumentos_CurrentCellDirtyStateChanged;

            dgvDocumentos.CellValueChanged -= dgvDocumentos_CellValueChanged;
            dgvDocumentos.CellValueChanged += dgvDocumentos_CellValueChanged;

            dgvDocumentos.DataBindingComplete -= dgvDocumentos_DataBindingComplete;
            dgvDocumentos.DataBindingComplete += dgvDocumentos_DataBindingComplete;
        }

        private void dgvDocumentos_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex != -1 || e.ColumnIndex != 0) return;

            e.PaintBackground(e.ClipBounds, false);

            var estado = _cabeceraMarcada
                ? CheckBoxState.CheckedNormal
                : CheckBoxState.UncheckedNormal;

            Size glifo = CheckBoxRenderer.GetGlyphSize(e.Graphics, estado);

            Point ubicacion = new Point(
                e.CellBounds.X + (e.CellBounds.Width - glifo.Width) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - glifo.Height) / 2);

            CheckBoxRenderer.DrawCheckBox(e.Graphics, ubicacion, estado);
            e.Handled = true;
        }

        private void dgvDocumentos_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Confirma el cambio del check de la fila apenas se hace clic
            if (dgvDocumentos.IsCurrentCellDirty &&
                dgvDocumentos.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvDocumentos.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvDocumentos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                SincronizarCheckCabecera();
        }

        private void dgvDocumentos_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Al recargar la grilla, la cabecera vuelve a desmarcada
            _cabeceraMarcada = false;
            dgvDocumentos.InvalidateCell(0, -1);
        }

        private void SincronizarCheckCabecera()
        {
            if (_actualizandoChecks) return;

            bool todas = dgvDocumentos.Rows.Count > 0;

            foreach (DataGridViewRow row in dgvDocumentos.Rows)
            {
                if (row.IsNewRow) continue;

                if (!Convert.ToBoolean(row.Cells[0].Value ?? false))
                {
                    todas = false;
                    break;
                }
            }

            if (_cabeceraMarcada != todas)
            {
                _cabeceraMarcada = todas;
                dgvDocumentos.InvalidateCell(0, -1);
            }
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

                bool terminado = false;


                if (rbTokenUSB.Checked)
                {
                    terminado = await ProcesarTokenUSB();
                }

                if (rbArchivoPfx.Checked)
                {
                    logger.Info("btnIniciarFirma_Click - Modo archivo PFX.");
                    await ProcesarFirmaDigitalConCertificadoYContrasenia();
                    terminado = true;
                }

                if (terminado)
                {
                    logger.Info("Firma finalizada. Cerrando DigitalSignature.");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al recorrer la grilla: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.Error(ex, "Ocurrió una excepción al procesar los documentos de la grilla en el botón Iniciar firma.");
            }
            finally
            {
                MostrarLoader(false);
                if (!IsDisposed)
                    btnIniciarFirma.Enabled = true;
            }
        }

        private async Task ProcesarFirmaDigitalConCertificadoYContrasenia()
        {
            try
            {
                using (var frmUploadFiles = new FrmUploadFiles())
                {
                    var dialogResult = frmUploadFiles.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        var certificado = frmUploadFiles.Certificado;
                        var password = frmUploadFiles.Password;

                        var filas = ObtenerFilasSeleccionadas();

                        if (filas.Count == 0)
                        {
                            logger.Warn("No hay filas con el checkbox marcado en la grilla.");
                            MessageBox.Show("Por favor, seleccione al menos un documento marcando su casilla.",
                                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        int firmados = 0;

                        foreach (var fila in filas)
                        {
                            string tocId = fila.Cells["id"]?.Value?.ToString() ?? string.Empty;
                            string fullPath = fila.Cells["colFullPath"]?.Value?.ToString() ?? string.Empty;
                            string signedFullPath = fila.Cells["colSignedFullPath"]?.Value?.ToString() ?? string.Empty;

                            string error = null;
                            string resultadoFirmaDigital = string.Empty;

                            try
                            {
                                resultadoFirmaDigital = GenerarFirmaDigitalConCertificadoYContrasenia(fullPath, signedFullPath, certificado, password);

                                firmados++;

                                if (string.IsNullOrEmpty(resultadoFirmaDigital))
                                {
                                    error = "No se encontró la clave privada en el certificado.";
                                    MessageBox.Show($"Error: {error}", "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, $"Error al firmar el documento {tocId} con PFX.");
                                error = "No se encontró la clave privada en el certificado.";
                                MessageBox.Show($"Error: {error}", "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            await RegistrarResultadoAsync(tocId, signedFullPath, resultadoFirmaDigital, error);
                        }

                        CargarGrilla();

                        logger.Info($"Documentos firmados con PFX: {firmados} de {filas.Count}.");

                        MessageBox.Show(
                                        $"Documentos firmados: {firmados} de {filas.Count}.",
                                        "Resultado", MessageBoxButtons.OK,
                                        firmados == filas.Count ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    }
                    else
                    {
                        logger.Info("El usuario canceló la selección de archivos en FrmUploadFiles.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ocurrió una excepción al procesar la firma digital.");
                MessageBox.Show("Error al procesar la firma digital:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerarFirmaDigitalConCertificadoYContrasenia(string fullPath, string signedFullPath, string certificate, string password)
        {
            try
            {
                using (PdfReader pdfReader = new PdfReader(fullPath))
                {
                    Pkcs12Store pfxKeyStore = new Pkcs12StoreBuilder().Build();

                    using (Stream stream = new FileStream(certificate, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        pfxKeyStore.Load(stream, password.ToCharArray());

                        using (FileStream fsDestino = new FileStream(signedFullPath, FileMode.Create))
                        {
                            PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, fsDestino, '\0', null, true);

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
                            var bcCert = pfxKeyStore.GetCertificate(alias).Certificate;

                            if (alias == null)
                                return null;


                            ICipherParameters privateKey = pfxKeyStore.GetKey(alias).Key;
                            IExternalSignature pks = new PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);
                            MakeSignature.SignDetached(signatureAppearance, pks, new
                            Org.BouncyCastle.X509.X509Certificate[] { bcCert },
                            null, null, null, 0, CryptoStandard.CMS);

                            X509Certificate2 cert = new X509Certificate2(DotNetUtilities.ToX509Certificate(bcCert));

                            return ConstruirResultadoFirma(cert);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error al generar la firma digital: " + ex.Message);
                return null;
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

                //pdfStamper.Close();

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

            List<string> ous = ObtenerValoresSubject(cert.Subject, "OU");


            var resultado = new
            {
                nombreFedatario = cert.GetNameInfo(X509NameType.SimpleName, false), // CN del titular
                fechaDesde = cert.NotBefore.ToString(),
                fechaHasta = cert.NotAfter.ToString(),
                emitidoPor = cert.GetNameInfo(X509NameType.SimpleName, true),       // CN del emisor
                versionDigitalSignature = cert.Version,
                algoritmo = cert.SignatureAlgorithm.FriendlyName,
                asunto = cert.Subject,
                emisor = cert.GetNameInfo(X509NameType.SimpleName, true),
                datosFedatario01 = ous.Count > 0 ? ous[0] : null,
                datosFedatario02 = ous.Count > 1 ? ous[1] : null,
            };

            return JsonConvert.SerializeObject(resultado);
        }

        /// <summary>
        /// Devuelve todos los valores de un atributo del Subject (por ejemplo "OU").
        /// Respeta valores entre comillas que contengan comas.
        /// </summary>
        private static List<string> ObtenerValoresSubject(string subject, string atributo)
        {
            var valores = new List<string>();
            if (string.IsNullOrEmpty(subject)) return valores;

            var partes = new List<string>();
            var actual = new StringBuilder();
            bool enComillas = false;

            foreach (char c in subject)
            {
                if (c == '"') enComillas = !enComillas;

                if (c == ',' && !enComillas)
                {
                    partes.Add(actual.ToString());
                    actual.Clear();
                }
                else
                {
                    actual.Append(c);
                }
            }
            partes.Add(actual.ToString());

            string prefijo = atributo + "=";

            foreach (string parte in partes)
            {
                string p = parte.Trim();
                if (p.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                    valores.Add(p.Substring(prefijo.Length).Trim('"'));
            }

            return valores;
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
                Status = ok ? "SIGNING" : "ERROR",
                ErrorCode = ok ? null : "SIGN_FAILED",
                ErrorMessage = ok ? null : errorMessage,
                SignatureResult = ok ? signatureResult : null
            };

            try
            {
                // Loguear el DTO que se va a enviar (serializado)
                string dtoJson = JsonConvert.SerializeObject(dto);
                logger.Info($"Enviando actualización a la API para doc {tocId}. DTO: {dtoJson}");
            }
            catch (Exception ex)
            {
                // No detener el flujo por un error al loguear
                logger.Warn(ex, "Error al serializar el DTO para logging antes de enviar a la API.");
            }

            var response = await ActualizarDocumentoFirmadoAsync(Convert.ToInt32(tocId), dto);

            if (!response.Success)
                logger.Warn($"La API rechazó la actualización del doc {tocId}: {response.StatusCode} - {response.Message}");
            else
                logger.Info($"Actualización exitosa del doc {tocId}: {response.StatusCode} - {response.Message}");
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

        private void dgvDocumentos_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != 0) return;

            dgvDocumentos.EndEdit();

            bool nuevoValor = !_cabeceraMarcada;   // se calcula una sola vez

            _actualizandoChecks = true;
            try
            {
                foreach (DataGridViewRow row in dgvDocumentos.Rows)
                {
                    if (row.IsNewRow) continue;
                    row.Cells[0].Value = nuevoValor;
                }
            }
            finally
            {
                _actualizandoChecks = false;
            }

            _cabeceraMarcada = nuevoValor;

            dgvDocumentos.InvalidateCell(0, -1);
            dgvDocumentos.Refresh();
        }
    }
}