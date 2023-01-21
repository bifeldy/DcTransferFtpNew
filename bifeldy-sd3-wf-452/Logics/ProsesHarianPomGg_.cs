/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data POMGG
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
using DcTransferFtpNew.Navigations;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianPomGg : ILogics { }

    public sealed class CProsesHarianPomGg : CLogics, IProsesHarianPomGg {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IDcFtpT _dcFtpT;
        private readonly IQTrfCsv _qTrfCsv;

        public CProsesHarianPomGg(
            IApp app,
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IDcFtpT dc_ftp_t,
            IQTrfCsv qTrfCsv
        ) : base(db, berkas) {
            _app = app;
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _dcFtpT = dc_ftp_t;
            _qTrfCsv = qTrfCsv;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);

            CProsesHarian prosesHarian = (CProsesHarian) currentControl;
            string usingBulanG = prosesHarian.ChkPomgg.Checked ? "Y" : "N";

            await Task.Run(async () => {
                if (IsDateRangeSameDay()) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    string procName = "TRF_POMGG_EVO";
                    CDbExecProcResult res = await _db.CALL_ICHO(procName, dateStart, usingBulanG);
                    if (res == null || !res.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName}");
                    }

                    string csvFileName = "POMGG.CSV";
                    await _qTrfCsv.CreateCSVFile("POMGG", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "POMGG");
                    _berkas.ZipListFileInFolder(zipFileName);
                    TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauZipFtpDev("POMGG", zipFileName, true)).Success.Count; // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });

            CheckHasilKiriman();
        }

    }

}
