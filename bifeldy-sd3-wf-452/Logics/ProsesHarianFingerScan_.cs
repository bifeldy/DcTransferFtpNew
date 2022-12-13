/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data Harian
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Navigations;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianFingerScan : ILogics { }

    public sealed class CProsesHarianFingerScan : CLogics, IProsesHarianFingerScan {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianFingerScan(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            CProsesHarian prosesHarian = (CProsesHarian)currentControl;
            Button button = (Button)sender;
            button.BackColor = Color.FromArgb(255, 207, 223);
            DateTime dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date;
            DateTime dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date;
            await Task.Run(async () => {
                string infoMessage = null;
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd) && await IsDateEndYesterday(dateEnd)) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    string fileTimeFingerScanFormat = $"{dateEnd:MMyyyy}";
                    string targetFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteLog(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    targetFileName = $"FINGER{await _db.GetKodeDc()}{fileTimeFingerScanFormat}.csv";
                    (bool success1, bool addQueue1) = await _qTrfCsv.CreateCSVFile(targetFileName, "FINGER");
                    if (success1 && addQueue1) {
                        TargetKirim++;
                    }

                    // string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "FINGER");
                    // int totalFileInZip = _berkas.ZipListFileInTempFolder(zipFileName);

                    BerhasilKirim += await _dcFtpT.KirimFtpFingerScan(); // *.CSV Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
                if (string.IsNullOrEmpty(infoMessage)) {
                    if (BerhasilKirim == 0 || TargetKirim == 0) {
                        infoMessage = $"Ada Masalah, Belum Ada {button.Text} Yang Diproses !!";
                    }
                    else if (BerhasilKirim < TargetKirim && TargetKirim > 0) {
                        infoMessage = $"Ada Beberapa Proses {button.Text} Yang Gagal !!";
                    }
                    else if (BerhasilKirim >= TargetKirim && TargetKirim > 0) {
                        infoMessage = $"{button.Text} Sukses !!";
                    }
                    else {
                        infoMessage = $"{button.Text} Error !!";
                    }
                }
                MessageBox.Show(infoMessage, button.Text);
            });
        }

    }

}
