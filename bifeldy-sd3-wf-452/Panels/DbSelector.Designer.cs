
namespace DcTransferFtpNew.Panels {

    public sealed partial class CDbSelector {
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
            this.btnPostgre = new System.Windows.Forms.Button();
            this.btnOracle = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPostgre
            // 
            this.btnPostgre.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPostgre.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPostgre.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnPostgre.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnPostgre.Location = new System.Drawing.Point(308, 38);
            this.btnPostgre.Name = "btnPostgre";
            this.btnPostgre.Size = new System.Drawing.Size(104, 54);
            this.btnPostgre.TabIndex = 0;
            this.btnPostgre.Text = "Postgre";
            this.btnPostgre.UseVisualStyleBackColor = true;
            this.btnPostgre.Click += new System.EventHandler(this.btnPostgre_Click);
            // 
            // btnOracle
            // 
            this.btnOracle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOracle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOracle.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnOracle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOracle.Location = new System.Drawing.Point(188, 38);
            this.btnOracle.Name = "btnOracle";
            this.btnOracle.Size = new System.Drawing.Size(104, 54);
            this.btnOracle.TabIndex = 1;
            this.btnOracle.Text = "Oracle";
            this.btnOracle.UseVisualStyleBackColor = true;
            this.btnOracle.Click += new System.EventHandler(this.btnOracle_Click);
            // 
            // CDbSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnPostgre);
            this.Controls.Add(this.btnOracle);
            this.Name = "CDbSelector";
            this.Size = new System.Drawing.Size(600, 130);
            this.Load += new System.EventHandler(this.CDbSelector_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPostgre;
        private System.Windows.Forms.Button btnOracle;
    }

}
