/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian REGSTO02
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

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianRegSto02 : ILogics { }

    public sealed class CProsesHarianRegSto02 : CLogics, IProsesHarianRegSto02 {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianRegSto02(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db, berkas) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                JumlahServerKirimCsv = 2;

                int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                string procName = "TRF_REGSTO02_EVO";
                CDbExecProcResult res = await _db.OraPg_CALL_(procName);
                if (res == null || !res.STATUS) {
                    throw new Exception($"Gagal Menjalankan Procedure {procName}");
                }

                await _qTrfCsv.CreateCSVFile("REG02");
                TargetKirim += JumlahServerKirimCsv;

                // string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "REG02");
                // _berkas.ZipListFileInFolder(zipFileName);
                // TargetKirim += JumlahServerKirimZip;

                BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; ; // *.CSV Sebanyak :: TargetKirim
                BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauZipFtpDev("REGSTO02")).Success.Count; // *.CSV Sebanyak :: TargetKirim

                _berkas.CleanUp();
            });
            CheckHasilKiriman();
        }

    }

}
