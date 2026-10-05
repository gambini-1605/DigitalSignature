namespace DigitalSignature
{
    partial class FrmUploadFiles
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Declaración de los controles para el Archivo 1
        private System.Windows.Forms.Button btnCertificado;
        private System.Windows.Forms.TextBox txtCertificado;
        private System.Windows.Forms.OpenFileDialog openFileCertificado;
        private System.Windows.Forms.OpenFileDialog openFilePassword;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.btnCertificado = new System.Windows.Forms.Button();
            this.txtCertificado = new System.Windows.Forms.TextBox();
            this.openFileCertificado = new System.Windows.Forms.OpenFileDialog();
            this.openFilePassword = new System.Windows.Forms.OpenFileDialog();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnCertificado
            // 
            this.btnCertificado.Location = new System.Drawing.Point(427, 34);
            this.btnCertificado.Margin = new System.Windows.Forms.Padding(4);
            this.btnCertificado.Name = "btnCertificado";
            this.btnCertificado.Size = new System.Drawing.Size(127, 28);
            this.btnCertificado.TabIndex = 1;
            this.btnCertificado.Text = "Certificado";
            this.btnCertificado.UseVisualStyleBackColor = true;
            this.btnCertificado.Click += new System.EventHandler(this.btnCertificado_Click);
            // 
            // txtCertificado
            // 
            this.txtCertificado.Location = new System.Drawing.Point(40, 37);
            this.txtCertificado.Margin = new System.Windows.Forms.Padding(4);
            this.txtCertificado.Name = "txtCertificado";
            this.txtCertificado.ReadOnly = true;
            this.txtCertificado.Size = new System.Drawing.Size(372, 22);
            this.txtCertificado.TabIndex = 0;
            // 
            // openFilePassword
            // 
            this.openFilePassword.Filter = "Archivos PDF (*.pdf)|*.pdf|Todos los archivos (*.*)|*.*";
            // 
            // btnAceptar
            // 
            this.btnAceptar.Location = new System.Drawing.Point(146, 137);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(75, 23);
            this.btnAceptar.TabIndex = 4;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Location = new System.Drawing.Point(263, 137);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(75, 23);
            this.btnCerrar.TabIndex = 5;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(40, 92);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(372, 22);
            this.txtPassword.TabIndex = 6;
            // 
            // FrmUploadFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 172);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnCerrar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCertificado);
            this.Controls.Add(this.txtCertificado);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmUploadFiles";
            this.Text = "Subir Archivos para Firma Digital";
            this.Load += new System.EventHandler(this.FrmUploadFiles_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.TextBox txtPassword;
    }
}