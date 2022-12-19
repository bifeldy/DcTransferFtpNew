/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Halaman Awal
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Forms;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Panels {

    public sealed partial class CMainPanel : UserControl {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IMenuNavigations _menuNavigations;

        private CMainForm mainForm;

        public IProgress<string> LogReporter { get; set; } = null;

        public CMainPanel(IApp app, ILogger logger, IDb db, IMenuNavigations menuNavigations) {
            _app = app;
            _logger = logger;
            _db = db;
            _menuNavigations = menuNavigations;

            InitializeComponent();
            OnInit();
        }

        public FlowLayoutPanel NavMenu => navMenu;

        public Panel NavContent => navContent;

        public TextBox LogInfo => textBoxLogInfo;

        private void OnInit() {
            Dock = DockStyle.Fill;

            LogReporter = new Progress<string>(log => {
                textBoxLogInfo.Text += log;
            });
        }

        private void imgDomar_Click(object sender, EventArgs e) {
            //
        }

        private async void CMainPanel_Load(object sender, EventArgs e) {
            mainForm = (CMainForm) Parent.Parent;
            mainForm.FormBorderStyle = FormBorderStyle.Sizable;
            mainForm.MaximizeBox = true;
            mainForm.MinimizeBox = true;

            appInfo.Text = _app.AppName;
            string dcKode = null;
            string namaDc = null;
            await Task.Run(async () => {
                dcKode = await _db.GetKodeDc();
                namaDc = await _db.GetNamaDc();
            });
            userInfo.Text = $".: {dcKode} - {namaDc} :: {_db.LoggedInUsername} :.";

            _menuNavigations.AddButtonToPanel(this);

            _logger.SetReportInfo(LogReporter);

            SetIdleBusyStatus(true);
        }

        public void SetIdleBusyStatus(bool isIdle) {
            lblStatus.Text = $"Program {(isIdle ? "Idle" : "Sibuk")} ...";
            prgrssBrStatus.Style = isIdle ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            EnableDisableControl(Controls, isIdle);
        }

        private void EnableDisableControl(ControlCollection controls, bool isIdle) {
            foreach (Control control in controls) {
                if (control is Button || control is CheckBox || control is DateTimePicker) {
                    control.Enabled = isIdle;
                }
                else {
                    EnableDisableControl(control.Controls, isIdle);
                }
            }
        }

        private void lnkLblLogClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            textBoxLogInfo.Text = string.Empty;
        }

    }

}
