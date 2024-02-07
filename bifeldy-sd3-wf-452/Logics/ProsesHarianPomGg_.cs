/**
 * 
 * Author       :: Basilius Bias Astho Christyono
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
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IDcFtpT _dcFtpT;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IKafkaFile _kafkaFile;

        public CProsesHarianPomGg(
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

            CProsesHarian prosesHarian = (CProsesHarian) currentControl;
            string usingBulanG = prosesHarian.ChkPomgg.Checked ? "Y" : "N";

            await Task.Run(async () => {
                if (IsDateRangeSameDay()) {
                    _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, 0);
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    string keterangan = $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)";
                    _logger.WriteInfo(GetType().Name, keterangan);

                    string procName = "TRF_POMGG_EVO";
                    CDbExecProcResult res = await _db.CALL_ICHO(procName, dateStart, usingBulanG);
                    if (res == null || !res.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName}");
                    }

                    string csvFileName = "POMGG.CSV";
                    await _qTrfCsv.CreateCSVFile("POMGG", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "POMGG");
                    _zip.ZipListFileInFolder(zipFileName, _csv.CsvFolderPath);
                    TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauSingleZipKeFtpDev("POMGG", zipFileName, true)).Success.Count; // *.ZIP Sebanyak :: 1

                    (string hostPort, string topicName) = await _kafkaFile.GetHostIpPortAndTopic("POMGG");
                    await _kafkaFile.KirimFile(hostPort, topicName, _csv.CsvFolderPath, csvFileName, dateStart, keterangan);
                    await _kafkaFile.KirimFile(hostPort, topicName, _zip.ZipFolderPath, zipFileName, dateStart, keterangan);

                    _berkas.CleanUp();
                }
            });

            CheckHasilKiriman();
        }

    }

}
