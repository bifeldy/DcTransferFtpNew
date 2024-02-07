/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data POM
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

    public interface IProsesHarianPom : ILogics { }

    public sealed class CProsesHarianPom : CLogics, IProsesHarianPom {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IDcFtpT _dcFtpT;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IKafkaFile _kafkaFile;

        public CProsesHarianPom(
            IApp app,
            ILogger logger,
            IDb db,
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IDcFtpT dc_ftp_t,
            IQTrfCsv qTrfCsv,
            IKafkaFile kafkaFile
        ) : base(db, csv, zip) {
            _app = app;
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _csv = csv;
            _zip = zip;
            _dcFtpT = dc_ftp_t;
            _qTrfCsv = qTrfCsv;
            _kafkaFile = kafkaFile;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid() && IsDateRangeSameMonth()) {
                    _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, 0);
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

                    string fileTimeICHOFormat = $"{dateStart:MM}";
                    string csvFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    string keterangan = $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)";
                    _logger.WriteInfo(GetType().Name, keterangan);

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "TRF_POM_EVO";
                        CDbExecProcResult res = await _db.CALL_ICHO(procName, xDate, "N");
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        csvFileName = $"POM{fileTimeICHOFormat}{xDate:dd}G.CSV";
                        await _qTrfCsv.CreateCSVFile("POM", csvFileName);
                        TargetKirim += JumlahServerKirimCsv;
                    }

                    string[] targetKafkaFile = _zip.ListFileForZip.ToArray();

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "POM");
                    _zip.ZipListFileInFolder(zipFileName, _csv.CsvFolderPath);
                    TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauSingleZipKeFtpDev("POM", zipFileName, true)).Success.Count; // *.ZIP Sebanyak :: 1

                    (string hostPort, string topicName) = await _kafkaFile.GetHostIpPortAndTopic("POM");
                    foreach (string fn in targetKafkaFile) {
                        await _kafkaFile.KirimFile(hostPort, topicName, _csv.CsvFolderPath, fn, dateStart, keterangan);
                    }
                    await _kafkaFile.KirimFile(hostPort, topicName, _zip.ZipFolderPath, zipFileName, dateStart, keterangan);

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
