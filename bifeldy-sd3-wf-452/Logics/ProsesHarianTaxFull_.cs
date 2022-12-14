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
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.localhost;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Navigations;
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

            await _db.OraPg.ExecQueryAsync(
                $@"
                    INSERT INTO dc_ttf_hdr_log (tbl_dc_kode, tgl_proses, tgl_doc, status_run)
                    VALUES (:KodeDc, {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")}, :xDate, '1')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await _db.GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
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
                        DataTable dtQueryRes = await _db.OraPg.GetDataTableAsync(queryForCSV);

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

            int statusOk = await _db.OraPg.ExecScalarAsync<int>(
                $@"
                    SELECT 1 FROM DC_TTF_HDR_LOG
                    WHERE
                        TBL_DC_KODE = :KodeDc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                        AND status_tax = 'OK'
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await _db.GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
            if (statusOk == 1) {
                await _db.UpdateDcTtfHdrLog($"start_BPB = {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}", xDate);

                try {
                    string queryTaxBPB = await _db.Q_TRF_CSV__GET("q_query", "TAXBPB");
                    DataTable dtTaxBPB = await _db.OraPg.GetDataTableAsync(queryTaxBPB);

                    await _db.UpdateDcTtfHdrLog($"JML_BPB_TAX = {dtTaxBPB.Rows.Count}", xDate);

                    int countBPBok = 0;
                    int countBPBfail = 0;
                    for (int idxTaxBPB = 0; idxTaxBPB < dtTaxBPB.Rows.Count; idxTaxBPB++) {
                        DataRow drTaxBPB = dtTaxBPB.Rows[idxTaxBPB];
                        string statusBlobTaxBPB = null;

                        try {
                            string filePathBlobRowTaxBPB = await _db.OraPg.RetrieveBlob(
                                TaxTempFullFolderPath,
                                drTaxBPB["FILE_NAME"].ToString(),
                                $@"
                                    SELECT a.HDR_DOC_BLOB
                                    FROM dc_header_blob_t a, dc_header_transaksi_t b
                                    WHERE
                                        a.hdr_hdr_id = b.hdr_hdr_id
                                        AND b.HDR_TYPE_TRANS = 'BPB SUPPLIER'
                                        AND b.HDR_NO_DOC = :no_doc
                                        AND TO_CHAR(b.HDR_TGL_DOC, 'MM/dd/yyyy') = :tgl_doc
                                        AND a.HDR_DOC_BLOB IS NOT NULL
                                ",
                                new List<CDbQueryParamBind>() {
                                    new CDbQueryParamBind { NAME = "no_doc", VALUE = drTaxBPB["DOCNO"].ToString() },
                                    new CDbQueryParamBind { NAME = "tgl_doc", VALUE = drTaxBPB["TANGGAL1"].ToString() }
                                }
                            );

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

                        await _db.OraPg.ExecQueryAsync(
                            $@"
                                INSERT INTO dc_ttf_dtl_log (tbl_dc_kode, tgl_proses, no_doc, tgl_doc, type_trans, tgl_create, supkode, status)
                                VALUES(:kode_dc, {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")}, :no_doc, to_date(:tgl_doc, 'mm/dd/yyyy'), 'BPB SUPPLIER', {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}, :sup_kode, '{statusBlobTaxBPB}')
                            ",
                            new List<CDbQueryParamBind>() {
                            new CDbQueryParamBind { NAME = "kode_dc", VALUE = await _db.GetKodeDc() },
                            new CDbQueryParamBind { NAME = "no_doc", VALUE = drTaxBPB["DOCNO"].ToString() },
                            new CDbQueryParamBind { NAME = "tgl_doc", VALUE = drTaxBPB["TANGGAL1"].ToString() },
                            new CDbQueryParamBind { NAME = "sup_kode", VALUE = drTaxBPB["SUPCO"].ToString() }
                            }
                        );
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
                    DataTable dtTaxNRB = await _db.OraPg.GetDataTableAsync(queryTaxNRB);

                    await _db.UpdateDcTtfHdrLog($"JML_NRB_TAX = {dtTaxNRB.Rows.Count}", xDate);

                    int countNRBok = 0;
                    int countNRBfail = 0;
                    for (int idxTaxNRB = 0; idxTaxNRB < dtTaxNRB.Rows.Count; idxTaxNRB++) {
                        DataRow drTaxNRB = dtTaxNRB.Rows[idxTaxNRB];
                        string statusBlobTaxNRB = null;

                        try {
                            string filePathBlobRowTaxNRB = await _db.OraPg.RetrieveBlob(
                                TaxTempFullFolderPath,
                                drTaxNRB["FILE_NAME"].ToString(),
                                $@"
                                    SELECT a.HDR_DOC_BLOB
                                    FROM dc_header_blob_t a, dc_header_transaksi_t b
                                    WHERE
                                        a.hdr_hdr_id = b.hdr_hdr_id
                                        AND b.HDR_TYPE_TRANS = 'NRB SUPPLIER'
                                        AND b.HDR_NO_DOC = :no_doc
                                        AND TO_CHAR(b.HDR_TGL_DOC, 'MM/dd/yyyy') = :tgl_doc
                                        AND a.HDR_DOC_BLOB IS NOT NULL
                                ",
                                new List<CDbQueryParamBind>() {
                                    new CDbQueryParamBind { NAME = "no_doc", VALUE = drTaxNRB["DOCNO"].ToString() },
                                    new CDbQueryParamBind { NAME = "tgl_doc", VALUE = drTaxNRB["TANGGAL1"].ToString() }
                                }
                            );

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

                        await _db.OraPg.ExecQueryAsync(
                            $@"
                                INSERT INTO dc_ttf_dtl_log (tbl_dc_kode, tgl_proses, no_doc, tgl_doc, type_trans, tgl_create, supkode, status)
                                VALUES(:kode_dc, {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")}, :no_doc, to_date(:tgl_doc, 'mm/dd/yyyy'), 'BPB SUPPLIER', {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}, :sup_kode, '{statusBlobTaxNRB}')
                            ",
                            new List<CDbQueryParamBind>() {
                                new CDbQueryParamBind { NAME = "kode_dc", VALUE = await _db.GetKodeDc() },
                                new CDbQueryParamBind { NAME = "no_doc", VALUE = drTaxNRB["DOCNO"].ToString() },
                                new CDbQueryParamBind { NAME = "tgl_doc", VALUE = drTaxNRB["TANGGAL1"].ToString() },
                                new CDbQueryParamBind { NAME = "sup_kode", VALUE = drTaxNRB["SUPCO"].ToString() }
                            }
                        );
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

                DataTable dtRettok = await _db.OraPg.GetDataTableAsync(
                    $@"
                        SELECT *
                        FROM DC_TTF_HDR_LOG
                        WHERE
                            TBL_DC_KODE = :kode_dc
                            AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                    ",
                    new List<CDbQueryParamBind>() {
                        new CDbQueryParamBind { NAME = "kode_dc", VALUE = await _db.GetKodeDc() },
                        new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                    }
                );

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
            CProsesHarian prosesHarian = (CProsesHarian)currentControl;
            Button button = (Button) sender;
            button.BackColor = Color.FromArgb(255, 207, 223);
            DateTime dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date;
            DateTime dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date;
            await Task.Run(async () => {
                string infoMessage = null;
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd) && await IsDateEndYesterday(dateEnd)) {
                    string TaxTempFullFolderPath = Path.Combine(_app.AppLocation, $"TAX-{await _db.GetKodeDc()}");
                    if (!Directory.Exists(TaxTempFullFolderPath)) {
                        Directory.CreateDirectory(TaxTempFullFolderPath);
                    }

                    _berkas.DeleteOldFilesInFolder(TaxTempFullFolderPath, 0);
                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    string targetFileName = null;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteLog(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        targetFileName = $"{await _db.GetKodeDc()}TTFONLINE{xDate:MMddyyyy}.ZIP";

                        int cekLog = await _db.OraPg.ExecScalarAsync<int>(
                            $@"
                                SELECT
                                    {(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(1, 0)
                                FROM
                                    DC_TTF_HDR_LOG
                                WHERE
                                    TBL_DC_KODE = :KodeDc
                                    AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                            ",
                            new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "KodeDc", VALUE = await _db.GetKodeDc() },
                                new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                            }
                        );
                        if (cekLog == 0) {

                            await FullCreate(button, xDate, TaxTempFullFolderPath);
                            await FromZip(targetFileName, xDate, TaxTempFullFolderPath);
                            await FromTransfer(targetFileName, button, xDate);

                        }
                        else {
                            string dialogTitle = "Proses Manual Full Re-create TAX-TTF";
                            string cekRun = await _db.OraPg.ExecScalarAsync<string>(
                                $@"
                                    SELECT
                                        {(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(STATUS_RUN, '0')
                                    FROM
                                        DC_TTF_HDR_LOG
                                    WHERE
                                        TBL_DC_KODE = :KodeDc
                                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                                ",
                                new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await _db.GetKodeDc() },
                                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                                }
                            );

                            if (string.IsNullOrEmpty(cekRun) || cekRun == "0") {
                                DialogResult dialogResult = MessageBox.Show(
                                    "Data sudah pernah dibuat, yakin akan menghapus data lama dan membuat dari awal?",
                                    dialogTitle,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2
                                );
                                if (dialogResult == DialogResult.Yes) {
                                    await _db.OraPg.ExecQueryAsync(
                                        $@"
                                            DELETE FROM dc_ttf_dtl_log
                                            WHERE
                                                TBL_DC_KODE = :KodeDc
                                                AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                                        ",
                                        new List<CDbQueryParamBind> {
                                            new CDbQueryParamBind { NAME = "KodeDc", VALUE = await _db.GetKodeDc() },
                                            new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                                        }
                                    );

                                    await _db.OraPg.ExecQueryAsync(
                                        $@"
                                            DELETE FROM dc_ttf_hdr_log
                                            WHERE
                                                TBL_DC_KODE = :KodeDc
                                                AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                                        ",
                                        new List<CDbQueryParamBind> {
                                            new CDbQueryParamBind { NAME = "KodeDc", VALUE = await _db.GetKodeDc() },
                                            new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                                        }
                                    );

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
                if (string.IsNullOrEmpty(infoMessage)) {
                    if (BerhasilKirim == 0 || TargetKirim == 0) {
                        infoMessage = $"Ada Masalah, Belum Ada {button.Text} Yang Diproses !!";
                    }
                    else if (BerhasilKirim < TargetKirim && TargetKirim > 0) {
                        infoMessage = $"Ada Beberapa Proses {button.Text} Yang Gagal !!";
                    }
                    else if (BerhasilKirim >= TargetKirim && TargetKirim > 0) {
                        infoMessage = $"{button.Text} Sukses !!";
                    }
                    else {
                        infoMessage = $"{button.Text} Error !!";
                    }
                }
                MessageBox.Show(infoMessage, button.Text);
            });
        }

    }

}
