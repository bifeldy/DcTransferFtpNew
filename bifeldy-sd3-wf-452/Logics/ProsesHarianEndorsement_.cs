﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data Endorsement
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

    public interface IProsesHarianEndorsement : ILogics { }

    public sealed class CProsesHarianEndorsement : CLogics, IProsesHarianEndorsement {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianEndorsement(
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

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        CDbExecProcResult res = await _db.CALL_ENDORSEMENT(xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure CREATE_ENDORSMENT_CSV");
                        }

                        string p_msg = $"{res.PARAMETERS["P_MSG"].Value}";
                        if (!string.IsNullOrEmpty(p_msg)) {
                            throw new Exception(p_msg);
                        }

                        await _qTrfCsv.CreateCSVFile("ENDCSV");
                        TargetKirim += JumlahServerKirimCsv;
                    }

                    // string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "ENDCSV");
                    // _zip.ZipListFileInFolder(zipFileName);
                    // TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauSingleZipKeFtpDev("ENDCSV")).Success.Count; // *.CSV Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
