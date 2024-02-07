/**
 * 
 * Author       :: Basilius Bias Astho Christyono
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
using System.Diagnostics;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;

namespace DcTransferFtpNew.Navigations {

    public sealed partial class CSettings : CNavigations {

        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IConfig _config;

        public CSettings(
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IConfig config
        ) {
            _berkas = berkas;
            _csv = csv;
            _zip = zip;
            _config = config;

            InitializeComponent();
            OnInit();
        }

        private void OnInit() {
            Dock = DockStyle.Fill;

            txtBxDaysRetentionFiles.Value = _berkas.MaxOldRetentionDay;
            txtBxOpenFolderTempCsv.Text = _csv.CsvFolderPath;
            txtBxOpenFolderZip.Text = _zip.ZipFolderPath;
            txtBxOpenFolderBackup.Text = _berkas.BackupFolderPath;
        }

        private void TxtBxDaysRetentionFiles_ValueChanged(object sender, EventArgs e) {
            _berkas.MaxOldRetentionDay = (int) txtBxDaysRetentionFiles.Value;
            _config.Set("MaxOldRetentionDay", _berkas.MaxOldRetentionDay);
        }

        private void BtnOpenFolderTempCsv_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo { Arguments = _csv.CsvFolderPath, FileName = "explorer.exe" });
        }

        private void BtnClearFolderTempCsv_Click(object sender, EventArgs e) {
            DialogResult dr = MessageBox.Show("Yakin Ingin Menghapus CSV ?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes) {
                _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, (int) txtBxDaysRetentionFiles.Value);
            }
        }

        private void BtnOpenFolderZip_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo { Arguments = _zip.ZipFolderPath, FileName = "explorer.exe" });
        }

        private void BtnClearFolderZip_Click(object sender, EventArgs e) {
            DialogResult dr = MessageBox.Show("Yakin Ingin Menghapus Zip ?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes) {
                _berkas.DeleteOldFilesInFolder(_zip.ZipFolderPath, (int) txtBxDaysRetentionFiles.Value);
            }
        }

        private void BtnOpenFolderBackup_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo { Arguments = _berkas.BackupFolderPath, FileName = "explorer.exe" });
        }

        private void BtnClearFolderBackup_Click(object sender, EventArgs e) {
            DialogResult dr = MessageBox.Show("Yakin Ingin Menghapus Backup ?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes) {
                _berkas.DeleteOldFilesInFolder(_berkas.BackupFolderPath, (int) txtBxDaysRetentionFiles.Value);
            }
        }

    }

}
