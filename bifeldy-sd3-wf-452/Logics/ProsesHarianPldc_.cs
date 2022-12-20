/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data PLDC
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

    public interface IProsesHarianPldc : ILogics { }

    public sealed class CProsesHarianPldc : CLogics, IProsesHarianPldc {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianPldc(
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

                    string fileTimeBRDFormat2Hariana = $"{dateStart:MM}";
                    string DBFformat = $"{dateStart:MM}";
                    string targetFileName = null;

                    string varDcExt = await _db.GetDcExt();

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "BUAT_PL_DC_EVO";
                        CDbExecProcResult res = await _db.CALL__P_TGL(procName, xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        targetFileName = $"DC{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        if (await _qTrfCsv.CreateCSVFile("DC", targetFileName)) {
                            TargetKirim++;
                        }

                        targetFileName = $"ST{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        if (await _qTrfCsv.CreateCSVFile("ST", targetFileName)) {
                            TargetKirim++;
                        }

                        targetFileName = $"SX{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        if (await _qTrfCsv.CreateCSVFile("SX", targetFileName)) {
                            TargetKirim++;
                        }
                    }

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "DC");
                    int totalFileInZip = _berkas.ZipListFileInFolder(zipFileName);

                    BerhasilKirim += await _dcFtpT.KirimFtp("LOCAL"); // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("PLDC", zipFileName, true); // *.ZIP Sebanyak :: 1
                    BerhasilKirim += await _dcFtpT.KirimFtp("EIS", null, zipFileName); // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
