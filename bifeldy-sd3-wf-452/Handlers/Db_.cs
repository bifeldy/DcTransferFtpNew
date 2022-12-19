/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Turunan `CDatabase`
 *              :: Harap Didaftarkan Ke DI Container
 *              :: Instance Semua Database Bridge
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using bifeldy_sd3_lib_452.Abstractions;
using bifeldy_sd3_lib_452.Databases;
using bifeldy_sd3_lib_452.Models;

using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Handlers {

    public interface IDb : IDbHandler {
        Task<DataTable> GetDataTable(string sqlQuery);
        Task<DateTime> GetYesterdayDate(int lastDay);
        Task<CDbExecProcResult> CALL__P_TGL(string procedureName, DateTime P_TGL);
        Task<string> Q_TRF_CSV__GET(string kolom, string q_filename);
        Task<DbDataReader> GetFtpInfo(string pga_type);
        Task<string> GetURLWebServiceHO();
        Task<string> GetDcExt();
        Task<string> GetWinFunction();
        /* Proses Harian Data Irpc */
        Task<DataTable> GetIrpc(DateTime xDate);
        /* Proses Harian Data TaxTemp Full */
        Task<int> TaxTempCekLog(DateTime xDate);
        Task<string> TaxTempCekRun(DateTime xDate);
        Task<bool> TaxTempDeleteDtl(DateTime xDate);
        Task<bool> TaxTempDeleteHdr(DateTime xDate);
        Task<bool> InsertNewDcTtfHdrLog(DateTime xDate);
        Task<bool> UpdateDcTtfHdrLog(string columnValue, DateTime xDate);
        Task<DataTable> TaxTempGetDataTable(DateTime xDate);
        Task<int> TaxTempStatusOk(DateTime xDate);
        Task<string> TaxTempRetrieveBlob(string path, string typeTrans, DataRow dataRow);
        Task<bool> InsertNewDcTtfDtlLog(string status, DataRow dataRow);
        Task<bool> UpdateDcTtfDtlLog(string columnValue, DataRow dataRow, DateTime xDate);
        Task<int> TaxTempHitungGagal(DateTime xDate);
        Task<int> TaxTempHitungUpload(DateTime xDate);
        Task<string> TaxTempFileTaxName(DateTime xDate);
        Task<string> TaxTempFileZipName(DateTime xDate);
        Task<int> TaxTempHitungUlangOk(DateTime xDate, string typeTrans);
        Task<int> TaxTempHitungUlangFail(DateTime xDate, string typeTrans);
    }

    public sealed class CDb : CDbHandler, IDb {

        private readonly IApp _app;

        public CDb(IApp app, IOracle oracle, IPostgres postgres, IMsSQL mssql) : base(app, oracle, postgres, mssql) {
            _app = app;
        }

        public async Task<DataTable> GetDataTable(string sqlQuery) {
            return await OraPg.GetDataTableAsync(sqlQuery);
        }

        public async Task<DateTime> GetYesterdayDate(int lastDay) {
            return await OraPg.ExecScalarAsync<DateTime>(
                $@"
                    SELECT
                        {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")} - :last_day
                    {(_app.IsUsingPostgres ? "" : "FROM DUAL")}
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "last_day", VALUE = lastDay }
                }
            );
        }

        public async Task<CDbExecProcResult> CALL__P_TGL(string procedureName, DateTime P_TGL) {
            return await OraPg.ExecProcedureAsync(
                procedureName,
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "P_TGL", VALUE = P_TGL }
                }
            );
        }

        public async Task<string> Q_TRF_CSV__GET(string kolom, string q_filename) {
            return await OraPg.ExecScalarAsync<string>(
                $@"
                    SELECT {kolom} FROM Q_TRF_CSV WHERE q_filename = :q_filename
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "q_filename", VALUE = q_filename }
                }
            );
        }

        public async Task<DbDataReader> GetFtpInfo(string pga_type) {
            return await OraPg.ExecReaderAsync(
                $@"
                    SELECT
                        PGA_IPADDRESS,
                        PGA_PORTNUMBER,
                        PGA_FOLDER,
                        PGA_USERNAME,
                        PGA_PASSWORD,
                        PGA_GD_CODE,
                        PGA_TYPE
                    FROM
                        DC_FTP_T
                    WHERE
                        PGA_TYPE = :pga_type
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "pga_type", VALUE = pga_type }
                }
            );
        }

        public async Task<string> GetURLWebServiceHO() {
            return await OraPg.ExecScalarAsync<string>($@"SELECT WEB_URL FROM DC_WEBSERVICE_T WHERE WEB_TYPE = 'HO'");
        }

        public async Task<string> GetDcExt() {
            return await OraPg.ExecScalarAsync<string>($"SELECT SUBSTR (TBL_DC_KODE, 2, 3) AS DCEXT FROM DC_TABEL_DC_T");
        }

        public async Task<string> GetWinFunction() {
            return await OraPg.ExecScalarAsync<string>($"SELECT SUBSTR(MAX(PERIODE), 3, 4) AS WINFUNCTION FROM DC_TRNH_HDR_T");
        }

        /* Proses Harian Data Irpc */

        public async Task<DataTable> GetIrpc(DateTime xDate) {
            return await OraPg.GetDataTableAsync(
                $@"
                    SELECT
                        dc_kode AS kode_Dc,
                        Tgl_Doc AS tanggal_doc,
                        No_Doc AS nomor_doc,
                        TYPE AS TYPE,
                        NOSJ AS no_sj,
                        PLU AS PLU,
                        Qty AS qty,
                        nilai AS Rupiah,
                        Keterangan AS keterangan,
                        SUPKODE
                    FROM
                        DC_BPBNRB_SUPROTI_T
                    WHERE
                        TO_CHAR(tgl_doc, 'yyyyMMdd') = TO_CHAR(:x_date, 'yyyyMMdd')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        /* Proses Harian Data TaxTemp Full */

        public async Task<int> TaxTempCekLog(DateTime xDate) {
            return await OraPg.ExecScalarAsync<int>(
                $@"
                    SELECT
                        {(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(1, 0)
                    FROM
                        DC_TTF_HDR_LOG
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<string> TaxTempCekRun(DateTime xDate) {
            return await OraPg.ExecScalarAsync<string>(
                $@"
                    SELECT
                        {(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(STATUS_RUN, '0')
                    FROM
                        DC_TTF_HDR_LOG
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<bool> TaxTempDeleteDtl(DateTime xDate) {
            return await OraPg.ExecQueryAsync(
                $@"
                    DELETE FROM dc_ttf_dtl_log
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<bool> TaxTempDeleteHdr(DateTime xDate) {
            return await OraPg.ExecQueryAsync(
                $@"
                    DELETE FROM dc_ttf_hdr_log
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<bool> InsertNewDcTtfHdrLog(DateTime xDate) {
            return await OraPg.ExecQueryAsync(
                $@"
                    INSERT INTO dc_ttf_hdr_log (tbl_dc_kode, tgl_proses, tgl_doc, status_run)
                    VALUES (:kode_dc, {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")}, :x_date, '1')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<bool> UpdateDcTtfHdrLog(string columnValue, DateTime xDate) {
            return await OraPg.ExecQueryAsync(
                $@"
                    UPDATE dc_ttf_hdr_log
                    SET {columnValue}
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<DataTable> TaxTempGetDataTable(DateTime xDate) {
            return await OraPg.GetDataTableAsync(
                $@"
                    SELECT *
                    FROM DC_TTF_HDR_LOG
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<int> TaxTempStatusOk(DateTime xDate) {
            return await OraPg.ExecScalarAsync<int>(
                $@"
                    SELECT 1
                    FROM DC_TTF_HDR_LOG
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                        AND status_tax = 'OK'
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<string> TaxTempRetrieveBlob(string path, string typeTrans, DataRow dataRow) {
            return await OraPg.RetrieveBlob(
                path,
                dataRow["FILE_NAME"].ToString(),
                $@"
                    SELECT a.HDR_DOC_BLOB
                    FROM dc_header_blob_t a, dc_header_transaksi_t b
                    WHERE
                        a.hdr_hdr_id = b.hdr_hdr_id
                        AND b.HDR_TYPE_TRANS = :type_trans
                        AND b.HDR_NO_DOC = :no_doc
                        AND TO_CHAR(b.HDR_TGL_DOC, 'MM/dd/yyyy') = TO_CHAR(:tgl_doc, 'MM/dd/yyyy')
                        AND a.HDR_DOC_BLOB IS NOT NULL
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "type_trans", VALUE = typeTrans },
                    new CDbQueryParamBind { NAME = "no_doc", VALUE = dataRow["DOCNO"].ToString() },
                    new CDbQueryParamBind { NAME = "tgl_doc", VALUE = DateTime.Parse(dataRow["TANGGAL1"].ToString()) }
                }
            );
        }

        public async Task<bool> InsertNewDcTtfDtlLog(string status, DataRow dataRow) {
            return await OraPg.ExecQueryAsync(
                $@"
                    INSERT INTO dc_ttf_dtl_log (tbl_dc_kode, tgl_proses, no_doc, tgl_doc, type_trans, tgl_create, supkode, status)
                    VALUES(:kode_dc, {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")}, :no_doc, :tgl_doc, 'BPB SUPPLIER', {(_app.IsUsingPostgres ? "NOW()" : "SYSDATE")}, :sup_kode, '{status}')
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "no_doc", VALUE = int.Parse(dataRow["DOCNO"].ToString()) },
                    new CDbQueryParamBind { NAME = "tgl_doc", VALUE = DateTime.Parse(dataRow["TANGGAL1"].ToString()) },
                    new CDbQueryParamBind { NAME = "sup_kode", VALUE = dataRow["SUPCO"].ToString() }
                }
            );
        }

        public async Task<bool> UpdateDcTtfDtlLog(string columnValue, DataRow dataRow, DateTime xDate) {
            return await OraPg.ExecQueryAsync(
                $@"
                    UPDATE dc_ttf_dtl_log
                    SET {columnValue}
                    WHERE
                        NO_DOC = :no_doc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "no_doc", VALUE = int.Parse(dataRow["DOCNO"].ToString()) },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<int> TaxTempHitungGagal(DateTime xDate) {
            return await OraPg.ExecScalarAsync<int>(
                $@"
                    SELECT COUNT(*)
                    FROM DC_TTF_DTL_LOG
                    WHERE
                        TBL_DC_KODE = :kode_dc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                        AND TYPE_TRANS IN ('BPB SUPPLIER', 'NRB SUPPLIER')
                        AND STATUS <> 'OK'
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<int> TaxTempHitungUpload(DateTime xDate) {
            return await OraPg.ExecScalarAsync<int>(
                $@"
                    SELECT COUNT(*)
                    FROM DC_TTF_DTL_LOG a, DC_HEADER_TRANSAKSI_T b, dc_header_blob_t c
                    WHERE
                        a.no_doc = b.hdr_no_doc
                        AND a.tgl_doc = TRUNC(b.hdr_tgl_doc)
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                        AND TYPE_TRANS IN ('BPB SUPPLIER', 'NRB SUPPLIER')
                        AND STATUS <> 'OK'
                        AND b.hdr_hdr_id = c.hdr_hdr_id
                        AND c.hdr_doc_blob IS NOT NULL
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<string> TaxTempFileTaxName(DateTime xDate) {
            return await OraPg.ExecScalarAsync<string>(
                $@"
                    SELECT FILE_TAX
                    FROM DC_TTF_HDR_LOG
                    WHERE TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<string> TaxTempFileZipName(DateTime xDate) {
            return await OraPg.ExecScalarAsync<string>(
                $@"
                    SELECT FILE_ZIP
                    FROM DC_TTF_HDR_LOG
                    WHERE TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        public async Task<int> TaxTempHitungUlangOk(DateTime xDate, string typeTrans) {
            return await OraPg.ExecScalarAsync<int>(
                $@"
                    SELECT COUNT(*)
                    FROM DC_TTF_DTL_LOG
                    WHERE
                        TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                        AND TYPE_TRANS = :type_trans
                        AND STATUS = 'OK'
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate },
                    new CDbQueryParamBind { NAME = "type_trans", VALUE = typeTrans }
                }
            );
        }

        public async Task<int> TaxTempHitungUlangFail(DateTime xDate, string typeTrans) {
            return await OraPg.ExecScalarAsync<int>(
                $@"
                    SELECT COUNT(*)
                    FROM DC_TTF_DTL_LOG
                    WHERE
                        TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                        AND TYPE_TRANS = :type_trans
                        AND STATUS <> 'OK'
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate },
                    new CDbQueryParamBind { NAME = "type_trans", VALUE = typeTrans }
                }
            );
        }

    }

}
