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

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Navigations;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianDataHarian : ILogics { }

    public sealed class CProsesHarianDataHarian : CLogics, IProsesHarianDataHarian {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianDataHarian(
            IApp app,
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db) {
            _app = app;
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            CProsesHarian prosesHarian = (CProsesHarian) currentControl;
            _logger.ClearLog();
            Button button = (Button) sender;
            button.BackColor = Color.FromArgb(255, 207, 223);
            DateTime dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date;
            DateTime dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date;
            await Task.Run(async () => {
                string infoMessage = null;
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd)) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    string fileTimeBRDFormat2Hariana = $"{dateStart:MM}";
                    string fileTimeBRDFormat2Harianb = $"{dateStart:yyyy}";
                    string targetFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteLog(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        CDbExecProcResult res = await _db.CALL__P_TGL("TRF_STM_BTR_EVO", xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception("Gagal Menjalankan Procedure");
                        }

                        targetFileName = $"STM{fileTimeBRDFormat2Harianb.Substring(3, 1)}{fileTimeBRDFormat2Hariana}{xDate:dd}.CSV";
                        (bool success1, bool addQueue1) = await _qTrfCsv.CreateCSVFile(targetFileName, "STM");
                        if (success1 && addQueue1) {
                            TargetKirim++;
                        }

                        targetFileName = $"HPG{fileTimeBRDFormat2Harianb.Substring(3, 1)}{fileTimeBRDFormat2Hariana}{xDate:dd}.CSV";
                        (bool success2, bool addQueue2) = await _qTrfCsv.CreateCSVFile(targetFileName, "HPG");
                        if (success2 && addQueue2) {
                            TargetKirim++;
                        }
                    }

                    targetFileName = "PRODMAST.CSV";
                    (bool success3, bool addQueue3) = await _qTrfCsv.CreateCSVFile(targetFileName, "PRODMAST");
                    if (success3 && addQueue3) {
                        TargetKirim++;
                    }

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "STM");
                    int totalFileInZip = _berkas.ZipListFileInTempFolder(zipFileName);

                    BerhasilKirim += await _dcFtpT.KirimFtpLocal(); // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("HARIAN", zipFileName, true); // *.ZIP Sebanyak :: 1

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
