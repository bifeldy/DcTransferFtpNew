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
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

namespace DcTransferFtpNew.Handlers {

    public interface IQTrfCsv {
        Task<bool> CreateCSVFile(string q_filename, string filename = null, string seperator = null, string outputFolderPath = null, string appendTargetName = null, bool addToQueueForZip = true, bool required = true);
        Task<List<string>> GetFileNameMulti(List<string> q_filename);
    }

    public sealed class CQTrfCsv : IQTrfCsv {

        private readonly IDb _db;
        private readonly IBerkas _berkas;

        public CQTrfCsv(IDb db, IBerkas berkas) {
            _db = db;
            _berkas = berkas;
        }

        public async Task<bool> CreateCSVFile(string q_filename, string filename = null, string seperator = null, string outputFolderPath = null, string appendTargetName = null, bool addToQueueForZip = true, bool required = true) {
            bool res = false;
            filename = await _db.Q_TRF_CSV__GET("q_namafile", q_filename) ?? filename;
            seperator = await _db.Q_TRF_CSV__GET("q_seperator", q_filename) ?? seperator;
            string queryForCSV = await _db.Q_TRF_CSV__GET("q_query", q_filename);
            if (string.IsNullOrEmpty(seperator) || string.IsNullOrEmpty(queryForCSV) || string.IsNullOrEmpty(filename)) {
                if (required) {
                    string dataTidakLengkap = $"Data CSV (Separator / Query / Nama File) Tidak Tersedia";
                    DialogResult dr = MessageBox.Show(
                        $"{dataTidakLengkap}, Ingin Melanjutkan ?",
                        $"Gagal Membuat CSV {q_filename}",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (dr == DialogResult.No) {
                        throw new Exception($"Data {q_filename} Pada Tabel Q_TRF_CSV Tidak Lengkap");
                    }
                }
            }
            else {
                if (appendTargetName != null) {
                    filename += appendTargetName;
                }
                DataTable dtQuery = await _db.OraPg_GetDataTable(queryForCSV);
                res = _berkas.DataTable2CSV(dtQuery, filename, seperator, outputFolderPath);
                if (addToQueueForZip) {
                    _berkas.ListFileForZip.Add(filename);
                }
            }
            return res;
        }

        public async Task<List<string>> GetFileNameMulti(List<string> q_filename) {
            List<string> fileNames = new List<string>();
            foreach(string q in q_filename) {
                fileNames.Add(await _db.Q_TRF_CSV__GET("q_namafile", q));
            }
            return fileNames;
        }

    }

}
