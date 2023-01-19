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
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 2;

                    string fileTimeBRDFormat2Hariana = $"{dateStart:MM}";
                    string DBFformat = $"{dateStart:MM}";
                    string csvFileName = null;

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

                        csvFileName = $"DC{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        await _qTrfCsv.CreateCSVFile("DC", csvFileName);
                        TargetKirim += JumlahServerKirimCsv;

                        csvFileName = $"ST{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        await _qTrfCsv.CreateCSVFile("ST", csvFileName);
                        TargetKirim += JumlahServerKirimCsv;

                        csvFileName = $"SX{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        await _qTrfCsv.CreateCSVFile("SX", csvFileName);
                        TargetKirim += JumlahServerKirimCsv;
                    }

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "DC");
                    _berkas.ZipListFileInFolder(zipFileName);
                    TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauZipFtpDev("PLDC", zipFileName, true)).Success.Count; // *.ZIP Sebanyak :: 1
                    BerhasilKirim += (await _dcFtpT.KirimSingleZip("EIS", zipFileName)).Success.Count; // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
