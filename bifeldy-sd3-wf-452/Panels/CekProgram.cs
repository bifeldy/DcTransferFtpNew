/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Cek Versi, IP, etc.
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using DcTransferFtpNew.Forms;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Panels {

    public sealed partial class CCekProgram : UserControl {

        private readonly IApp _app;
        private readonly IDb _db;

        private CMainForm mainForm;

        public CCekProgram(IApp app, IDb db) {
            _app = app;
            _db = db;

            InitializeComponent();
            OnInit();
        }

        public Label LoadingInformation => loadingInformation;

        private void OnInit() {
            loadingInformation.Text = "Sedang Mengecek Program ...";
            Dock = DockStyle.Fill;
        }

        private void CCekProgram_Load(object sender, EventArgs e) {
            mainForm = (CMainForm) Parent.Parent;
            CheckProgram();
        }

        private async void CheckProgram() {
            mainForm.StatusStripContainer.Items["statusStripDbName"].Text = _db.DbName;

            // Check Jenis DC
            bool allowed = false;
            await Task.Run(async () => {
                allowed = _app.ListDcCanUse.Contains(await _db.GetJenisDc());
            });
            if (allowed) {

                // Check Version
                string responseCekProgram = null;
                await Task.Run(async () => {
                    responseCekProgram = await _db.CekVersi();
                });
                if (responseCekProgram == "OKE") {
                    ShowLoginPanel();
                    return;
                }
                else {
                    MessageBox.Show(responseCekProgram, "Program Checker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else {
                MessageBox.Show(
                    $"Program Hanya Dapat Di Jalankan Di DC {Environment.NewLine}{string.Join(", ", _app.ListDcCanUse.ToArray())}",
                    "Program Checker",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            _app.Exit();
        }

        private void ShowLoginPanel() {

            // Create & Show `Login` Panel
            try {
                if (!mainForm.PanelContainer.Controls.ContainsKey("CLogin")) {
                    mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.ResolveClass<CLogin>());
                }
                mainForm.PanelContainer.Controls["CLogin"].BringToFront();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

}
