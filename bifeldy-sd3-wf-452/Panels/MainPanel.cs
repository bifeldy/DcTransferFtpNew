/**
 * 
 * Author       :: Basilius Bias Astho Christyono
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
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Forms;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;
using Microsoft.Reporting.WinForms;

namespace DcTransferFtpNew.Panels {

    public sealed partial class CMainPanel : UserControl {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IConfig _config;
        private readonly IWinReg _winreg;
        private readonly IMenuNavigations _menuNavigations;

        private CMainForm mainForm;

        public IProgress<string> LogReporter { get; set; } = null;

        public CMainPanel(
            IApp app,
            ILogger logger,
            IDb db,
            IConfig config,
            IWinReg winreg,
            IMenuNavigations menuNavigations
        ) {
            _app = app;
            _logger = logger;
            _db = db;
            _config = config;
            _winreg = winreg;
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
                textBoxLogInfo.Text = log + textBoxLogInfo.Text;
            });
        }

        private void ImgDomar_Click(object sender, EventArgs e) {
            if (_app.IsIdle) {
                List<Control> ctrls = new List<Control>();
                foreach (Control ctrl in NavContent.Controls) {
                    if (ctrl is CNavigations) {
                        ctrls.Add(ctrl);
                    }
                }
                foreach (Control ctrl in ctrls) {
                    NavContent.Controls.Remove(ctrl);
                }
                foreach (Control navMenuItem in NavMenu.Controls) {
                    if (navMenuItem is Button) {
                        navMenuItem.BackColor = SystemColors.ControlLight;
                    }
                }
                LnkLblLogClear_LinkClicked(sender, EventArgs.Empty);
            }
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

            bool windowsStartup = _config.Get<bool>("WindowsStartup", bool.Parse(_app.GetConfig("windows_startup")));
            chkWindowsStartup.Checked = windowsStartup;

            _menuNavigations.AddButtonToPanel(this);

            _logger.SetLogReporter(LogReporter);

            chkDebugSimulasi.Checked = _app.DebugMode;
            #if !DEBUG
                chkDebugSimulasi.Enabled = false;
            #endif

            SetIdleBusyStatus(true);
        }

        public void SetIdleBusyStatus(bool isIdle) {
            _app.IsIdle = isIdle;
            lblStatus.Text = $"Program {(isIdle ? "Idle" : "Sibuk")} ...";
            prgrssBrStatus.Style = isIdle ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            EnableDisableControl(Controls);
        }

        private void EnableDisableControl(ControlCollection controls) {
            foreach (Control control in controls) {
                if (control is Button || control is CheckBox || control is DateTimePicker) {
                    if (control.Name != chkDebugSimulasi.Name) {
                        control.Enabled = _app.IsIdle;
                    }
                }
                else {
                    EnableDisableControl(control.Controls);
                }
            }
        }

        private void LnkLblLogClear_LinkClicked(object sender, EventArgs e) {
            textBoxLogInfo.Text = string.Empty;
        }

        private void ChkDebugSimulasi_CheckedChanged(object sender, EventArgs e) {
            _app.DebugMode = chkDebugSimulasi.Checked;
            if (_app.DebugMode) {
                MessageBox.Show(
                    "File Yang Di Kirim Akan Memiliki Prefix _SIMULASI__filename.ext",
                    "Mode Debug &/ Simulasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private async void BtnMiscLogErrorTransfer_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            CReportLaporan logError = new CReportLaporan();
            DataTable dt = null;
            List<ReportParameter> rps = new List<ReportParameter> {
                new ReportParameter("parTglPrint", $"{DateTime.Now}")
            };
            await Task.Run(async () => {
                dt = await _db.GetLogErrorTransfer();
            });
            if (logError.SetLaporan(dt, rps, "Rdlcs/Transfer.rdlc", "DCTransferFTP_DS_DCTransferFTP_DT")) {
                logError.Show();
            }
            SetIdleBusyStatus(true);
        }

        private async void BtnMiscLogErrorProses_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            CReportLaporan logError = new CReportLaporan();
            DataTable dt = null;
            List<ReportParameter> rps = new List<ReportParameter> {
                new ReportParameter("parTglPrint2", $"{DateTime.Now}")
            };
            await Task.Run(async () => {
                dt = await _db.GetLogErrorProses();
            });
            if (logError.SetLaporan(dt, rps, "Rdlcs/Proses.rdlc", "DCTransferFTP_DS_DCTransferFTP_DT2")) {
                logError.Show();
            }
            SetIdleBusyStatus(true);
        }

        private void ChkWindowsStartup_CheckedChanged(object sender, EventArgs e) {
            CheckBox cb = (CheckBox) sender;
            _config.Set("WindowsStartup", cb.Checked);
            _winreg.SetWindowsStartup(cb.Checked);
        }

    }

}
