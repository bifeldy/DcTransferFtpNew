﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data Tax Full
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.localhost;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;
using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianTaxFull : ILogics { }

    public sealed class CProsesHarianTaxFull : CLogics, IProsesHarianTaxFull {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;
        private readonly IConverter _converter;
        private readonly IStream _stream;

        public CProsesHarianTaxFull(
            IApp app,
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t,
            IConverter converter,
            IStream stream
        ) : base(db) {
            _app = app;
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
            _converter = converter;
            _stream = stream;
        }

        private async Task FullCreate(Button button, DateTime xDate, string TaxTempFullFolderPath) {

            await _db.InsertNewDcTtfHdrLog(xDate);
            await _db.UpdateDcTtfHdrLog($"start_tax = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

            try {
                string procName1 = "CREATE_TAXTEMP1_EVO";
                CDbExecProcResult res1 = await _db.CALL__P_TGL(procName1, xDate);
                if (!res1.STATUS) {
                    throw new Exception($"Gagal Menjalankan Procedure {procName1}");
                }

                string seperator = await _db.Q_TRF_CSV__GET("q_seperator", "TAX2") ?? "|";
                string queryForCSV = await _db.Q_TRF_CSV__GET("q_query", "TAX2");
                string filename = await _db.Q_TRF_CSV__GET("q_namafile", "TAX2");

                if (string.IsNullOrEmpty(seperator) || string.IsNullOrEmpty(queryForCSV) || string.IsNullOrEmpty(filename)) {
                    string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                    MessageBox.Show(status_error, $"{button.Text} :: Q_TRF_CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    await _db.UpdateDcTtfHdrLog($"status_tax = '{status_error}'", xDate);
                }
                else {
                    await _db.UpdateDcTtfHdrLog($"FILE_TAX = '{filename}'", xDate);

                    try {
                        DataTable dtQueryRes = await _db.GetDataTable(queryForCSV);

                        if (dtQueryRes.Rows.Count > 0) {
                            await _db.UpdateDcTtfHdrLog($"status_tax = 'OK'", xDate);
                        }
                        else {
                            await _db.UpdateDcTtfHdrLog($"status_tax = 'Data Kosong'", xDate);
                        }

                        if (_berkas.DataTable2CSV(dtQueryRes, filename, seperator, TaxTempFullFolderPath)) {
                            // _berkas.ListFileForZip.Add(filename);
                            // TargetKirim++;
                        }
                    }
                    catch (Exception e) {
                        MessageBox.Show(e.Message, $"{button.Text} :: TAX2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        await _db.UpdateDcTtfHdrLog($"status_tax = '{e.Message}'", xDate);
                    }
                }
            }
            catch (Exception ex) {
                await _db.UpdateDcTtfHdrLog($"status_tax = '{ex.Message}'", xDate);
            }

            await _db.UpdateDcTtfHdrLog($"stop_tax = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

            (bool success1, bool addedQueue1) = await _qTrfCsv.CreateCSVFile("", "SUMTAX", TaxTempFullFolderPath);
            // if (success1 && addedQueue1) {
            //     TargetKirim++;
            // }

            (bool success2, bool addedQueue2) = await _qTrfCsv.CreateCSVFile("", "REKTAX", TaxTempFullFolderPath);
            // if (success2 && addedQueue2) {
            //     TargetKirim++;
            // }

            string procName2 = "TRF_BAKP_EVO";
            CDbExecProcResult res2 = await _db.CALL__P_TGL(procName2, xDate);
            if (res2 == null || !res2.STATUS) {
                throw new Exception($"Gagal Menjalankan Procedure {procName2}");
            }

            (bool success3, bool addedQueue3) = await _qTrfCsv.CreateCSVFile("", "BAKP", TaxTempFullFolderPath);
            // if (success3 && addedQueue3) {
            //     TargetKirim++;
            // }

            int statusOk = await _db.TaxTempStatusOk(xDate);
            if (statusOk == 1) {
                await _db.UpdateDcTtfHdrLog($"start_BPB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

                try {
                    string queryTaxBPB = await _db.Q_TRF_CSV__GET("q_query", "TAXBPB");
                    DataTable dtTaxBPB = await _db.GetDataTable(queryTaxBPB);

                    await _db.UpdateDcTtfHdrLog($"JML_BPB_TAX = {dtTaxBPB.Rows.Count}", xDate);

                    int countBPBok = 0;
                    int countBPBfail = 0;
                    for (int idxTaxBPB = 0; idxTaxBPB < dtTaxBPB.Rows.Count; idxTaxBPB++) {
                        DataRow drTaxBPB = dtTaxBPB.Rows[idxTaxBPB];
                        string statusBlobTaxBPB = null;

                        try {
                            string filePathBlobRowTaxBPB = await _db.TaxTempRetrieveBlob(TaxTempFullFolderPath, "BPB SUPPLIER", drTaxBPB);
                            if (string.IsNullOrEmpty(filePathBlobRowTaxBPB)) {
                                throw new Exception("Gagal Mengunduh File BPB SUPPLIER");
                            }

                            countBPBok++;
                            statusBlobTaxBPB = "OK";
                            // TargetKirim++;
                            // _berkas.ListFileForZip.Add(drTaxBPB["FILE_NAME"].ToString());
                        }
                        catch (Exception e) {
                            countBPBfail++;
                            statusBlobTaxBPB = e.Message;
                            MessageBox.Show(e.Message, $"{button.Text} :: BPB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        await _db.InsertNewDcTtfDtlLog(statusBlobTaxBPB, drTaxBPB);
                    }

                    // TargetKirim += (countBPBok + countBPBfail);
                    string statusTaxBPB = "NOT COMPLETED";
                    if (countBPBok + countBPBfail == dtTaxBPB.Rows.Count) {
                        statusTaxBPB = "COMPLETED";
                    }

                    await _db.UpdateDcTtfHdrLog($@"
                        JML_BPB_OK = {countBPBok},
                        JML_BPB_FAIL = {countBPBfail},
                        STATUS_BPB = '{statusTaxBPB}'
                    ", xDate);
                }
                catch (Exception ex) {
                    await _db.UpdateDcTtfHdrLog($"STATUS_BPB = '{ex.Message}'", xDate);
                }

                await _db.UpdateDcTtfHdrLog($@"
                    stOP_BPB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")},
                    stART_NRB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}
                ", xDate);

                try {
                    string queryTaxNRB = await _db.Q_TRF_CSV__GET("q_query", "TAXNRB");
                    DataTable dtTaxNRB = await _db.GetDataTable(queryTaxNRB);

                    await _db.UpdateDcTtfHdrLog($"JML_NRB_TAX = {dtTaxNRB.Rows.Count}", xDate);

                    int countNRBok = 0;
                    int countNRBfail = 0;
                    for (int idxTaxNRB = 0; idxTaxNRB < dtTaxNRB.Rows.Count; idxTaxNRB++) {
                        DataRow drTaxNRB = dtTaxNRB.Rows[idxTaxNRB];
                        string statusBlobTaxNRB = null;

                        try {
                            string filePathBlobRowTaxNRB = await _db.TaxTempRetrieveBlob(TaxTempFullFolderPath, "NRB SUPPLIER", drTaxNRB);
                            if (string.IsNullOrEmpty(filePathBlobRowTaxNRB)) {
                                throw new Exception("Gagal Mengunduh File NRB SUPPLIER");
                            }

                            countNRBok++;
                            statusBlobTaxNRB = "OK";
                            // TargetKirim++;
                            // _berkas.ListFileForZip.Add(drTaxNRB["FILE_NAME"].ToString());
                        }
                        catch (Exception e) {
                            countNRBfail++;
                            statusBlobTaxNRB = e.Message;
                            MessageBox.Show(e.Message, $"{button.Text} :: NRB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        await _db.InsertNewDcTtfDtlLog(statusBlobTaxNRB, drTaxNRB);
                    }

                    // TargetKirim += (countNRBok + countNRBfail);
                    string statusTaxNRB = "NOT COMPLETED";
                    if (countNRBok + countNRBfail == dtTaxNRB.Rows.Count) {
                        statusTaxNRB = "COMPLETED";
                    }

                    await _db.UpdateDcTtfHdrLog($@"
                        JML_NRB_OK = {countNRBok},
                        JML_NRB_FAIL = {countNRBfail},
                        STATUS_NRB = '{statusTaxNRB}',
                    ", xDate);
                }
                catch (Exception ex) {
                    await _db.UpdateDcTtfHdrLog($"STATUS_NRB = '{ex.Message}'", xDate);
                }

                await _db.UpdateDcTtfHdrLog($@"stOP_NRB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

            }
        }

        private async Task FromZip(string targetFileName, DateTime xDate, string TaxTempFullFolderPath) {
            int totalFileInZip = _berkas.ZipAllFileInFolder(targetFileName, TaxTempFullFolderPath);
            TargetKirim++;

            await _db.UpdateDcTtfHdrLog($"FILE_ZIP = {targetFileName}", xDate);

            if (Directory.Exists(TaxTempFullFolderPath)) {
                Directory.Delete(TaxTempFullFolderPath, true);
            }
        }

        private async Task FromTransfer(string targetFileName, Button button, DateTime xDate) {
            try {
                BerhasilKirim += await _dcFtpT.KirimFtpTaxTempFull(); // *.CSV Sebanyak :: TargetKirim
                int terkirim = await _dcFtpT.KirimFtpTaxTempFull(targetFileName); // *.ZIP Sebanyak :: 1
                BerhasilKirim += terkirim;

                await _db.UpdateDcTtfHdrLog($@"
                    STATUS_TRF = '{(terkirim > 0 ? "OK" : "FAIL")}',
                    TGL_TRF = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}
                ", xDate);
            }
            catch (Exception ex1) {
                await _db.UpdateDcTtfHdrLog($@"
                    STATUS_TRF = '{ex1.Message}',
                    TGL_TRF = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}
                ", xDate);
            }

            try {
                TTFLOGService ws = new TTFLOGService();
                ws.Url = _app.GetConfig("ws_dcho");

                DataTable dtRettok = await _db.TaxTempGetDataTable(xDate);

                List<DCHO_TTF_HDR_LOG> listTTF = _converter.ConvertDataTableToList<DCHO_TTF_HDR_LOG>(dtRettok);
                string sTTF = _converter.ObjectToJson(listTTF);
                byte[] byteOfData = _stream.MemStream(sTTF);
                string tempHasil = ws.SendLogTTF(byteOfData);
            }
            catch (Exception ex2) {
                _logger.WriteError(ex2);
                MessageBox.Show(ex2.Message, $"{button.Text} :: DCHO_TTF_HDR_LOG", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            await _db.UpdateDcTtfHdrLog($@"status_run = '0'", xDate);
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd) && await IsDateEndYesterday(dateEnd)) {
                    string TaxTempFullFolderPath = Path.Combine(_app.AppLocation, $"TAX-{await _db.GetKodeDc()}");
                    if (!Directory.Exists(TaxTempFullFolderPath)) {
                        Directory.CreateDirectory(TaxTempFullFolderPath);
                    }

                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    string targetFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        _berkas.DeleteOldFilesInFolder(TaxTempFullFolderPath, 0);
                        targetFileName = $"{await _db.GetKodeDc()}TTFONLINE{xDate:MMddyyyy}.ZIP";

                        int cekLog = await _db.TaxTempCekLog(xDate);
                        if (cekLog == 0) {

                            await FullCreate(button, xDate, TaxTempFullFolderPath);
                            await FromZip(targetFileName, xDate, TaxTempFullFolderPath);
                            await FromTransfer(targetFileName, button, xDate);

                        }
                        else {
                            string dialogTitle = "Proses Manual Full Re-create TAX-TTF";
                            string cekRun = await _db.TaxTempCekRun(xDate);

                            if (string.IsNullOrEmpty(cekRun) || cekRun == "0") {
                                DialogResult dialogResult = MessageBox.Show(
                                    "Data sudah pernah dibuat, yakin akan menghapus data lama dan membuat dari awal?",
                                    dialogTitle,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2
                                );
                                if (dialogResult == DialogResult.Yes) {
                                    await _db.TaxTempDeleteDtl(xDate);
                                    await _db.TaxTempDeleteHdr(xDate);

                                    await FullCreate(button, xDate, TaxTempFullFolderPath);
                                    await FromZip(targetFileName, xDate, TaxTempFullFolderPath);
                                    await FromTransfer(targetFileName, button, xDate);
                                }
                            }
                            else {
                                MessageBox.Show(
                                    "Proses otomatis sedang berjalan, silahkan coba beberapa MENIT lagi.",
                                    dialogTitle,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }

                        }

                    }

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}