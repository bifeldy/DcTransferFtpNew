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

    public interface IProsesHarianIcho : ILogics { }

    public sealed class CProsesHarianIcho : CLogics, IProsesHarianIcho {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianIcho(
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
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

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
                        await _qTrfCsv.CreateCSVFile("PAR", targetFileName);
                        TargetKirim += JumlahServerKirimCsv;
                    }

                    targetFileName = "SUPMAST.CSV";
                    await _qTrfCsv.CreateCSVFile("SUPMAST", targetFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    targetFileName = "HRGBELI.CSV";
                    await _qTrfCsv.CreateCSVFile("HRGBELI", targetFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    targetFileName = "PROTECT.CSV";
                    await _qTrfCsv.CreateCSVFile("PROTECT", targetFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    targetFileName = $"REG{fileTimeICHOFormat2}.CSV";
                    await _qTrfCsv.CreateCSVFile("REG", targetFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    targetFileName = $"TRNH{fileTimeICHOFormat2}.CSV";
                    await _qTrfCsv.CreateCSVFile("TRNH", targetFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "TRNH");
                    _berkas.ZipListFileInFolder(zipFileName);
                    TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += await _dcFtpT.KirimAllCsvOrZip("LOCAL"); // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("ICHO", zipFileName, true); // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
