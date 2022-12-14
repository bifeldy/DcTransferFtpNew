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
        Task<DateTime> GetYesterdayDate(int lastDay);
        Task<CDbExecProcResult> CALL__P_TGL(string procedureName, DateTime P_TGL);
        Task<string> Q_TRF_CSV__GET(string kolom, string q_filename);
        Task<DbDataReader> GetFtpInfo(string pga_type);
        Task<string> GetURLWebServiceHO();
        Task<string> GetDcExt();
        Task<string> GetWinFunction();
        Task<DataTable> GetIrpc(DateTime xDate);
        Task<int> CekLogTtfHdr(DateTime xDate);
        Task<string> CekRunTtfHdr(DateTime xDate);
        Task<bool> InsertDcTtfHdrLog(DateTime xDate);
        Task<bool> UpdateDcTtfHdrLog(string columnValue, DateTime xDate);
        Task<int> CekLogTtfHdr_status_ok(DateTime xDate);
        Task<bool> DeleteDcTtfDtlLog(DateTime xDate);
        Task<bool> DeleteDcTtfHdrLog(DateTime xDate);
    }

    public sealed class CDb : CDbHandler, IDb {

        private readonly IApp _app;

        public CDb(IApp app, IOracle oracle, IPostgres postgres, IMsSQL mssql) : base(app, oracle, postgres, mssql) {
            _app = app;
        }

        public async Task<DateTime> GetYesterdayDate(int lastDay) {
            return await OraPg?.ExecScalarAsync<DateTime>(
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
            return await OraPg?.ExecScalarAsync<string>(
                $@"
                    SELECT {kolom} FROM Q_TRF_CSV WHERE q_filename = :q_filename
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "q_filename", VALUE = q_filename }
                }
            );
        }

        public async Task<DbDataReader> GetFtpInfo(string pga_type) {
            return await OraPg?.ExecReaderAsync(
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
            return await OraPg?.ExecScalarAsync<string>($@"SELECT WEB_URL FROM DC_WEBSERVICE_T WHERE WEB_TYPE = 'HO'");
        }

        public async Task<string> GetDcExt() {
            return await OraPg?.ExecScalarAsync<string>($"SELECT SUBSTR (TBL_DC_KODE, 2, 3) AS DCEXT FROM DC_TABEL_DC_T");
        }

        public async Task<string> GetWinFunction() {
            return await OraPg?.ExecScalarAsync<string>($"SELECT SUBSTR(MAX(PERIODE), 3, 4) AS WINFUNCTION FROM DC_TRNH_HDR_T");
        }

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
                        TO_CHAR(tgl_doc, 'yyyyMMdd') = TO_CHAR(:xDate, 'yyyyMMdd')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

        public async Task<int> CekLogTtfHdr(DateTime xDate) {
            return await OraPg?.ExecScalarAsync<int>(
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
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

        public async Task<string> CekRunTtfHdr(DateTime xDate) {
            return await OraPg?.ExecScalarAsync<string>(
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
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

        public async Task<bool> InsertDcTtfHdrLog(DateTime xDate) {
            return await OraPg?.ExecQueryAsync(
                $@"
                    INSERT INTO dc_ttf_hdr_log (tbl_dc_kode, tgl_proses, tgl_doc, status_run)
                    VALUES (:KodeDc, {(_app.IsUsingPostgres ? "CURRENT_DATE" : "TRUNC(SYSDATE)")}, :xDate, '1')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

        public async Task<bool> UpdateDcTtfHdrLog(string columnValue, DateTime xDate) {
            return await OraPg?.ExecQueryAsync(
                $@"
                    UPDATE dc_ttf_hdr_log
                    SET {columnValue}
                    WHERE
                        TBL_DC_KODE = :KodeDc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

        public async Task<int> CekLogTtfHdr_status_ok(DateTime xDate) {
            return await OraPg?.ExecScalarAsync<int>(
                $@"
                    SELECT 1 FROM DC_TTF_HDR_LOG
                    WHERE
                        TBL_DC_KODE = :KodeDc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                        AND status_tax = 'OK'
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

        public async Task<bool> DeleteDcTtfDtlLog(DateTime xDate) {
            return await OraPg?.ExecQueryAsync(
                $@"
                    DELETE FROM dc_ttf_dtl_log
                    WHERE
                        TBL_DC_KODE = :KodeDc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

        public async Task<bool> DeleteDcTtfHdrLog(DateTime xDate) {
            return await OraPg?.ExecQueryAsync(
                $@"
                    DELETE FROM dc_ttf_hdr_log
                    WHERE
                        TBL_DC_KODE = :KodeDc
                        AND TO_CHAR(tgl_doc, 'dd/MM/yyyy') = TO_CHAR(:xDate, 'dd/MM/yyyy')
                ",
                new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "KodeDc", VALUE = await GetKodeDc() },
                    new CDbQueryParamBind { NAME = "xDate", VALUE = xDate }
                }
            );
        }

    }

}
