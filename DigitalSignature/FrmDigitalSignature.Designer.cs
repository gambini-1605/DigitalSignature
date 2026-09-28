namespace DigitalSignature
{
    partial class FrmDigitalSignature
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.groupSolicitud = new System.Windows.Forms.GroupBox();
            this.txtEstadoSolicitud = new System.Windows.Forms.TextBox();
            this.lblEstadoSolicitudTitulo = new System.Windows.Forms.Label();
            this.txtFechaCreacion = new System.Windows.Forms.TextBox();
            this.lblFechaCreacionTitulo = new System.Windows.Forms.Label();
            this.btnCopiar = new System.Windows.Forms.Button();
            this.txtRequestId = new System.Windows.Forms.TextBox();
            this.lblRequestIdTitulo = new System.Windows.Forms.Label();
            this.groupMecanismo = new System.Windows.Forms.GroupBox();
            this.btnConfigurarCertificado = new System.Windows.Forms.Button();
            this.rbArchivoPfx = new System.Windows.Forms.RadioButton();
            this.rbTokenUSB = new System.Windows.Forms.RadioButton();
            this.groupDocumentos = new System.Windows.Forms.GroupBox();
            this.dgvDocumentos = new System.Windows.Forms.DataGridView();
            this.btnOpcionesExtra = new System.Windows.Forms.Button();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.btnIniciarFirma = new System.Windows.Forms.Button();
            this.groupLog = new System.Windows.Forms.GroupBox();
            this.txtLogProceso = new System.Windows.Forms.TextBox();
            this.colSeleccion = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFullPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSignedFullPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupSolicitud.SuspendLayout();
            this.groupMecanismo.SuspendLayout();
            this.groupDocumentos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocumentos)).BeginInit();
            this.groupLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.Location = new System.Drawing.Point(17, 16);
            this.lblTitulo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(291, 29);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Documentos para firmar";
            // 
            // lblSubtitulo
            // 
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblSubtitulo.Location = new System.Drawing.Point(20, 50);
            this.lblSubtitulo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(408, 16);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Seleccione la solicitud y revise los documentos que se procesarán.";
            // 
            // groupSolicitud
            // 
            this.groupSolicitud.Controls.Add(this.txtEstadoSolicitud);
            this.groupSolicitud.Controls.Add(this.lblEstadoSolicitudTitulo);
            this.groupSolicitud.Controls.Add(this.txtFechaCreacion);
            this.groupSolicitud.Controls.Add(this.lblFechaCreacionTitulo);
            this.groupSolicitud.Controls.Add(this.btnCopiar);
            this.groupSolicitud.Controls.Add(this.txtRequestId);
            this.groupSolicitud.Controls.Add(this.lblRequestIdTitulo);
            this.groupSolicitud.Location = new System.Drawing.Point(21, 84);
            this.groupSolicitud.Margin = new System.Windows.Forms.Padding(4);
            this.groupSolicitud.Name = "groupSolicitud";
            this.groupSolicitud.Padding = new System.Windows.Forms.Padding(4);
            this.groupSolicitud.Size = new System.Drawing.Size(747, 117);
            this.groupSolicitud.TabIndex = 2;
            this.groupSolicitud.TabStop = false;
            this.groupSolicitud.Text = "Solicitud";
            // 
            // txtEstadoSolicitud
            // 
            this.txtEstadoSolicitud.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(252)))));
            this.txtEstadoSolicitud.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEstadoSolicitud.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(84)))), ((int)(((byte)(150)))));
            this.txtEstadoSolicitud.Location = new System.Drawing.Point(171, 80);
            this.txtEstadoSolicitud.Margin = new System.Windows.Forms.Padding(4);
            this.txtEstadoSolicitud.Name = "txtEstadoSolicitud";
            this.txtEstadoSolicitud.ReadOnly = true;
            this.txtEstadoSolicitud.Size = new System.Drawing.Size(145, 23);
            this.txtEstadoSolicitud.TabIndex = 6;
            this.txtEstadoSolicitud.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblEstadoSolicitudTitulo
            // 
            this.lblEstadoSolicitudTitulo.AutoSize = true;
            this.lblEstadoSolicitudTitulo.Location = new System.Drawing.Point(20, 84);
            this.lblEstadoSolicitudTitulo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEstadoSolicitudTitulo.Name = "lblEstadoSolicitudTitulo";
            this.lblEstadoSolicitudTitulo.Size = new System.Drawing.Size(135, 16);
            this.lblEstadoSolicitudTitulo.TabIndex = 5;
            this.lblEstadoSolicitudTitulo.Text = "Estado de la solicitud";
            // 
            // txtFechaCreacion
            // 
            this.txtFechaCreacion.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtFechaCreacion.Location = new System.Drawing.Point(497, 52);
            this.txtFechaCreacion.Margin = new System.Windows.Forms.Padding(4);
            this.txtFechaCreacion.Name = "txtFechaCreacion";
            this.txtFechaCreacion.ReadOnly = true;
            this.txtFechaCreacion.Size = new System.Drawing.Size(219, 22);
            this.txtFechaCreacion.TabIndex = 4;
            // 
            // lblFechaCreacionTitulo
            // 
            this.lblFechaCreacionTitulo.AutoSize = true;
            this.lblFechaCreacionTitulo.Location = new System.Drawing.Point(493, 31);
            this.lblFechaCreacionTitulo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFechaCreacionTitulo.Name = "lblFechaCreacionTitulo";
            this.lblFechaCreacionTitulo.Size = new System.Drawing.Size(119, 16);
            this.lblFechaCreacionTitulo.TabIndex = 3;
            this.lblFechaCreacionTitulo.Text = "Fecha de creación";
            // 
            // btnCopiar
            // 
            this.btnCopiar.Location = new System.Drawing.Point(432, 49);
            this.btnCopiar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopiar.Name = "btnCopiar";
            this.btnCopiar.Size = new System.Drawing.Size(36, 28);
            this.btnCopiar.TabIndex = 2;
            this.btnCopiar.Text = "📋";
            this.btnCopiar.UseVisualStyleBackColor = true;
            // 
            // txtRequestId
            // 
            this.txtRequestId.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtRequestId.Location = new System.Drawing.Point(24, 52);
            this.txtRequestId.Margin = new System.Windows.Forms.Padding(4);
            this.txtRequestId.Name = "txtRequestId";
            this.txtRequestId.ReadOnly = true;
            this.txtRequestId.Size = new System.Drawing.Size(399, 22);
            this.txtRequestId.TabIndex = 1;
            // 
            // lblRequestIdTitulo
            // 
            this.lblRequestIdTitulo.AutoSize = true;
            this.lblRequestIdTitulo.Location = new System.Drawing.Point(20, 31);
            this.lblRequestIdTitulo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRequestIdTitulo.Name = "lblRequestIdTitulo";
            this.lblRequestIdTitulo.Size = new System.Drawing.Size(69, 16);
            this.lblRequestIdTitulo.TabIndex = 0;
            this.lblRequestIdTitulo.Text = "RequestId";
            // 
            // groupMecanismo
            // 
            this.groupMecanismo.Controls.Add(this.btnConfigurarCertificado);
            this.groupMecanismo.Controls.Add(this.rbArchivoPfx);
            this.groupMecanismo.Controls.Add(this.rbTokenUSB);
            this.groupMecanismo.Location = new System.Drawing.Point(787, 84);
            this.groupMecanismo.Margin = new System.Windows.Forms.Padding(4);
            this.groupMecanismo.Name = "groupMecanismo";
            this.groupMecanismo.Padding = new System.Windows.Forms.Padding(4);
            this.groupMecanismo.Size = new System.Drawing.Size(447, 117);
            this.groupMecanismo.TabIndex = 3;
            this.groupMecanismo.TabStop = false;
            this.groupMecanismo.Text = "Mecanismo de firma digital";
            // 
            // btnConfigurarCertificado
            // 
            this.btnConfigurarCertificado.Location = new System.Drawing.Point(287, 74);
            this.btnConfigurarCertificado.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfigurarCertificado.Name = "btnConfigurarCertificado";
            this.btnConfigurarCertificado.Size = new System.Drawing.Size(147, 31);
            this.btnConfigurarCertificado.TabIndex = 3;
            this.btnConfigurarCertificado.Text = "⚙ Configurar";
            this.btnConfigurarCertificado.UseVisualStyleBackColor = true;
            // 
            // rbArchivoPfx
            // 
            this.rbArchivoPfx.AutoSize = true;
            this.rbArchivoPfx.Location = new System.Drawing.Point(24, 66);
            this.rbArchivoPfx.Margin = new System.Windows.Forms.Padding(4);
            this.rbArchivoPfx.Name = "rbArchivoPfx";
            this.rbArchivoPfx.Size = new System.Drawing.Size(181, 20);
            this.rbArchivoPfx.TabIndex = 1;
            this.rbArchivoPfx.Text = "Archivo PFX + contraseña";
            this.rbArchivoPfx.UseVisualStyleBackColor = true;
            // 
            // rbTokenUSB
            // 
            this.rbTokenUSB.AutoSize = true;
            this.rbTokenUSB.Checked = true;
            this.rbTokenUSB.Location = new System.Drawing.Point(24, 38);
            this.rbTokenUSB.Margin = new System.Windows.Forms.Padding(4);
            this.rbTokenUSB.Name = "rbTokenUSB";
            this.rbTokenUSB.Size = new System.Drawing.Size(177, 20);
            this.rbTokenUSB.TabIndex = 0;
            this.rbTokenUSB.TabStop = true;
            this.rbTokenUSB.Text = "Certificado en token USB";
            this.rbTokenUSB.UseVisualStyleBackColor = true;
            // 
            // groupDocumentos
            // 
            this.groupDocumentos.Controls.Add(this.dgvDocumentos);
            this.groupDocumentos.Controls.Add(this.btnOpcionesExtra);
            this.groupDocumentos.Controls.Add(this.btnActualizar);
            this.groupDocumentos.Controls.Add(this.btnIniciarFirma);
            this.groupDocumentos.Location = new System.Drawing.Point(21, 215);
            this.groupDocumentos.Margin = new System.Windows.Forms.Padding(4);
            this.groupDocumentos.Name = "groupDocumentos";
            this.groupDocumentos.Padding = new System.Windows.Forms.Padding(4);
            this.groupDocumentos.Size = new System.Drawing.Size(1212, 283);
            this.groupDocumentos.TabIndex = 4;
            this.groupDocumentos.TabStop = false;
            this.groupDocumentos.Text = "Documentos";
            // 
            // dgvDocumentos
            // 
            this.dgvDocumentos.AllowUserToAddRows = false;
            this.dgvDocumentos.AllowUserToDeleteRows = false;
            this.dgvDocumentos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDocumentos.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvDocumentos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDocumentos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSeleccion,
            this.Id,
            this.colName,
            this.ColFileName,
            this.colStatus,
            this.colFullPath,
            this.colSignedFullPath});
            this.dgvDocumentos.Location = new System.Drawing.Point(24, 68);
            this.dgvDocumentos.Margin = new System.Windows.Forms.Padding(4);
            this.dgvDocumentos.Name = "dgvDocumentos";
            this.dgvDocumentos.RowHeadersVisible = false;
            this.dgvDocumentos.RowHeadersWidth = 51;
            this.dgvDocumentos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDocumentos.Size = new System.Drawing.Size(1172, 197);
            this.dgvDocumentos.TabIndex = 3;
            // 
            // btnOpcionesExtra
            // 
            this.btnOpcionesExtra.Location = new System.Drawing.Point(1156, 23);
            this.btnOpcionesExtra.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpcionesExtra.Name = "btnOpcionesExtra";
            this.btnOpcionesExtra.Size = new System.Drawing.Size(40, 37);
            this.btnOpcionesExtra.TabIndex = 2;
            this.btnOpcionesExtra.Text = "...";
            this.btnOpcionesExtra.UseVisualStyleBackColor = true;
            // 
            // btnActualizar
            // 
            this.btnActualizar.Location = new System.Drawing.Point(1035, 23);
            this.btnActualizar.Margin = new System.Windows.Forms.Padding(4);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.Size = new System.Drawing.Size(113, 37);
            this.btnActualizar.TabIndex = 1;
            this.btnActualizar.Text = "🔄 Actualizar";
            this.btnActualizar.UseVisualStyleBackColor = true;
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);
            // 
            // btnIniciarFirma
            // 
            this.btnIniciarFirma.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnIniciarFirma.ForeColor = System.Drawing.Color.White;
            this.btnIniciarFirma.Location = new System.Drawing.Point(893, 23);
            this.btnIniciarFirma.Margin = new System.Windows.Forms.Padding(4);
            this.btnIniciarFirma.Name = "btnIniciarFirma";
            this.btnIniciarFirma.Size = new System.Drawing.Size(133, 37);
            this.btnIniciarFirma.TabIndex = 0;
            this.btnIniciarFirma.Text = "▶  Iniciar firma";
            this.btnIniciarFirma.UseVisualStyleBackColor = false;
            this.btnIniciarFirma.Click += new System.EventHandler(this.btnIniciarFirma_Click);
            // 
            // groupLog
            // 
            this.groupLog.Controls.Add(this.txtLogProceso);
            this.groupLog.Location = new System.Drawing.Point(21, 507);
            this.groupLog.Margin = new System.Windows.Forms.Padding(4);
            this.groupLog.Name = "groupLog";
            this.groupLog.Padding = new System.Windows.Forms.Padding(4);
            this.groupLog.Size = new System.Drawing.Size(1212, 172);
            this.groupLog.TabIndex = 5;
            this.groupLog.TabStop = false;
            this.groupLog.Text = "Log del proceso";
            // 
            // txtLogProceso
            // 
            this.txtLogProceso.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.txtLogProceso.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLogProceso.Location = new System.Drawing.Point(24, 25);
            this.txtLogProceso.Margin = new System.Windows.Forms.Padding(4);
            this.txtLogProceso.Multiline = true;
            this.txtLogProceso.Name = "txtLogProceso";
            this.txtLogProceso.ReadOnly = true;
            this.txtLogProceso.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogProceso.Size = new System.Drawing.Size(1171, 128);
            this.txtLogProceso.TabIndex = 0;
            // 
            // colSeleccion
            // 
            this.colSeleccion.FillWeight = 30F;
            this.colSeleccion.HeaderText = "";
            this.colSeleccion.MinimumWidth = 6;
            this.colSeleccion.Name = "colSeleccion";
            // 
            // Id
            // 
            this.Id.DataPropertyName = "tocId";
            this.Id.FillWeight = 60F;
            this.Id.HeaderText = "Id";
            this.Id.MinimumWidth = 6;
            this.Id.Name = "Id";
            // 
            // colName
            // 
            this.colName.DataPropertyName = "name";
            this.colName.FillWeight = 150F;
            this.colName.HeaderText = "Nombre del documento";
            this.colName.MinimumWidth = 6;
            this.colName.Name = "colName";
            // 
            // ColFileName
            // 
            this.ColFileName.DataPropertyName = "fileName";
            this.ColFileName.HeaderText = "Archivo";
            this.ColFileName.MinimumWidth = 6;
            this.ColFileName.Name = "ColFileName";
            // 
            // colStatus
            // 
            this.colStatus.DataPropertyName = "status";
            this.colStatus.FillWeight = 70F;
            this.colStatus.HeaderText = "Estado";
            this.colStatus.MinimumWidth = 6;
            this.colStatus.Name = "colStatus";
            // 
            // colFullPath
            // 
            this.colFullPath.DataPropertyName = "fullPath";
            this.colFullPath.HeaderText = "FullPath";
            this.colFullPath.MinimumWidth = 6;
            this.colFullPath.Name = "colFullPath";
            this.colFullPath.Visible = false;
            // 
            // colSignedFullPath
            // 
            this.colSignedFullPath.DataPropertyName = "signedFullPath";
            this.colSignedFullPath.HeaderText = "SignedFullPath";
            this.colSignedFullPath.MinimumWidth = 6;
            this.colSignedFullPath.Name = "colSignedFullPath";
            this.colSignedFullPath.Visible = false;
            // 
            // FrmDigitalSignature
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1249, 695);
            this.Controls.Add(this.groupLog);
            this.Controls.Add(this.groupDocumentos);
            this.Controls.Add(this.groupMecanismo);
            this.Controls.Add(this.groupSolicitud);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.lblTitulo);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmDigitalSignature";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Documentos para firmar";
            this.Load += new System.EventHandler(this.FrmDigitalSignature_Load);
            this.groupSolicitud.ResumeLayout(false);
            this.groupSolicitud.PerformLayout();
            this.groupMecanismo.ResumeLayout(false);
            this.groupMecanismo.PerformLayout();
            this.groupDocumentos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocumentos)).EndInit();
            this.groupLog.ResumeLayout(false);
            this.groupLog.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.GroupBox groupSolicitud;
        private System.Windows.Forms.TextBox txtRequestId;
        private System.Windows.Forms.Label lblRequestIdTitulo;
        private System.Windows.Forms.Button btnCopiar;
        private System.Windows.Forms.TextBox txtFechaCreacion;
        private System.Windows.Forms.Label lblFechaCreacionTitulo;
        private System.Windows.Forms.TextBox txtEstadoSolicitud;
        private System.Windows.Forms.Label lblEstadoSolicitudTitulo;
        private System.Windows.Forms.GroupBox groupMecanismo;
        private System.Windows.Forms.RadioButton rbArchivoPfx;
        private System.Windows.Forms.RadioButton rbTokenUSB;
        private System.Windows.Forms.Button btnConfigurarCertificado;
        private System.Windows.Forms.GroupBox groupDocumentos;
        private System.Windows.Forms.DataGridView dgvDocumentos;
        private System.Windows.Forms.Button btnOpcionesExtra;
        private System.Windows.Forms.Button btnActualizar;
        private System.Windows.Forms.Button btnIniciarFirma;
        private System.Windows.Forms.GroupBox groupLog;
        private System.Windows.Forms.TextBox txtLogProceso;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colSeleccion;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFullPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSignedFullPath;
    }
}