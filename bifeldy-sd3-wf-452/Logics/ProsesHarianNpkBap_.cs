/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data NPK BAP
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

    public interface IProsesHarianNpkBap : ILogics { }

    public sealed class CProsesHarianNpkBap : CLogics, IProsesHarianNpkBap {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianNpkBap(
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
                    JumlahServerKirimZip = 2;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    /* NPK */

                    CDbExecProcResult res1 = await _db.CALL_NPK_BAP("CREATE_NPK_TEMP", $"{dateStart:MM/dd/yyyy}", $"{dateEnd:MM/dd/yyyy}");
                    if (res1 == null || !res1.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure CREATE_ENDORSMENT_CSV");
                    }

                    string p_msg1 = $"{res1.PARAMETERS["V_RESULT"].Value}";
                    if (!string.IsNullOrEmpty(p_msg1)) {
                        throw new Exception(p_msg1);
                    }

                    await _qTrfCsv.CreateCSVFile("NPK");
                    await _qTrfCsv.CreateCSVFile("NPKHDR");

                    /* BAP */

                    CDbExecProcResult res2 = await _db.CALL_NPK_BAP("CREATE_BAP_TEMP", $"{dateStart:MM/dd/yyyy}", $"{dateEnd:MM/dd/yyyy}");
                    if (res2 == null || !res2.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure CREATE_ENDORSMENT_CSV");
                    }

                    string p_msg2 = $"{res1.PARAMETERS["V_RESULT"].Value}";
                    if (!string.IsNullOrEmpty(p_msg2)) {
                        throw new Exception(p_msg2);
                    }

                    await _qTrfCsv.CreateCSVFile("BAP");
                    await _qTrfCsv.CreateCSVFile("BAPHDR");

                    /* ** */

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "NPKBAP");
                    if (_berkas.ZipListFileInFolder(zipFileName) > 0) {
                        TargetKirim += JumlahServerKirimZip;
                    }

                    BerhasilKirim += await _dcFtpT.KirimFtp("LOCAL", zipFileName: zipFileName); // *.ZIP Sebanyak :: 1
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("NPKBAP", zipFileName: zipFileName); // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
