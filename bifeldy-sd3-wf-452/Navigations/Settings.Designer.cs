
namespace DcTransferFtpNew.Navigations {

    public sealed partial class CSettings {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtBxDaysRetentionFiles = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.btnClearFolderBackup = new System.Windows.Forms.Button();
            this.btnClearFolderZip = new System.Windows.Forms.Button();
            this.btnClearFolderTempCsv = new System.Windows.Forms.Button();
            this.btnOpenFolderBackup = new System.Windows.Forms.Button();
            this.txtBxOpenFolderBackup = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnOpenFolderZip = new System.Windows.Forms.Button();
            this.txtBxOpenFolderZip = new System.Windows.Forms.TextBox();
            this.btnOpenFolderTempCsv = new System.Windows.Forms.Button();
            this.txtBxOpenFolderTempCsv = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBxDaysRetentionFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.lblStatus);
            this.panel1.Controls.Add(this.txtBxDaysRetentionFiles);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.btnClearFolderBackup);
            this.panel1.Controls.Add(this.btnClearFolderZip);
            this.panel1.Controls.Add(this.btnClearFolderTempCsv);
            this.panel1.Controls.Add(this.btnOpenFolderBackup);
            this.panel1.Controls.Add(this.txtBxOpenFolderBackup);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnOpenFolderZip);
            this.panel1.Controls.Add(this.txtBxOpenFolderZip);
            this.panel1.Controls.Add(this.btnOpenFolderTempCsv);
            this.panel1.Controls.Add(this.txtBxOpenFolderTempCsv);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(527, 356);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(64)))), ((int)(((byte)(129)))));
            this.panel2.Location = new System.Drawing.Point(17, 51);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(493, 1);
            this.panel2.TabIndex = 39;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.lblStatus.Location = new System.Drawing.Point(412, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(98, 23);
            this.lblStatus.TabIndex = 38;
            this.lblStatus.Text = "Hari Terakhir";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBxDaysRetentionFiles
            // 
            this.txtBxDaysRetentionFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBxDaysRetentionFiles.Location = new System.Drawing.Point(336, 14);
            this.txtBxDaysRetentionFiles.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.txtBxDaysRetentionFiles.Name = "txtBxDaysRetentionFiles";
            this.txtBxDaysRetentionFiles.Size = new System.Drawing.Size(70, 20);
            this.txtBxDaysRetentionFiles.TabIndex = 34;
            this.txtBxDaysRetentionFiles.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBxDaysRetentionFiles.ThousandsSeparator = true;
            this.txtBxDaysRetentionFiles.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.txtBxDaysRetentionFiles.Value = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.txtBxDaysRetentionFiles.ValueChanged += new System.EventHandler(this.txtBxDaysRetentionFiles_ValueChanged);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label4.Location = new System.Drawing.Point(13, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(295, 23);
            this.label4.TabIndex = 33;
            this.label4.Text = "Max Umur Retention Berkas File";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClearFolderBackup
            // 
            this.btnClearFolderBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearFolderBackup.AutoSize = true;
            this.btnClearFolderBackup.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnClearFolderBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFolderBackup.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnClearFolderBackup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnClearFolderBackup.Location = new System.Drawing.Point(443, 216);
            this.btnClearFolderBackup.Name = "btnClearFolderBackup";
            this.btnClearFolderBackup.Size = new System.Drawing.Size(67, 31);
            this.btnClearFolderBackup.TabIndex = 32;
            this.btnClearFolderBackup.Text = "Hapus";
            this.btnClearFolderBackup.UseVisualStyleBackColor = false;
            this.btnClearFolderBackup.Click += new System.EventHandler(this.BtnClearFolderBackup_Click);
            // 
            // btnClearFolderZip
            // 
            this.btnClearFolderZip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearFolderZip.AutoSize = true;
            this.btnClearFolderZip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnClearFolderZip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFolderZip.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnClearFolderZip.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnClearFolderZip.Location = new System.Drawing.Point(443, 148);
            this.btnClearFolderZip.Name = "btnClearFolderZip";
            this.btnClearFolderZip.Size = new System.Drawing.Size(67, 31);
            this.btnClearFolderZip.TabIndex = 31;
            this.btnClearFolderZip.Text = "Hapus";
            this.btnClearFolderZip.UseVisualStyleBackColor = false;
            this.btnClearFolderZip.Click += new System.EventHandler(this.BtnClearFolderZip_Click);
            // 
            // btnClearFolderTempCsv
            // 
            this.btnClearFolderTempCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearFolderTempCsv.AutoSize = true;
            this.btnClearFolderTempCsv.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnClearFolderTempCsv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFolderTempCsv.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnClearFolderTempCsv.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnClearFolderTempCsv.Location = new System.Drawing.Point(443, 80);
            this.btnClearFolderTempCsv.Name = "btnClearFolderTempCsv";
            this.btnClearFolderTempCsv.Size = new System.Drawing.Size(67, 31);
            this.btnClearFolderTempCsv.TabIndex = 30;
            this.btnClearFolderTempCsv.Text = "Hapus";
            this.btnClearFolderTempCsv.UseVisualStyleBackColor = false;
            this.btnClearFolderTempCsv.Click += new System.EventHandler(this.BtnClearFolderTempCsv_Click);
            // 
            // btnOpenFolderBackup
            // 
            this.btnOpenFolderBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolderBackup.AutoSize = true;
            this.btnOpenFolderBackup.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOpenFolderBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFolderBackup.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnOpenFolderBackup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOpenFolderBackup.Location = new System.Drawing.Point(370, 216);
            this.btnOpenFolderBackup.Name = "btnOpenFolderBackup";
            this.btnOpenFolderBackup.Size = new System.Drawing.Size(67, 31);
            this.btnOpenFolderBackup.TabIndex = 29;
            this.btnOpenFolderBackup.Text = "Buka";
            this.btnOpenFolderBackup.UseVisualStyleBackColor = false;
            this.btnOpenFolderBackup.Click += new System.EventHandler(this.BtnOpenFolderBackup_Click);
            // 
            // txtBxOpenFolderBackup
            // 
            this.txtBxOpenFolderBackup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBxOpenFolderBackup.Location = new System.Drawing.Point(17, 227);
            this.txtBxOpenFolderBackup.Name = "txtBxOpenFolderBackup";
            this.txtBxOpenFolderBackup.ReadOnly = true;
            this.txtBxOpenFolderBackup.Size = new System.Drawing.Size(347, 20);
            this.txtBxOpenFolderBackup.TabIndex = 28;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label3.Location = new System.Drawing.Point(13, 201);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(351, 23);
            this.label3.TabIndex = 27;
            this.label3.Text = "Folder Lokasi Backup";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOpenFolderZip
            // 
            this.btnOpenFolderZip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolderZip.AutoSize = true;
            this.btnOpenFolderZip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOpenFolderZip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFolderZip.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnOpenFolderZip.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOpenFolderZip.Location = new System.Drawing.Point(370, 148);
            this.btnOpenFolderZip.Name = "btnOpenFolderZip";
            this.btnOpenFolderZip.Size = new System.Drawing.Size(67, 31);
            this.btnOpenFolderZip.TabIndex = 26;
            this.btnOpenFolderZip.Text = "Buka";
            this.btnOpenFolderZip.UseVisualStyleBackColor = false;
            this.btnOpenFolderZip.Click += new System.EventHandler(this.BtnOpenFolderZip_Click);
            // 
            // txtBxOpenFolderZip
            // 
            this.txtBxOpenFolderZip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBxOpenFolderZip.Location = new System.Drawing.Point(17, 159);
            this.txtBxOpenFolderZip.Name = "txtBxOpenFolderZip";
            this.txtBxOpenFolderZip.ReadOnly = true;
            this.txtBxOpenFolderZip.Size = new System.Drawing.Size(347, 20);
            this.txtBxOpenFolderZip.TabIndex = 25;
            // 
            // btnOpenFolderTempCsv
            // 
            this.btnOpenFolderTempCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolderTempCsv.AutoSize = true;
            this.btnOpenFolderTempCsv.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOpenFolderTempCsv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFolderTempCsv.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnOpenFolderTempCsv.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOpenFolderTempCsv.Location = new System.Drawing.Point(370, 80);
            this.btnOpenFolderTempCsv.Name = "btnOpenFolderTempCsv";
            this.btnOpenFolderTempCsv.Size = new System.Drawing.Size(67, 31);
            this.btnOpenFolderTempCsv.TabIndex = 24;
            this.btnOpenFolderTempCsv.Text = "Buka";
            this.btnOpenFolderTempCsv.UseVisualStyleBackColor = false;
            this.btnOpenFolderTempCsv.Click += new System.EventHandler(this.BtnOpenFolderTempCsv_Click);
            // 
            // txtBxOpenFolderTempCsv
            // 
            this.txtBxOpenFolderTempCsv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBxOpenFolderTempCsv.Location = new System.Drawing.Point(17, 91);
            this.txtBxOpenFolderTempCsv.Name = "txtBxOpenFolderTempCsv";
            this.txtBxOpenFolderTempCsv.ReadOnly = true;
            this.txtBxOpenFolderTempCsv.Size = new System.Drawing.Size(347, 20);
            this.txtBxOpenFolderTempCsv.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label2.Location = new System.Drawing.Point(13, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(351, 23);
            this.label2.TabIndex = 22;
            this.label2.Text = "Folder Lokasi ZIP";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label1.Location = new System.Drawing.Point(13, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(351, 23);
            this.label1.TabIndex = 21;
            this.label1.Text = "Folder Lokasi Temp / CSV";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "CSettings";
            this.Size = new System.Drawing.Size(527, 356);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBxDaysRetentionFiles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnOpenFolderBackup;
        private System.Windows.Forms.TextBox txtBxOpenFolderBackup;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOpenFolderZip;
        private System.Windows.Forms.TextBox txtBxOpenFolderZip;
        private System.Windows.Forms.Button btnOpenFolderTempCsv;
        private System.Windows.Forms.TextBox txtBxOpenFolderTempCsv;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClearFolderTempCsv;
        private System.Windows.Forms.Button btnClearFolderBackup;
        private System.Windows.Forms.Button btnClearFolderZip;
        private System.Windows.Forms.NumericUpDown txtBxDaysRetentionFiles;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panel2;
    }

}
