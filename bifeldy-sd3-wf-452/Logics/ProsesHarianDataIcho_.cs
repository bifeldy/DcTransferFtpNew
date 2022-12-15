/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data ICHO
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

    public interface IProsesHarianDataIcho : ILogics { }

    public sealed class CProsesHarianDataIcho : CLogics, IProsesHarianDataIcho {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianDataIcho(
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
            CProsesHarian prosesHarian = (CProsesHarian)currentControl;
            Button button = (Button) sender;
            button.BackColor = Color.FromArgb(255, 207, 223);
            DateTime dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date;
            DateTime dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date;
            await Task.Run(async () => {
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd)) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    string fileTimeICHOFormat = $"{dateStart:MM}";
                    string targetFileName = null;

                    string fileTimeICHOFormat2 = await _db.GetWinFunction();

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteLog(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "TRF_ICHO_NEW_EVO";
                        CDbExecProcResult res = await _db.CALL__P_TGL(procName, xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        targetFileName = $"PAR{fileTimeICHOFormat}{xDate:dd}G.CSV";
                        (bool success1, bool addQueue1) = await _qTrfCsv.CreateCSVFile(targetFileName, "PAR");
                        if (success1 && addQueue1) {
                            TargetKirim++;
                        }
                    }

                    targetFileName = "SUPMAST.CSV";
                    (bool success2, bool addQueue2) = await _qTrfCsv.CreateCSVFile(targetFileName, "SUPMAST");
                    if (success2 && addQueue2) {
                        TargetKirim++;
                    }

                    targetFileName = "HRGBELI.CSV";
                    (bool success3, bool addQueue3) = await _qTrfCsv.CreateCSVFile(targetFileName, "HRGBELI");
                    if (success3 && addQueue3) {
                        TargetKirim++;
                    }

                    targetFileName = "PROTECT.CSV";
                    (bool success4, bool addQueue4) = await _qTrfCsv.CreateCSVFile(targetFileName, "PROTECT");
                    if (success4 && addQueue4) {
                        TargetKirim++;
                    }

                    targetFileName = $"REG{fileTimeICHOFormat2}.CSV";
                    (bool success5, bool addQueue5) = await _qTrfCsv.CreateCSVFile(targetFileName, "REG");
                    if (success5 && addQueue5) {
                        TargetKirim++;
                    }

                    targetFileName = $"TRNH{fileTimeICHOFormat2}.CSV";
                    (bool success6, bool addQueue6) = await _qTrfCsv.CreateCSVFile(targetFileName, "TRNH");
                    if (success6 && addQueue6) {
                        TargetKirim++;
                    }

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "TRNH");
                    int totalFileInZip = _berkas.ZipListFileInTempFolder(zipFileName);

                    BerhasilKirim += await _dcFtpT.KirimFtpLocal(); // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("ICHO", zipFileName, true); // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman(button.Text);
        }

    }

}
