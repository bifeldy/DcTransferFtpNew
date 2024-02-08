/**
 * 
 * Author       :: Basilius Bias Astho Christyono
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
using System.Collections.Generic;
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
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianIcho(
            IApp app,
            ILogger logger,
            IDb db,
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db, csv, zip) {
            _app = app;
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _csv = csv;
            _zip = zip;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid() && IsDateRangeSameMonth()) {
                    _berkas.BackupAllFilesInFolder(_csv.CsvFolderPath);
                    _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, 0);
                    _berkas.BackupAllFilesInFolder(_zip.ZipFolderPath);
                    _berkas.DeleteOldFilesInFolder(_zip.ZipFolderPath, 0);
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

                    string fileTimeICHOFormat = $"{dateStart:MM}";
                    string csvFileName = null;

                    string fileTimeICHOFormat2 = await _db.GetWinFunction();

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "TRF_ICHO_NEW_EVO";
                        CDbExecProcResult res = await _db.CALL_ICHO(procName, xDate, "N");
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        csvFileName = $"PAR{fileTimeICHOFormat}{xDate:dd}G.CSV";
                        List<string> reqPAR = new List<string> { "INDUK", "DEPO" };
                        if (await _qTrfCsv.CreateCSVFile("PAR", csvFileName, required: reqPAR.Contains(await _db.GetJenisDc()))) {
                            TargetKirim += JumlahServerKirimCsv;
                        }
                    }

                    csvFileName = "SUPMAST.CSV";
                    await _qTrfCsv.CreateCSVFile("SUPMAST", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = "HRGBELI.CSV";
                    await _qTrfCsv.CreateCSVFile("HRGBELI", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = "PROTECT.CSV";
                    await _qTrfCsv.CreateCSVFile("PROTECT", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"REG{fileTimeICHOFormat2}.CSV";
                    await _qTrfCsv.CreateCSVFile("REG", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"TRNH{fileTimeICHOFormat2}.CSV";
                    await _qTrfCsv.CreateCSVFile("TRNH", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "TRNH");
                    _zip.ZipListFileInFolder(zipFileName, _csv.CsvFolderPath);
                    TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauSingleZipKeFtpDev("ICHO", zipFileName, true)).Success.Count; // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
