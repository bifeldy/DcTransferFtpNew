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
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
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
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd)) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

                    string fileTimeBRDFormat2Hariana = $"{dateStart:MM}";
                    string fileTimeBRDFormat2Harianb = $"{dateStart:yyyy}";
                    string targetFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "TRF_STM_BTR_EVO";
                        CDbExecProcResult res = await _db.CALL__P_TGL(procName, xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        targetFileName = $"STM{fileTimeBRDFormat2Harianb.Substring(3, 1)}{fileTimeBRDFormat2Hariana}{xDate:dd}.CSV";
                        if (await _qTrfCsv.CreateCSVFile("STM", targetFileName)) {
                            TargetKirim += JumlahServerKirimCsv;
                        }

                        targetFileName = $"HPG{fileTimeBRDFormat2Harianb.Substring(3, 1)}{fileTimeBRDFormat2Hariana}{xDate:dd}.CSV";
                        if (await _qTrfCsv.CreateCSVFile("HPG", targetFileName)) {
                            TargetKirim += JumlahServerKirimCsv;
                        }
                    }

                    targetFileName = "PRODMAST.CSV";
                    if (await _qTrfCsv.CreateCSVFile("PRODMAST", targetFileName)) {
                        TargetKirim += JumlahServerKirimCsv;
                    }

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "STM");
                    if (_berkas.ZipListFileInFolder(zipFileName) > 0) {
                        TargetKirim += JumlahServerKirimZip;
                    }

                    BerhasilKirim += await _dcFtpT.KirimFtp("LOCAL"); // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("HARIAN", zipFileName, true); // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
