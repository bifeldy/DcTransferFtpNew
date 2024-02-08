/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data EXP Plano
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianExpPlano : ILogics { }

    public sealed class CProsesHarianExpPlano : CLogics, IProsesHarianExpPlano {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IDcFtpT _dcFtpT;
        private readonly IQTrfCsv _qTrfCsv;

        public CProsesHarianExpPlano(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IDcFtpT dc_ftp_t,
            IQTrfCsv qTrfCsv
        ) : base(db, csv, zip) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _csv = csv;
            _zip = zip;
            _dcFtpT = dc_ftp_t;
            _qTrfCsv = qTrfCsv;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeSameDay()) {
                    _berkas.BackupAllFilesInFolder(_csv.CsvFolderPath);
                    _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, 0);
                    _berkas.BackupAllFilesInFolder(_zip.ZipFolderPath);
                    _berkas.DeleteOldFilesInFolder(_zip.ZipFolderPath, 0);
                    JumlahServerKirimCsv = 1;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    // Hanya Dc Tertentu
                    List<string> allowedJenisDc = new List<string> { "INDUK", "DEPO" };
                    if (!allowedJenisDc.Contains(await _db.GetJenisDc())) {
                        throw new Exception($"{button.Text} Hanya Dapat Di Jalankan Di DC {Environment.NewLine}{string.Join(", ", allowedJenisDc.ToArray())}");
                    }

                    int jmlPluExp = await _db.GetJumlahPluExpired();
                    if (jmlPluExp <= 0) {
                        throw new Exception($"Logistik HO Belum Input PLU Expired");
                    }

                    string procName = await _db.DC_FILE_SCHEDULER_T__GET("FILE_PROCEDURE", "EXPPLANO") ?? "TRF_EXPPLANO_EVO";
                    CDbExecProcResult res = await _db.OraPg_CALL_(procName);
                    if (res == null || !res.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName}");
                    }

                    await _qTrfCsv.CreateCSVFile("EXPPLANO");
                    TargetKirim += JumlahServerKirimCsv;

                    // string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "EXPPLANO");
                    // _zip.ZipListFileInFolder(zipFileName);
                    // TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("WEBREKAP", reportLog: true)).Success.Count; // *.CSV Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
