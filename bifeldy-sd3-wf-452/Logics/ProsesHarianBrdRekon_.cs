﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian BRD Rekon
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

    public interface IProsesHarianBrdRekon : ILogics { }

    public sealed class CProsesHarianBrdRekon : CLogics, IProsesHarianBrdRekon {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianBrdRekon(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db, csv, zip) {
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
                    JumlahServerKirimCsv = 2;

                    string fileTimeBRDFormat = $"{dateStart:yyyyMM}";
                    string fileTimeBRDFormat2a = $"{dateStart:MM}";
                    string fileTimeBRDFormat2b = $"{dateStart.Year}";
                    string csvFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "SEND_DATA_ORA_NEW_BRD_EVO";
                        CDbExecProcResult res = await _db.CALL__P_TGL(procName, xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        csvFileName = $"{fileTimeBRDFormat}{xDate:dd}.BRD";
                        await _qTrfCsv.CreateCSVFile("BRD", csvFileName);
                        TargetKirim += JumlahServerKirimCsv;

                        csvFileName = $"REC{fileTimeBRDFormat2b.Substring(3, 1)}{fileTimeBRDFormat2a}{xDate:dd}.BRD";
                        await _qTrfCsv.CreateCSVFile("REC", csvFileName);
                        TargetKirim += JumlahServerKirimCsv;

                        csvFileName = $"{fileTimeBRDFormat}{xDate:dd}.PRM";
                        await _qTrfCsv.CreateCSVFile("PRM", csvFileName);
                        TargetKirim += JumlahServerKirimCsv;
                    }

                    // string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "BRD");
                    // _zip.ZipListFileInFolder(zipFileName);
                    // TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauSingleZipKeFtpDev("BRD Rekon")).Success.Count; // *.CSV Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
