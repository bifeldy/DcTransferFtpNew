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

        public CMainPanel(IApp app, ILogger logger, IDb db, IMenuNavigations menuNavigations) {
            _app = app;
            _logger = logger;
            _db = db;
            _menuNavigations = menuNavigations;

            InitializeComponent();
            OnInit();
        }

        public Label LabelStatus => lblStatus;

        public ProgressBar ProgressBarStatus => prgrssBrStatus;

        public FlowLayoutPanel NavMenu => navMenu;

        public Panel NavContent => navContent;

        private void OnInit() {
            Dock = DockStyle.Fill;
        }

        private void imgDomar_Click(object sender, EventArgs e) {
            mainForm.Width = 800;
            mainForm.Height = 600;
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

            SetIdleBusyStatus(true);
        }

        public void SetIdleBusyStatus(bool isIdle) {
            LabelStatus.Text = $"Program {(isIdle ? "Idle" : "Sibuk")} ...";
            ProgressBarStatus.Style = isIdle ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
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

        private void logUpdater_Tick(object sender, EventArgs e) {
            if (textBoxLogInfo.Text != _logger.LogInfo) {
                textBoxLogInfo.Text = _logger.LogInfo;
            }
        }

        private void lnkLblLogClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            _logger.ClearLog();
        }

    }

}
