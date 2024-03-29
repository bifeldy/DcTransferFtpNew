﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data Tax Re:
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianTaxRe : ILogics { }

    public sealed class CProsesHarianTaxRe : CLogics, IProsesHarianTaxRe {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;

        private readonly IProsesHarianTaxFull _prosesHarianTaxFull;

        public CProsesHarianTaxRe(
            IApp app,
            ILogger logger,
            IDb db,
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IProsesHarianTaxFull prosesHarianTaxFull
        ) : base(db, csv, zip) {
            _app = app;
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _csv = csv;
            _zip = zip;
            _prosesHarianTaxFull = prosesHarianTaxFull;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid() && IsDateRangeSameMonth() && await IsDateEndMaxYesterday()) {
                    _berkas.BackupAllFilesInFolder(_csv.CsvFolderPath);
                    _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, 0);
                    _berkas.BackupAllFilesInFolder(_zip.ZipFolderPath);
                    _berkas.DeleteOldFilesInFolder(_zip.ZipFolderPath, 0);
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

                    string csvFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string TaxTempReFolderPath = Path.Combine(_csv.CsvFolderPath, $"TAXRE_{xDate:yyyy-MM-dd}");
                        if (!Directory.Exists(TaxTempReFolderPath)) {
                            Directory.CreateDirectory(TaxTempReFolderPath);
                        }
                        _berkas.DeleteOldFilesInFolder(TaxTempReFolderPath, 0);

                        int statusOk = await _db.TaxTempStatusOk(xDate);
                        if (statusOk == 1) {

                            // Cek Log Hari x Sudah Ada, Jika Belum Skip
                            int countGagal = await _db.TaxTempHitungGagal(xDate);
                            if (countGagal > 0) {

                                // Log Tetap 1, Tapi Di Filter Hanya Yang Ada BPB Dan NPB Saja TAX-nya PR
                                // Jika Ada Blob NULL, Skip

                                int countUpload = await _db.TaxTempHitungUpload(xDate);
                                if (countGagal == countUpload) {
                                    await _db.UpdateDcTtfHdrLog($"start_tax = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

                                    try {
                                        string procName1 = "CREATE_TAXTEMP1RE_EVO";
                                        CDbExecProcResult res1 = await _db.CALL__P_TGL(procName1, xDate);
                                        if (!res1.STATUS) {
                                            throw new Exception($"Gagal Menjalankan Procedure {procName1}");
                                        }

                                        string tempName = await _db.TaxTempFileTaxName(xDate);
                                        int countSeq = 0;
                                        string tempNameDepan = string.Empty;

                                        if (tempName.IndexOf("_") > -1) {
                                            countSeq = int.Parse(tempName.Substring(tempName.LastIndexOf("_") + 1, 1)) + 1;
                                            tempNameDepan = tempName.Substring(0, tempName.LastIndexOf("_"));
                                        }
                                        else {
                                            countSeq = 1;
                                            tempNameDepan = tempName.Substring(0, tempName.LastIndexOf("."));
                                        }

                                        csvFileName = $"{tempNameDepan}_{countSeq}{tempName.Substring(tempName.LastIndexOf("."))}";

                                        await _db.UpdateDcTtfHdrLog($"FILE_TAX = '{csvFileName}'", xDate);

                                        string seperator = await _db.Q_TRF_CSV__GET("q_seperator", "TAX2RE");
                                        string queryTax2RE = await _db.Q_TRF_CSV__GET("q_query", "TAX2RE");

                                        if (string.IsNullOrEmpty(seperator) || string.IsNullOrEmpty(queryTax2RE) || string.IsNullOrEmpty(csvFileName)) {
                                            string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                                            MessageBox.Show(status_error, $"{button.Text} :: TAX2RE", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                            await _db.UpdateDcTtfHdrLog($"status_tax = '{status_error}'", xDate);
                                        }
                                        else {
                                            try {
                                                DataTable dtTax2RE = await _db.OraPg_GetDataTable(queryTax2RE);

                                                int totalRowTax = dtTax2RE.Rows.Count;
                                                if (totalRowTax > 0) {
                                                    await _db.UpdateDcTtfHdrLog($"status_tax = 'OK'", xDate);
                                                    await _db.UpdateDcTtfTaxLog($"tbl_sum_file_taxtemp = {totalRowTax}, tbl_sum_file_keter = '{_app.AppName}'", xDate);
                                                }
                                                else {
                                                    await _db.UpdateDcTtfHdrLog($"status_tax = 'Data Kosong'", xDate);
                                                }

                                                _csv.DataTable2CSV(dtTax2RE, csvFileName, seperator, TaxTempReFolderPath);
                                                // _zip.ListFileForZip.Add(filename);
                                                TargetKirim += JumlahServerKirimCsv;
                                            }
                                            catch (Exception ex) {
                                                MessageBox.Show(ex.Message, $"{button.Text} :: TAX2RE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                await _db.UpdateDcTtfHdrLog($"status_tax = '{ex.Message}'", xDate);
                                            }
                                        }
                                    }
                                    catch (Exception exception) {
                                        await _db.UpdateDcTtfHdrLog($"status_tax = '{exception.Message}'", xDate);
                                    }

                                    await _db.UpdateDcTtfHdrLog($"stop_tax = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

                                    int statusOkLagi = await _db.TaxTempStatusOk(xDate);
                                    if (statusOkLagi == 1) {
                                        await _db.UpdateDcTtfHdrLog($"start_BPB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

                                        try {
                                            string queryTaxBPBRe = await _db.Q_TRF_CSV__GET("q_query", "TAXBPBRE");
                                            DataTable dtTaxBPBRe = await _db.OraPg_GetDataTable(queryTaxBPBRe);

                                            int countBPBok = 0;
                                            int countBPBfail = 0;
                                            for (int idxTaxBPBRe = 0; idxTaxBPBRe < dtTaxBPBRe.Rows.Count; idxTaxBPBRe++) {
                                                DataRow drTaxBPBRe = dtTaxBPBRe.Rows[idxTaxBPBRe];
                                                string statusBlobTaxBPBRe = null;

                                                try {
                                                    string filePathBlobRowTaxBPB = await _db.TaxTempRetrieveBlob(TaxTempReFolderPath, "BPB SUPPLIER", drTaxBPBRe);
                                                    if (string.IsNullOrEmpty(filePathBlobRowTaxBPB)) {
                                                        throw new Exception("Gagal Mengunduh File BPB SUPPLIER");
                                                    }

                                                    countBPBok++;
                                                    statusBlobTaxBPBRe = "OK";
                                                }
                                                catch (Exception ex) {
                                                    countBPBfail++;
                                                    statusBlobTaxBPBRe = ex.Message;
                                                    MessageBox.Show(ex.Message, $"{button.Text} :: BPBRe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                                finally {
                                                    // _zip.ListFileForZip.Add(drTaxBPBRe["FILE_NAME"].ToString());
                                                    TargetKirim += JumlahServerKirimCsv;
                                                }

                                                await _db.UpdateDcTtfDtlLog(
                                                    $@"
                                                        status = '{statusBlobTaxBPBRe}',
                                                        TGL_CREATE = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}
                                                    ",
                                                    drTaxBPBRe,
                                                    xDate
                                                );
                                            }

                                            string statusTaxBPBRe = "NOT COMPLETED";
                                            if (countBPBok + countBPBfail == dtTaxBPBRe.Rows.Count) {
                                                statusTaxBPBRe = "COMPLETED";
                                            }

                                            // Hitung Ulang
                                            int hitBPBok = await _db.TaxTempHitungUlangOk(xDate, "BPB SUPPLIER");
                                            int hitBPBfail = await _db.TaxTempHitungUlangFail(xDate, "BPB SUPPLIER");

                                            await _db.UpdateDcTtfHdrLog($@"
                                                JML_BPB_OK = {hitBPBok},
                                                JML_BPB_FAIL = {hitBPBfail},
                                                STATUS_BPB = '{statusTaxBPBRe}'
                                            ", xDate);
                                        }
                                        catch (Exception exception) {
                                            await _db.UpdateDcTtfHdrLog($"STATUS_BPB = '{exception.Message}'", xDate);
                                        }

                                        await _db.UpdateDcTtfHdrLog($@"
                                            stOP_BPB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")},
                                            stART_NRB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}
                                        ", xDate);

                                        try {
                                            string queryTaxNRBRe = await _db.Q_TRF_CSV__GET("q_query", "TAXNRBRE");
                                            DataTable dtTaxNRBRe = await _db.OraPg_GetDataTable(queryTaxNRBRe);

                                            await _db.UpdateDcTtfHdrLog($"JML_NRB_TAX = {dtTaxNRBRe.Rows.Count}", xDate);

                                            int countNRBok = 0;
                                            int countNRBfail = 0;
                                            for (int idxTaxNRBRe = 0; idxTaxNRBRe < dtTaxNRBRe.Rows.Count; idxTaxNRBRe++) {
                                                DataRow drTaxNRBRe = dtTaxNRBRe.Rows[idxTaxNRBRe];
                                                string statusBlobTaxNRBRe = null;

                                                try {
                                                    string filePathBlobRowTaxNRBRe = await _db.TaxTempRetrieveBlob(TaxTempReFolderPath, "NRB SUPPLIER", drTaxNRBRe);
                                                    if (string.IsNullOrEmpty(filePathBlobRowTaxNRBRe)) {
                                                        throw new Exception("Gagal Mengunduh File NRB SUPPLIER");
                                                    }

                                                    countNRBok++;
                                                    statusBlobTaxNRBRe = "OK";
                                                }
                                                catch (Exception ex) {
                                                    countNRBfail++;
                                                    statusBlobTaxNRBRe = ex.Message;
                                                    MessageBox.Show(ex.Message, $"{button.Text} :: NRBRe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                                finally {
                                                    // _zip.ListFileForZip.Add(drTaxNRBRe["FILE_NAME"].ToString());
                                                    TargetKirim += JumlahServerKirimCsv;
                                                }

                                                await _db.UpdateDcTtfDtlLog(
                                                    $@"
                                                        status = '{statusBlobTaxNRBRe}',
                                                        TGL_CREATE = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}
                                                    ",
                                                    drTaxNRBRe,
                                                    xDate
                                                );
                                            }

                                            string statusTaxNRBRe = "NOT COMPLETED";
                                            if (countNRBok + countNRBfail == dtTaxNRBRe.Rows.Count) {
                                                statusTaxNRBRe = "COMPLETED";
                                            }

                                            // Hitung Ulang
                                            int hitNRBok = await _db.TaxTempHitungUlangOk(xDate, "BPB SUPPLIER");
                                            int hitNRBfail = await _db.TaxTempHitungUlangFail(xDate, "BPB SUPPLIER");

                                            await _db.UpdateDcTtfHdrLog($@"
                                                JML_NRB_OK = {hitNRBok},
                                                JML_NRB_FAIL = {hitNRBfail},
                                                STATUS_NRB = '{statusTaxNRBRe}',
                                            ", xDate);
                                        }
                                        catch (Exception ex) {
                                            await _db.UpdateDcTtfHdrLog($"STATUS_NRB = '{ex.Message}'", xDate);
                                        }

                                        await _db.UpdateDcTtfHdrLog($@"stOP_NRB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

                                        string tempName = await _db.TaxTempFileZipName(xDate);
                                        int countSeq = tempName.IndexOf("_") > -1 ? int.Parse(tempName.Substring(tempName.LastIndexOf("_") + 1, 1)) + 1 : 1;

                                        string zipFileName = $"{await _db.GetKodeDc()}TTFONLINE{xDate:MMddyyyy}_{countSeq}.ZIP";

                                        // Sama Persis Dari Yang Full
                                        await _prosesHarianTaxFull.FromZip(zipFileName, xDate, TaxTempReFolderPath);
                                        TargetKirim += JumlahServerKirimZip;

                                        (int csvTerkirim, int zipTerkirim) = await _prosesHarianTaxFull.FromTransfer(zipFileName, button, xDate, TaxTempReFolderPath);
                                        BerhasilKirim += (csvTerkirim + zipTerkirim);
                                    }
                                    else {
                                        MessageBox.Show(
                                            $"Tax gagal terbentuk pada tanggal - {xDate:MM/dd/yyyy}",
                                            button.Text,
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information
                                        );
                                    }
                                }
                                else {
                                    MessageBox.Show(
                                        $"Belum semua dokumen BPB dan NRB terupload pada tanggal - {xDate:MM/dd/yyyy}",
                                        button.Text,
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                }
                            }
                            else {
                                MessageBox.Show(
                                    $"Semua data BPB dan NRB sudah komplit pada tanggal - {xDate:MM/dd/yyyy}",
                                    button.Text,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );
                            }
                        }
                        else {
                            MessageBox.Show(
                                $"Tax UTAMA gagal terbentuk pada tanggal - {xDate:MM/dd/yyyy}, mohon buat ulang dari awal secara keseluruhan",
                                button.Text,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                    }
                }
            });
            CheckHasilKiriman();
        }

    }

}
