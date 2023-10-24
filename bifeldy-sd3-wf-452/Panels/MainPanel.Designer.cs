
namespace DcTransferFtpNew.Panels {

    public sealed partial class CMainPanel {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CMainPanel));
            this.chkDebugSimulasi = new System.Windows.Forms.CheckBox();
            this.userInfo = new System.Windows.Forms.Label();
            this.imgDomar = new System.Windows.Forms.PictureBox();
            this.prgrssBrStatus = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.appInfo = new System.Windows.Forms.Label();
            this.lnkLblLogClear = new System.Windows.Forms.LinkLabel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.btnMiscLogErrorProses = new System.Windows.Forms.Button();
            this.btnMiscLogErrorTransfer = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.textBoxLogInfo = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.navContent = new System.Windows.Forms.Panel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.navMenu = new DcTransferFtpNew.Components.FixAutoScrollFlowLayoutPanel();
            this.chkWindowsStartup = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgDomar)).BeginInit();
            this.navContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // chkDebugSimulasi
            // 
            this.chkDebugSimulasi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDebugSimulasi.AutoSize = true;
            this.chkDebugSimulasi.Location = new System.Drawing.Point(575, 87);
            this.chkDebugSimulasi.Name = "chkDebugSimulasi";
            this.chkDebugSimulasi.Size = new System.Drawing.Size(143, 17);
            this.chkDebugSimulasi.TabIndex = 12;
            this.chkDebugSimulasi.Text = "Mode Debug &&/ Simulasi";
            this.chkDebugSimulasi.UseVisualStyleBackColor = true;
            this.chkDebugSimulasi.CheckedChanged += new System.EventHandler(this.ChkDebugSimulasi_CheckedChanged);
            // 
            // userInfo
            // 
            this.userInfo.AutoSize = true;
            this.userInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.userInfo.Location = new System.Drawing.Point(51, 62);
            this.userInfo.Name = "userInfo";
            this.userInfo.Size = new System.Drawing.Size(343, 21);
            this.userInfo.TabIndex = 13;
            this.userInfo.Text = ".: {{ KodeDc }} - {{ NamaDc }} :: {{ UserName }} :.";
            this.userInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // imgDomar
            // 
            this.imgDomar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.imgDomar.Image = ((System.Drawing.Image)(resources.GetObject("imgDomar.Image")));
            this.imgDomar.Location = new System.Drawing.Point(724, 27);
            this.imgDomar.Name = "imgDomar";
            this.imgDomar.Size = new System.Drawing.Size(51, 56);
            this.imgDomar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgDomar.TabIndex = 14;
            this.imgDomar.TabStop = false;
            this.imgDomar.Click += new System.EventHandler(this.ImgDomar_Click);
            // 
            // prgrssBrStatus
            // 
            this.prgrssBrStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.prgrssBrStatus.ForeColor = System.Drawing.Color.GreenYellow;
            this.prgrssBrStatus.Location = new System.Drawing.Point(575, 52);
            this.prgrssBrStatus.MarqueeAnimationSpeed = 25;
            this.prgrssBrStatus.Name = "prgrssBrStatus";
            this.prgrssBrStatus.Size = new System.Drawing.Size(139, 10);
            this.prgrssBrStatus.Step = 1;
            this.prgrssBrStatus.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.prgrssBrStatus.TabIndex = 15;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(64)))), ((int)(((byte)(129)))));
            this.lblStatus.Location = new System.Drawing.Point(572, 30);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(142, 23);
            this.lblStatus.TabIndex = 16;
            this.lblStatus.Text = "Program {{ Idle }} ...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // appInfo
            // 
            this.appInfo.AutoSize = true;
            this.appInfo.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(133)))), ((int)(((byte)(244)))));
            this.appInfo.Location = new System.Drawing.Point(26, 27);
            this.appInfo.Name = "appInfo";
            this.appInfo.Size = new System.Drawing.Size(342, 30);
            this.appInfo.TabIndex = 17;
            this.appInfo.Text = "{{ BoilerPlate-Net452-WinForm }}";
            this.appInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lnkLblLogClear
            // 
            this.lnkLblLogClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkLblLogClear.AutoSize = true;
            this.lnkLblLogClear.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkLblLogClear.Location = new System.Drawing.Point(184, 483);
            this.lnkLblLogClear.Name = "lnkLblLogClear";
            this.lnkLblLogClear.Size = new System.Drawing.Size(39, 13);
            this.lnkLblLogClear.TabIndex = 0;
            this.lnkLblLogClear.TabStop = true;
            this.lnkLblLogClear.Text = "CLEAR";
            this.lnkLblLogClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkLblLogClear_LinkClicked);
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(64)))), ((int)(((byte)(129)))));
            this.panel5.Location = new System.Drawing.Point(27, 377);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(111, 1);
            this.panel5.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label7.Location = new System.Drawing.Point(23, 352);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 21);
            this.label7.TabIndex = 2;
            this.label7.Text = "Lain - Lain ...";
            // 
            // btnMiscLogErrorProses
            // 
            this.btnMiscLogErrorProses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMiscLogErrorProses.AutoSize = true;
            this.btnMiscLogErrorProses.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnMiscLogErrorProses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMiscLogErrorProses.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnMiscLogErrorProses.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnMiscLogErrorProses.Location = new System.Drawing.Point(30, 424);
            this.btnMiscLogErrorProses.Name = "btnMiscLogErrorProses";
            this.btnMiscLogErrorProses.Size = new System.Drawing.Size(167, 31);
            this.btnMiscLogErrorProses.TabIndex = 3;
            this.btnMiscLogErrorProses.Text = "Proses Error Logs";
            this.btnMiscLogErrorProses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMiscLogErrorProses.UseVisualStyleBackColor = false;
            this.btnMiscLogErrorProses.Click += new System.EventHandler(this.BtnMiscLogErrorProses_Click);
            // 
            // btnMiscLogErrorTransfer
            // 
            this.btnMiscLogErrorTransfer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMiscLogErrorTransfer.AutoSize = true;
            this.btnMiscLogErrorTransfer.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnMiscLogErrorTransfer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMiscLogErrorTransfer.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnMiscLogErrorTransfer.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnMiscLogErrorTransfer.Location = new System.Drawing.Point(30, 387);
            this.btnMiscLogErrorTransfer.Name = "btnMiscLogErrorTransfer";
            this.btnMiscLogErrorTransfer.Size = new System.Drawing.Size(167, 31);
            this.btnMiscLogErrorTransfer.TabIndex = 4;
            this.btnMiscLogErrorTransfer.Text = "Transfer Error Logs";
            this.btnMiscLogErrorTransfer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMiscLogErrorTransfer.UseVisualStyleBackColor = false;
            this.btnMiscLogErrorTransfer.Click += new System.EventHandler(this.BtnMiscLogErrorTransfer_Click);
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(64)))), ((int)(((byte)(129)))));
            this.panel3.Location = new System.Drawing.Point(232, 117);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1, 350);
            this.panel3.TabIndex = 5;
            // 
            // textBoxLogInfo
            // 
            this.textBoxLogInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLogInfo.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBoxLogInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLogInfo.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBoxLogInfo.Location = new System.Drawing.Point(27, 509);
            this.textBoxLogInfo.Multiline = true;
            this.textBoxLogInfo.Name = "textBoxLogInfo";
            this.textBoxLogInfo.ReadOnly = true;
            this.textBoxLogInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLogInfo.Size = new System.Drawing.Size(748, 64);
            this.textBoxLogInfo.TabIndex = 7;
            this.textBoxLogInfo.Text = "// Belum Ada Riwayat Apapun ...";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label4.Location = new System.Drawing.Point(23, 475);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "Event logs";
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(64)))), ((int)(((byte)(129)))));
            this.panel4.Location = new System.Drawing.Point(27, 499);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(196, 1);
            this.panel4.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(64)))), ((int)(((byte)(129)))));
            this.panel2.Location = new System.Drawing.Point(27, 138);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(125, 1);
            this.panel2.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label2.Location = new System.Drawing.Point(23, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 21);
            this.label2.TabIndex = 11;
            this.label2.Text = "Menu Navigasi";
            // 
            // navContent
            // 
            this.navContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.navContent.Controls.Add(this.pictureBox);
            this.navContent.Location = new System.Drawing.Point(248, 128);
            this.navContent.Name = "navContent";
            this.navContent.Size = new System.Drawing.Size(527, 356);
            this.navContent.TabIndex = 18;
            // 
            // pictureBox
            // 
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox.Image")));
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(527, 356);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 19;
            this.pictureBox.TabStop = false;
            // 
            // navMenu
            // 
            this.navMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.navMenu.AutoScroll = true;
            this.navMenu.Location = new System.Drawing.Point(27, 145);
            this.navMenu.Name = "navMenu";
            this.navMenu.Size = new System.Drawing.Size(190, 186);
            this.navMenu.TabIndex = 6;
            // 
            // chkWindowsStartup
            // 
            this.chkWindowsStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkWindowsStartup.AutoSize = true;
            this.chkWindowsStartup.Location = new System.Drawing.Point(575, 68);
            this.chkWindowsStartup.Name = "chkWindowsStartup";
            this.chkWindowsStartup.Size = new System.Drawing.Size(143, 17);
            this.chkWindowsStartup.TabIndex = 19;
            this.chkWindowsStartup.Text = "Run After Windows Start";
            this.chkWindowsStartup.UseVisualStyleBackColor = true;
            this.chkWindowsStartup.CheckedChanged += new System.EventHandler(this.ChkWindowsStartup_CheckedChanged);
            // 
            // CMainPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkWindowsStartup);
            this.Controls.Add(this.navContent);
            this.Controls.Add(this.lnkLblLogClear);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnMiscLogErrorProses);
            this.Controls.Add(this.btnMiscLogErrorTransfer);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.navMenu);
            this.Controls.Add(this.textBoxLogInfo);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkDebugSimulasi);
            this.Controls.Add(this.userInfo);
            this.Controls.Add(this.imgDomar);
            this.Controls.Add(this.prgrssBrStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.appInfo);
            this.Name = "CMainPanel";
            this.Size = new System.Drawing.Size(800, 600);
            this.Load += new System.EventHandler(this.CMainPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.imgDomar)).EndInit();
            this.navContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkDebugSimulasi;
        private System.Windows.Forms.Label userInfo;
        private System.Windows.Forms.PictureBox imgDomar;
        private System.Windows.Forms.ProgressBar prgrssBrStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label appInfo;
        private System.Windows.Forms.LinkLabel lnkLblLogClear;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnMiscLogErrorProses;
        private System.Windows.Forms.Button btnMiscLogErrorTransfer;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox textBoxLogInfo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel navContent;
        private System.Windows.Forms.PictureBox pictureBox;
        private Components.FixAutoScrollFlowLayoutPanel navMenu;
        private System.Windows.Forms.CheckBox chkWindowsStartup;
    }

}
