
namespace DcTransferFtpNew.Navigations {

    public sealed partial class CProsesHarian {
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
            this.flowLayoutPanelProsesHarian = new System.Windows.Forms.FlowLayoutPanel();
            this.panelProsesHarian = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.dateTimePickerHarianAkhir = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerHarianAwal = new System.Windows.Forms.DateTimePicker();
            this.panelProsesHarian.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanelProsesHarian
            // 
            this.flowLayoutPanelProsesHarian.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanelProsesHarian.AutoScroll = true;
            this.flowLayoutPanelProsesHarian.Location = new System.Drawing.Point(0, 86);
            this.flowLayoutPanelProsesHarian.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelProsesHarian.Name = "flowLayoutPanelProsesHarian";
            this.flowLayoutPanelProsesHarian.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.flowLayoutPanelProsesHarian.Size = new System.Drawing.Size(527, 270);
            this.flowLayoutPanelProsesHarian.TabIndex = 18;
            // 
            // panelProsesHarian
            // 
            this.panelProsesHarian.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelProsesHarian.Controls.Add(this.label3);
            this.panelProsesHarian.Controls.Add(this.label5);
            this.panelProsesHarian.Controls.Add(this.checkBox1);
            this.panelProsesHarian.Controls.Add(this.dateTimePickerHarianAkhir);
            this.panelProsesHarian.Controls.Add(this.dateTimePickerHarianAwal);
            this.panelProsesHarian.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProsesHarian.Location = new System.Drawing.Point(0, 0);
            this.panelProsesHarian.Name = "panelProsesHarian";
            this.panelProsesHarian.Size = new System.Drawing.Size(527, 88);
            this.panelProsesHarian.TabIndex = 19;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label3.Location = new System.Drawing.Point(226, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 30);
            this.label3.TabIndex = 17;
            this.label3.Text = "-";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(81)))), ((int)(((byte)(181)))));
            this.label5.Location = new System.Drawing.Point(13, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(448, 23);
            this.label5.TabIndex = 15;
            this.label5.Text = "Tanggal Proses Harian";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(18, 62);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(131, 17);
            this.checkBox1.TabIndex = 16;
            this.checkBox1.Text = "Data POMGG 3 Bulan";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // dateTimePickerHarianAkhir
            // 
            this.dateTimePickerHarianAkhir.Location = new System.Drawing.Point(254, 35);
            this.dateTimePickerHarianAkhir.Name = "dateTimePickerHarianAkhir";
            this.dateTimePickerHarianAkhir.Size = new System.Drawing.Size(207, 20);
            this.dateTimePickerHarianAkhir.TabIndex = 9;
            // 
            // dateTimePickerHarianAwal
            // 
            this.dateTimePickerHarianAwal.Location = new System.Drawing.Point(13, 35);
            this.dateTimePickerHarianAwal.Name = "dateTimePickerHarianAwal";
            this.dateTimePickerHarianAwal.Size = new System.Drawing.Size(207, 20);
            this.dateTimePickerHarianAwal.TabIndex = 10;
            this.dateTimePickerHarianAwal.Value = new System.DateTime(2022, 12, 21, 0, 0, 0, 0);
            this.dateTimePickerHarianAwal.ValueChanged += new System.EventHandler(this.dateTimePickerHarianAwal_ValueChanged);
            // 
            // CProsesHarian
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelProsesHarian);
            this.Controls.Add(this.flowLayoutPanelProsesHarian);
            this.Name = "CProsesHarian";
            this.Size = new System.Drawing.Size(527, 356);
            this.Load += new System.EventHandler(this.CProsesHarian_Load);
            this.panelProsesHarian.ResumeLayout(false);
            this.panelProsesHarian.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelProsesHarian;
        private System.Windows.Forms.Panel panelProsesHarian;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.DateTimePicker dateTimePickerHarianAkhir;
        private System.Windows.Forms.DateTimePicker dateTimePickerHarianAwal;
    }

}
