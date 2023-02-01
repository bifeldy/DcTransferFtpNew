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
        private readonly IMenuNavigations _menuNavigations;

        private CMainForm mainForm;

        public IProgress<string> LogInfoReporter { get; set; } = null;

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

            LogInfoReporter = new Progress<string>(log => {
                textBoxLogInfo.Text = log + textBoxLogInfo.Text;
            });

            chkDebugSimulasi.Checked = _app.DebugMode;
        }

        private void ImgDomar_Click(object sender, EventArgs e) {
            List<Control> ctrls = new List<Control>();
            foreach(Control ctrl in NavContent.Controls) {
                if (ctrl is CNavigations) {
                    ctrls.Add(ctrl);
                }
            }
            foreach(Control ctrl in ctrls) {
                NavContent.Controls.Remove(ctrl);
            }
            foreach (Control navMenuItem in NavMenu.Controls) {
                if (navMenuItem is Button) {
                    navMenuItem.BackColor = SystemColors.ControlLight;
                }
            }
            LnkLblLogClear_LinkClicked(sender, EventArgs.Empty);
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

            _logger.SetReportInfo(LogInfoReporter);

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

        private void LnkLblLogClear_LinkClicked(object sender, EventArgs e) {
            textBoxLogInfo.Text = string.Empty;
        }

        private async void ChkDebugSimulasi_CheckedChanged(object sender, EventArgs e) {
            _app.DebugMode = chkDebugSimulasi.Checked;
            await Task.Run(() => {
                if (_app.DebugMode) {
                    MessageBox.Show(
                        "File Yang Di Kirim Akan Memiliki Prefix _SIMULASI__filename.ext",
                        "Mode Debug &/ Simulasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            });
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

    }

}
