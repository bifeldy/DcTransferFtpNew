﻿
namespace DcTransferFtpNew.Navigations {

    public sealed partial class CProsesBulanan {
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
            this.panelProsesBulanan = new System.Windows.Forms.Panel();
            this.dtpBulanan = new DcTransferFtpNew.Components.MonthYearPicker();
            this.label5 = new System.Windows.Forms.Label();
            this.flowLayoutPanelProsesBulanan = new DcTransferFtpNew.Components.FixAutoScrollFlowLayoutPanel();
            this.panelProsesBulanan.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelProsesBulanan
            // 
            this.panelProsesBulanan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelProsesBulanan.Controls.Add(this.dtpBulanan);
            this.panelProsesBulanan.Controls.Add(this.label5);
            this.panelProsesBulanan.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProsesBulanan.Location = new System.Drawing.Point(0, 0);
            this.panelProsesBulanan.Name = "panelProsesBulanan";
            this.panelProsesBulanan.Size = new System.Drawing.Size(527, 75);
            this.panelProsesBulanan.TabIndex = 21;
            // 
            // dtpBulanan
            // 
            this.dtpBulanan.CustomFormat = "MMMM yyyy";
            this.dtpBulanan.Location = new System.Drawing.Point(13, 35);
            this.dtpBulanan.Name = "dtpBulanan";
            this.dtpBulanan.Size = new System.Drawing.Size(207, 20);
            this.dtpBulanan.TabIndex = 19;
            this.dtpBulanan.ValueChanged += new System.EventHandler(this.DtpBulanan_ValueChanged);
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label5.Location = new System.Drawing.Point(13, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(448, 23);
            this.label5.TabIndex = 15;
            this.label5.Text = "Periode Proses Bulanan";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanelProsesBulanan
            // 
            this.flowLayoutPanelProsesBulanan.AutoScroll = true;
            this.flowLayoutPanelProsesBulanan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelProsesBulanan.Location = new System.Drawing.Point(0, 75);
            this.flowLayoutPanelProsesBulanan.Name = "flowLayoutPanelProsesBulanan";
            this.flowLayoutPanelProsesBulanan.Size = new System.Drawing.Size(527, 281);
            this.flowLayoutPanelProsesBulanan.TabIndex = 22;
            // 
            // CProsesBulanan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanelProsesBulanan);
            this.Controls.Add(this.panelProsesBulanan);
            this.Name = "CProsesBulanan";
            this.Size = new System.Drawing.Size(527, 356);
            this.Load += new System.EventHandler(this.CProsesBulanan_Load);
            this.panelProsesBulanan.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelProsesBulanan;
        private System.Windows.Forms.Label label5;
        private Components.MonthYearPicker dtpBulanan;
        private Components.FixAutoScrollFlowLayoutPanel flowLayoutPanelProsesBulanan;
    }

}
