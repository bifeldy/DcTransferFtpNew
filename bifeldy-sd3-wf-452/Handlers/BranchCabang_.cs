/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Branch Connection Induk & Cabangnya
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using bifeldy_sd3_lib_452.Abstractions;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Handlers {

    public interface IBranchCabang {
        Task<List<DC_TABEL_V>> GetListBranchDbInformation(string kodeDcInduk);
        Task<IDictionary<string, CDatabase>> GetListBranchDbConnection(string kodeDcInduk);
    }

    public sealed class CBranchCabang : IBranchCabang {

        private readonly IApi _api;
        private readonly IDb _db;
        private readonly ILogger _logger;
        private readonly IConverter _converter;

        private IDictionary<
            string, IDictionary<
                string, CDatabase
            >
        > BranchConnectionInfo { get; } = new Dictionary<
            string, IDictionary<
                string, CDatabase
            >
        >();

        public CBranchCabang(IApi api, IDb db, ILogger logger, IConverter converter) {
            _api = api;
            _db = db;
            _logger = logger;
            _converter = converter;
        }

        public async Task<List<DC_TABEL_V>> GetListBranchDbInformation(string kodeDcInduk) {
            string url = await _db.GetURLWebService("SYNCHO"); // ?? _config.Get<string>("WsSyncHo", _app.GetConfig("ws_syncho"));
            url += kodeDcInduk;

            HttpResponseMessage httpResponse = await _api.PostData(url, null);
            string httpResString = await httpResponse.Content.ReadAsStringAsync();

            return _converter.JsonToObj<List<DC_TABEL_V>>(httpResString);
        }

        // Sepertinya Yang Ini Akan Kurang Berguna
        // Karena Dapat Akses Langsung Ke Database
        // Cuma Tahu `CDatabase` Tidak Tahu Postgre / Oracle
        //
        // var dbCon = await GetListBranchDbConnection("G001");
        // var res = dbCon["G055"].ExecScalarAsync<...>(...);
        //

        public async Task<IDictionary<string, CDatabase>> GetListBranchDbConnection(string kodeDcInduk) {
            IDictionary<string, CDatabase> dbCons = new Dictionary<string, CDatabase>();

            try {
                List<DC_TABEL_V> dbInfo = await GetListBranchDbInformation(kodeDcInduk);
                foreach (DC_TABEL_V dbi in dbInfo) {
                    CDatabase dbCon;
                    if (dbi.FLAG_DBPG == "Y") {
                        dbCon = _db.NewExternalConnectionPg(dbi.DBPG_IP, dbi.DBPG_PORT, dbi.DBPG_USER, dbi.DBPG_PASS, dbi.DBPG_NAME);
                    }
                    else {
                        dbCon = _db.NewExternalConnectionOra(dbi.IP_DB, dbi.DB_PORT, dbi.DB_USER_NAME, dbi.DB_PASSWORD, dbi.DB_SID);
                    }
                    dbCons.Add(dbi.TBL_DC_KODE, dbCon);
                }
                BranchConnectionInfo[kodeDcInduk] = dbCons;
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
            }

            return BranchConnectionInfo[kodeDcInduk];
        }

    }

}
