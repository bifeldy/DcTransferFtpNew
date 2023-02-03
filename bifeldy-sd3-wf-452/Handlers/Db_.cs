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
        Task<DataTable> OraPg_GetDataTable(string sqlQuery);
        Task<DateTime> OraPg_GetYesterdayDate(int lastDay);
        Task<DateTime> OraPg_GetLastMonth(int lastMonth);
        Task<DateTime> OraPg_GetCurrentDate();
        Task<CDbExecProcResult> OraPg_CALL_(string procName);
        Task<DataTable> GetLogErrorTransfer();
        Task<DataTable> GetLogErrorProses();
        Task<CDbExecProcResult> CALL__P_TGL(string procedureName, DateTime P_TGL);
        Task<string> Q_TRF_CSV__GET(string kolom, string q_filename);
        Task<string> DC_FILE_SCHEDULER_T__GET(string kolom, string file_key);
        Task<DbDataReader> GetFtpInfo(string pga_type);
        Task<string> GetURLWebService(string webType);
        Task<string> GetDcExt();
        Task<string> GetWinFunction();
        Task<string> GetKodeDCInduk();
        /* Proses Harian Data Irpc */
        Task<DataTable> GetIrpc(DateTime xDate);
        /* Proses Harian Data TaxTemp Full */
        Task<CDbExecProcResult> CALL_TRF_BAKP_EVO(string procedureName, DateTime P_TGL);
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
        Task<CDbExecProcResult> CALL_ENDORSEMENT(DateTime P_TGL, string P_QUERY = null, string P_FILENAME = null, string P_QUERY2 = null, string P_FILENAME2 = null, string P_MSG = null);
        Task<CDbExecProcResult> CALL_NPK_BAP(string procName, string TGLAWALPAR, string TGLAKHIRPAR, string V_RESULT = null);
        Task<CDbExecProcResult> CALL_ID_BULAN_G_EVO(string procName, DateTime dateTime);
        Task<CDbExecProcResult> CALL_DataBulananCentralisasiHO(string procName, string tahunBulan);
        Task<CDbExecProcResult> CALL_ICHO(string procName, DateTime dateTime, string usingBulan);
        Task<bool> BulananDeleteDcDsiWsToko(string fileTimeIdBulanGFormat);
        Task<DataTable> BulananDsiGetDataTable(int periode);
        Task<bool> InsertNewDcAmtaLog(DateTime xDate);
        Task<bool> UpdateDcDcAmtaLog(string columnValue, DateTime xDate);
        Task<int> GetJumlahPluExpired();
    }

    public sealed class CDb : CDbHandler, IDb {

        private readonly IApp _app;

        public CDb(IApp app, IOracle oracle, IPostgres postgres, IMsSQL mssql) : base(app, oracle, postgres, mssql) {
            _app = app;
        }

        public async Task<DataTable> OraPg_GetDataTable(string sqlQuery) {
            return await OraPg.GetDataTableAsync(sqlQuery);
        }

        public async Task<DateTime> OraPg_GetYesterdayDate(int lastDay) {
            return await OraPg.ExecScalarAsync<DateTime>(
                $@"
                    SELECT {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")} - :last_day
                    {(_app.IsUsingPostgres ? "" : "FROM DUAL")}
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "last_day", VALUE = lastDay }
                }
            );
        }

        public async Task<DateTime> OraPg_GetLastMonth(int lastMonth) {
            return await OraPg.ExecScalarAsync<DateTime>(
                $@"
                    SELECT TRUNC(add_months({(_app.IsUsingPostgres ? "CURRENT_DATE" : "SYSDATE")}, - :last_month))
                    {(_app.IsUsingPostgres ? "" : "FROM DUAL")}
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "last_month", VALUE = lastMonth }
                }
            );
        }

        public async Task<DateTime> OraPg_GetCurrentDate() {
            return await OraPg.ExecScalarAsync<DateTime>($@"
                SELECT {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE) FROM DUAL")}
            ");
        }

        public async Task<CDbExecProcResult> OraPg_CALL_(string procedureName) {
            return await OraPg.ExecProcedureAsync(procedureName);
        }

        /* ** */

        public async Task<DataTable> GetLogErrorTransfer() {
            return await OraPg.GetDataTableAsync(
                $@"
                    SELECT
                        ERROR_TIME AS err_tme,
                        ERROR_MSG AS err_msg
                    FROM
                        FTP_ERROR_LOG
                    WHERE
                        TO_CHAR(ERROR_TIME, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                    ORDER BY
                        ERROR_TIME DESC
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = DateTime.Now }
                }
            );
        }

        public async Task<DataTable> GetLogErrorProses() {
            return await OraPg.GetDataTableAsync(
                $@"
                    SELECT
                        TAXTEMP_TGL AS err_tme2,
                        STATUS AS err_msg2
                    FROM
                        TAXTEMP_LOG
                    WHERE
                        TO_CHAR(TAXTEMP_TGL, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                    ORDER BY
                        TAXTEMP_TGL DESC
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "x_date", VALUE = DateTime.Now }
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

        public async Task<string> DC_FILE_SCHEDULER_T__GET(string kolom, string file_key) {
            return await OraPg.ExecScalarAsync<string>(
                $@"
                    SELECT {kolom} FROM DC_FILE_SCHEDULER_T WHERE file_key = :file_key
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "file_key", VALUE = file_key }
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

        public async Task<string> GetURLWebService(string webType) {
            return await OraPg.ExecScalarAsync<string>(
                $@"SELECT WEB_URL FROM DC_WEBSERVICE_T WHERE WEB_TYPE = :web_type",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "web_type", VALUE = webType }
                }
            );
        }

        public async Task<string> GetDcExt() {
            return await OraPg.ExecScalarAsync<string>($"SELECT SUBSTR (TBL_DC_KODE, 2, 3) AS DCEXT FROM DC_TABEL_DC_T");
        }

        public async Task<string> GetWinFunction() {
            return await OraPg.ExecScalarAsync<string>($"SELECT SUBSTR(MAX(PERIODE), 3, 4) AS WINFUNCTION FROM DC_TRNH_HDR_T");
        }

        public async Task<string> GetKodeDCInduk() {
            return await OraPg.ExecScalarAsync<string>($"SELECT CASE WHEN TBL_JENIS_DC NOT IN ('INDUK', 'LPG', 'IPLAZA') THEN TBL_DC_INDUK ELSE TBL_DC_KODE END AS KODE_DC FROM DC_TABEL_DC_T");
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

        public async Task<CDbExecProcResult> CALL_TRF_BAKP_EVO(string procedureName, DateTime P_TGL) {
            return await OraPg.ExecProcedureAsync(
                procedureName,
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "p_dckode", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "p_tgl", VALUE = P_TGL }
                }
            );
        }

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

        /* ** */

        public async Task<CDbExecProcResult> CALL_ENDORSEMENT(DateTime P_TGL, string P_QUERY = null, string P_FILENAME = null, string P_QUERY2 = null, string P_FILENAME2 = null, string P_MSG = null) {
            return await OraPg.ExecProcedureAsync(
                "CREATE_ENDORSMENT_CSV",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "P_TGL", VALUE = P_TGL },
                    new CDbQueryParamBind { NAME = "P_QUERY", VALUE = P_QUERY, DIRECTION = ParameterDirection.InputOutput },
                    new CDbQueryParamBind { NAME = "P_FILENAME", VALUE = P_FILENAME, DIRECTION = ParameterDirection.InputOutput },
                    new CDbQueryParamBind { NAME = "P_QUERY2", VALUE = P_QUERY2, DIRECTION = ParameterDirection.InputOutput },
                    new CDbQueryParamBind { NAME = "P_FILENAME2", VALUE = P_FILENAME2, DIRECTION = ParameterDirection.InputOutput },
                    new CDbQueryParamBind { NAME = "P_MSG", VALUE = P_MSG, DIRECTION = ParameterDirection.InputOutput }
                }
            );
        }

        public async Task<CDbExecProcResult> CALL_NPK_BAP(string procName, string TGLAWALPAR, string TGLAKHIRPAR, string V_RESULT = null) {
            return await OraPg.ExecProcedureAsync(
                procName,
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "TGLAWALPAR", VALUE = TGLAWALPAR },
                    new CDbQueryParamBind { NAME = "TGLAKHIRPAR", VALUE = TGLAKHIRPAR },
                    new CDbQueryParamBind { NAME = "V_RESULT", VALUE = V_RESULT, DIRECTION = ParameterDirection.InputOutput }
                }
            );
        }

        /* Data Bulanan */

        public async Task<CDbExecProcResult> CALL_ID_BULAN_G_EVO(string procName, DateTime dateTime) {
            return await OraPg.ExecProcedureAsync(
                procName,
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "p_tgl", VALUE = dateTime },
                    new CDbQueryParamBind { NAME = "p_dckode", VALUE = await GetKodeDc() }
                }
            );
        }

        public async Task<CDbExecProcResult> CALL_DataBulananCentralisasiHO(string procName, string tahunBulan) {
            return await OraPg.ExecProcedureAsync(
                procName,
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "p_tahunbulan", VALUE = tahunBulan }
                }
            );
        }

        public async Task<CDbExecProcResult> CALL_ICHO(string procName, DateTime dateTime, string usingBulan) {
            return await OraPg.ExecProcedureAsync(
                procName,
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "p_tgl", VALUE = dateTime },
                    new CDbQueryParamBind { NAME = "p_tga_bln", VALUE = usingBulan }
                }
            );
        }

        public async Task<bool> BulananDeleteDcDsiWsToko(string fileTimeIdBulanGFormat) {
            return await OraPg.ExecQueryAsync(
                $@"DELETE FROM DC_DSI_WSTOKO WHERE THNBLN = :thn_bln",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "thn_bln", VALUE = fileTimeIdBulanGFormat }
                }
            );
        }

        public async Task<DataTable> BulananDsiGetDataTable(int periode) {
            return await OraPg.GetDataTableAsync(
                $@"
                    SELECT
                        KDDC, PERIODE, JML_HARI, PLUID, DIV, DEP,
                        KAT, TAG, KD_SUPPLIER, NM_SUPPLIER, QTY_SLD_AWL, RP_SLD_AWL,
                        QTY_SLD_AKHR, RP_SLD_AKHR, QTY_NPB, RP_NPB, DSI, UPDREC_DATE,
                        HPP_TOKO
                    FROM
                        dc_analisa_dsi_t
                    WHERE
                        kddc = :kddc
                        AND periode = :periode
                    ORDER BY
                        pluid
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "kddc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "periode", VALUE = periode }
                }
            );
        }

        /* Proses Harian Data AMTA */

        public async Task<bool> InsertNewDcAmtaLog(DateTime xDate) {
            return await OraPg.ExecQueryAsync(
                $@"
                    INSERT INTO dc_amta_log (tbl_dc_kode, tgl_proses, type_proses)
                    VALUES (:kode_dc, :x_date, :app_name)
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate },
                    new CDbQueryParamBind { NAME = "app_name", VALUE = _app.AppName }
                }
            );
        }

        public async Task<bool> UpdateDcDcAmtaLog(string columnValue, DateTime xDate) {
            return await OraPg.ExecQueryAsync(
                $@"
                    UPDATE dc_amta_log
                    SET {columnValue}
                    WHERE
                        tbl_dc_kode = :kode_dc
                        AND type_proses = :app_name
                        AND TO_CHAR(tgl_proses, 'dd/MM/yyyy') = TO_CHAR(:x_date, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind>() {
                    new CDbQueryParamBind { NAME = "kode_dc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "app_name", VALUE = _app.AppName },
                    new CDbQueryParamBind { NAME = "x_date", VALUE = xDate }
                }
            );
        }

        /* ** */

        public async Task<int> GetJumlahPluExpired() {
            return await OraPg.ExecScalarAsync<int>("SELECT COUNT(1) FROM DC_PLU_EXPIRED_T");
        }

    }

}
