/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Database Selector
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Windows.Forms;

using DcTransferFtpNew.Forms;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Panels {

    public sealed partial class CDbSelector : UserControl {

        private readonly IApp _app;

        private CMainForm mainForm;

        public CDbSelector(IApp app) {
            _app = app;

            InitializeComponent();
            OnInit();
        }

        private void OnInit() {
            Dock = DockStyle.Fill;
        }

        private void CDbSelector_Load(object sender, EventArgs e) {
            mainForm = (CMainForm) Parent.Parent;
        }

        private void ShowCheckProgramPanel() {
            btnOracle.Enabled = false;
            btnPostgre.Enabled = false;
            mainForm.Text = $"[{(_app.IsUsingPostgres ? "PG" : "ORCL")}+MSSQL] " + mainForm.Text;

            // Create & Show `CekProgram` Panel
            try {
                if (!mainForm.PanelContainer.Controls.ContainsKey("CCekProgram")) {
                    mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.ResolveClass<CCekProgram>());
                }
                mainForm.PanelContainer.Controls["CCekProgram"].BringToFront();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOracle_Click(object sender, EventArgs e) {
            _app.IsUsingPostgres = false;
            ShowCheckProgramPanel();
        }

        private void BtnPostgre_Click(object sender, EventArgs e) {
            _app.IsUsingPostgres = true;
            ShowCheckProgramPanel();
        }

        public void DchoOnlyBypass(object sender, EventArgs e) {
            BtnOracle_Click(sender, e);
        }

    }

}
