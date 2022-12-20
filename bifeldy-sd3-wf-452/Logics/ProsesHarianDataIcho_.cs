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
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
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
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd)) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    string fileTimeICHOFormat = $"{dateStart:MM}";
                    string targetFileName = null;

                    string fileTimeICHOFormat2 = await _db.GetWinFunction();

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "TRF_ICHO_NEW_EVO";
                        CDbExecProcResult res = await _db.CALL__P_TGL(procName, xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        targetFileName = $"PAR{fileTimeICHOFormat}{xDate:dd}G.CSV";
                        if (await _qTrfCsv.CreateCSVFile(targetFileName, "PAR")) {
                            TargetKirim++;
                        }
                    }

                    targetFileName = "SUPMAST.CSV";
                    if (await _qTrfCsv.CreateCSVFile(targetFileName, "SUPMAST")) {
                        TargetKirim++;
                    }

                    targetFileName = "HRGBELI.CSV";
                    if (await _qTrfCsv.CreateCSVFile(targetFileName, "HRGBELI")) {
                        TargetKirim++;
                    }

                    targetFileName = "PROTECT.CSV";
                    if (await _qTrfCsv.CreateCSVFile(targetFileName, "PROTECT")) {
                        TargetKirim++;
                    }

                    targetFileName = $"REG{fileTimeICHOFormat2}.CSV";
                    if (await _qTrfCsv.CreateCSVFile(targetFileName, "REG")) {
                        TargetKirim++;
                    }

                    targetFileName = $"TRNH{fileTimeICHOFormat2}.CSV";
                    if (await _qTrfCsv.CreateCSVFile(targetFileName, "TRNH")) {
                        TargetKirim++;
                    }

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "TRNH");
                    int totalFileInZip = _berkas.ZipListFileInFolder(zipFileName);

                    BerhasilKirim += await _dcFtpT.KirimFtp("LOCAL"); // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("ICHO", zipFileName, true); // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
