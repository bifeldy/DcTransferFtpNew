/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Tabel Q_TRF_CSV
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

namespace DcTransferFtpNew.Handlers {

    public interface IQTrfCsv {
        Task<(bool, bool)> CreateCSVFile(string filename, string q_filename, string outputFolderPath = null, string appendTargetName = null, bool addToQueueForZip = true);
    }

    public sealed class CQTrfCsv : IQTrfCsv {

        private readonly IDb _db;
        private readonly IBerkas _berkas;

        public CQTrfCsv(IDb db, IBerkas berkas) {
            _db = db;
            _berkas = berkas;
        }

        public async Task<(bool, bool)> CreateCSVFile(string filename, string q_filename, string outputFolderPath = null, string appendTargetName = null, bool addToQueueForZip = true) {
            bool res = false;
            string seperator = await _db.Q_TRF_CSV__GET("q_seperator", q_filename);
            string queryForCSV = await _db.Q_TRF_CSV__GET("q_query", q_filename);
            filename = await _db.Q_TRF_CSV__GET("q_namafile", q_filename) ?? filename;
            if (string.IsNullOrEmpty(seperator) || string.IsNullOrEmpty(queryForCSV) || string.IsNullOrEmpty(filename)) {
                MessageBox.Show("Data CSV (Separator / Query / Nama File) Dari Tabel Q_TRF_CSV Tidak Lengkap!", $"{q_filename} :: {filename}");
            }
            else {
                if (appendTargetName != null) {
                    filename += appendTargetName;
                }
                (DataTable dtQuery, Exception ex) = await _db.OraPg.GetDataTableAsync(queryForCSV);
                if (ex == null) {
                    res = _berkas.DataTable2CSV(dtQuery, filename, seperator, outputFolderPath);
                    if (addToQueueForZip) {
                        _berkas.ListFileForZip.Add(filename);
                    }
                }
            }
            return (res, addToQueueForZip);
        }

    }

}
