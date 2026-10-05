using System.Windows.Forms;

namespace DigitalSignature
{
    public partial class FrmUploadFiles : Form
    {
        public string Certificado { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;

        public FrmUploadFiles()
        {
            InitializeComponent();
        }

        private void FrmUploadFiles_Load(object sender, System.EventArgs e)
        {
           
        }
                
        private void btnCerrar_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnAceptar_Click(object sender, System.EventArgs e)
        {
            // Leer los valores de los TextBox y asignarlos a las propiedades públicas
            Certificado = txtCertificado.Text?.Trim() ?? string.Empty;
            Password = txtPassword.Text?.Trim() ?? string.Empty;

            // Indicar que el diálogo terminó correctamente y cerrar el formulario
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCertificado_Click(object sender, System.EventArgs e)
        {
            if (openFileCertificado.ShowDialog() == DialogResult.OK)
            {
                txtCertificado.Text = openFileCertificado.FileName;
            }
        }
    }
}
