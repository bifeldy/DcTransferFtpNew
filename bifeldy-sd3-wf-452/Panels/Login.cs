/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Login User
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

    public sealed partial class CLogin : UserControl {

        private readonly IApp _app;
        private readonly IDb _db;

        private CMainForm mainForm;

        public CLogin(IApp app, IDb db) {
            _app = app;
            _db = db;

            InitializeComponent();
            OnInit();
        }

        private void OnInit() {
            Dock = DockStyle.Fill;
        }

        private void CLogin_Load(object sender, EventArgs e) {
            mainForm = (CMainForm) Parent.Parent;

            ((CCekProgram) mainForm.PanelContainer.Controls["CCekProgram"]).LoadingInformation.Text = "Harap Menunggu ...";
        }

        private void ShowLoading(bool isShow) {

            // Set State To Loading
            ToggleEnableDisableInput(!isShow);
            if (isShow) {
                mainForm.PanelContainer.Controls["CCekProgram"].BringToFront();
            }
            else {
                BringToFront();
            }
        }

        private void ToggleEnableDisableInput(bool isEnable) {

            // Enable / Disable View
            btnLogin.Enabled = isEnable;
            txtUserNameNik.Enabled = isEnable;
            txtPassword.Enabled = isEnable;
        }

        private void ShowMainPanel() {

            // Change Window Size & Position To Middle Screen
            mainForm.Width = 800;
            mainForm.Height = 600;
            mainForm.SetDesktopLocation((_app.ScreenWidth - mainForm.Width) / 2, (_app.ScreenHeight - mainForm.Height) / 2);

            // Change Panel To Fully Windowed Mode And Go To `MainPanel`
            mainForm.HideLogo();
            mainForm.PanelContainer.Dock = DockStyle.Fill;

            // Create & Show `MainPanel`
            try {
                if (!mainForm.PanelContainer.Controls.ContainsKey("CMainPanel")) {
                    mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.ResolveClass<CMainPanel>());
                }
                mainForm.PanelContainer.Controls["CMainPanel"].BringToFront();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Remove All Other Unused Panels
            mainForm.PanelContainer.Controls.RemoveByKey("CDbSelector");
            mainForm.PanelContainer.Controls.RemoveByKey("CCekProgram");
            mainForm.PanelContainer.Controls.RemoveByKey("CLogin");
        }

        private async void ProcessLogin() {

            // Disable View While Loading
            ShowLoading(true);

            // Check User Input
            if (string.IsNullOrEmpty(txtUserNameNik.Text) || string.IsNullOrEmpty(txtPassword.Text)) {
                ShowLoading(false);
                MessageBox.Show("Username / NIK Dan Kata Sandi Wajib Diisi!", "User Authentication", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Login Check Credential
            bool resultLogin = false;
            await Task.Run(async () => {
                resultLogin = await _db.LoginUser(txtUserNameNik.Text, txtPassword.Text);
            });
            if (!resultLogin) {
                ShowLoading(false);
                MessageBox.Show("Login Gagal, Kredensial Salah!", "User Authentication", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {

                // Check IP / MAC
                bool resultCekIpMac = false;
                await Task.Run(async () => {
                    resultCekIpMac = await _db.CheckIpMac();
                });
                if (!resultCekIpMac) {
                    ShowLoading(false);
                    MessageBox.Show("Alamat IP / MAC Tidak Terdaftar!", "User Authentication", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    ShowMainPanel();
                }
            }
        }

        private void checkKeyboard(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Enter:
                    ProcessLogin();
                    break;
                default:
                    break;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e) {
            ProcessLogin();
        }

        private void txt_KeyDown(object sender, KeyEventArgs e) {
            checkKeyboard(sender, e);
        }

    }

}
