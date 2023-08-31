/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Tax Non-FAD
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

    public interface IProsesHarianTaxNonFad : ILogics { }

    public sealed class CProsesHarianTaxNonFad : CLogics, IProsesHarianTaxNonFad {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;

        public CProsesHarianTaxNonFad(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv
        ) : base(db, berkas) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid() && IsDateRangeSameMonth() && await IsDateEndMaxYesterday()) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);

                    string csvFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "CREATE_TAXTEMP1_EVO";
                        CDbExecProcResult res = await _db.CALL__P_TGL(procName, xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        await _qTrfCsv.CreateCSVFile("TAX2", csvFileName, appendTargetName: "_NONFAD");
                        // TargetKirim += JumlahServerKirimCsv;
                    }

                    // string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "TAX2");
                    // _berkas.ZipListFileInFolder(zipFileName);
                    // TargetKirim += JumlahServerKirimZip;

                    // // Tidak Ada Kirim File
                    // BerhasilKirim += await _dcFtpT.KirimAllCsv("LOCAL"); // *.CSV Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
