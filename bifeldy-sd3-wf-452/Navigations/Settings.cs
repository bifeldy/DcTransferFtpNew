/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: UI Navigation Transfer Settings
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;

namespace DcTransferFtpNew.Navigations {

    public sealed partial class CSettings : CNavigations {

        private readonly IBerkas _berkas;
        private readonly IConfig _config;

        public CSettings(
            IBerkas berkas,
            IConfig config
        ) {
            _berkas = berkas;
            _config = config;

            InitializeComponent();
            OnInit();
        }

        private void OnInit() {
            Dock = DockStyle.Fill;

            txtBxDaysRetentionFiles.Value = _berkas.MaxOldRetentionDay;
            txtBxOpenFolderTempCsv.Text = _berkas.TempFolderPath;
            txtBxOpenFolderZip.Text = _berkas.ZipFolderPath;
            txtBxOpenFolderBackup.Text = _berkas.BackupFolderPath;
        }

        private void TxtBxDaysRetentionFiles_ValueChanged(object sender, EventArgs e) {
            _berkas.MaxOldRetentionDay = (int) txtBxDaysRetentionFiles.Value;
            _config.Set("MaxOldRetentionDay", _berkas.MaxOldRetentionDay);
        }

        private void BtnOpenFolderTempCsv_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo { Arguments = _berkas.TempFolderPath, FileName = "explorer.exe" });
        }

        private void BtnClearFolderTempCsv_Click(object sender, EventArgs e) {
            _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, (int) txtBxDaysRetentionFiles.Value);
        }

        private void BtnOpenFolderZip_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo { Arguments = _berkas.ZipFolderPath, FileName = "explorer.exe" });
        }

        private void BtnClearFolderZip_Click(object sender, EventArgs e) {
            _berkas.DeleteOldFilesInFolder(_berkas.ZipFolderPath, (int) txtBxDaysRetentionFiles.Value);
        }

        private void BtnOpenFolderBackup_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo { Arguments = _berkas.BackupFolderPath, FileName = "explorer.exe" });
        }

        private void BtnClearFolderBackup_Click(object sender, EventArgs e) {
            _berkas.DeleteOldFilesInFolder(_berkas.BackupFolderPath, (int) txtBxDaysRetentionFiles.Value);
        }

    }

}
