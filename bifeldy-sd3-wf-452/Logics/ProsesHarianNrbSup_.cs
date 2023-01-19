/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Tax Non-FAD
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Abstractions;
using bifeldy_sd3_lib_452.Databases;
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Models;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianNrbSup : ILogics { }

    public sealed class CProsesHarianNrbSup : CLogics, IProsesHarianNrbSup {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IOracle _oracle;
        private readonly IPostgres _postgres;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;
        private readonly IBranchCabang _branchCabang;

        public CProsesHarianNrbSup(
            IApp app,
            ILogger logger,
            IDb db,
            IOracle oracle,
            IPostgres postgres,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t,
            IBranchCabang branchCabang
        ) : base(db) {
            _app = app;
            _logger = logger;
            _db = db;
            _oracle = oracle;
            _postgres = postgres;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
            _branchCabang = branchCabang;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid() && IsDateRangeSameMonth() && await IsDateStartMaxYesterday() && await IsDateEndMaxYesterday()) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    JumlahServerKirimZip = 1;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    string kodeDCInduk = await _db.GetKodeDCInduk();
                    List<DC_TABEL_V> listBranchConnection = _branchCabang.GetListConnection(kodeDCInduk);
                    List<string> ftpFileKirim = new List<string>();

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);
                        _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);

                        string zipFileName = null;

                        foreach (DC_TABEL_V lbc in listBranchConnection) {
                            CDatabase db = null;
                            if (lbc.FLAG_DBPG == "Y") {
                                db = _postgres.NewExternalConnection(lbc.DBPG_IP, lbc.DBPG_PORT, lbc.DBPG_USER, lbc.DBPG_PASS, lbc.DBPG_NAME);
                            }
                            else {
                                db = _oracle.NewExternalConnection(lbc.IP_DB, lbc.DB_PORT, lbc.DB_USER_NAME, lbc.DB_PASSWORD, lbc.DB_SID);
                            }

                            string procName = await db.ExecScalarAsync<string>(
                                $@"
                                    SELECT FILE_PROCEDURE
                                    FROM DC_FILE_SCHEDULER_T
                                    WHERE file_key = :file_key
                                ",
                                new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "file_key", VALUE = "NRBSUP" }
                                }
                            );
                            CDbExecProcResult res = await db.ExecProcedureAsync(
                                procName,
                                new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "P_TGL", VALUE = xDate }
                                }
                            );
                            if (res == null || !res.STATUS) {
                                throw new Exception($"Gagal Menjalankan Procedure {procName}");
                            }

                            if (lbc.TBL_DC_KODE == kodeDCInduk) {
                                zipFileName = await db.ExecScalarAsync<string>(
                                    $@"
                                        SELECT {(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)
                                        FROM Q_TRF_CSV
                                        WHERE q_filename = :q_filename
                                    ",
                                    new List<CDbQueryParamBind> {
                                        new CDbQueryParamBind { NAME = "q_filename", VALUE = "NRBSUP" }
                                    }
                                );
                            }

                            string seperator = await db.ExecScalarAsync<string>(
                                $@"
                                    SELECT q_seperator
                                    FROM Q_TRF_CSV
                                    WHERE q_filename = :q_filename
                                ",
                                new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "q_filename", VALUE = "NRBSUP" }
                                }
                            );
                            string queryForCSV = await db.ExecScalarAsync<string>(
                                $@"
                                    SELECT q_query
                                    FROM Q_TRF_CSV
                                    WHERE q_filename = :q_filename
                                ",
                                new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "q_filename", VALUE = "NRBSUP" }
                                }
                            );
                            string filename = await db.ExecScalarAsync<string>(
                                $@"
                                    SELECT q_namafile
                                    FROM Q_TRF_CSV
                                    WHERE q_filename = :q_filename
                                ",
                                new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "q_filename", VALUE = "NRBSUP" }
                                }
                            );

                            if (string.IsNullOrEmpty(seperator) || string.IsNullOrEmpty(queryForCSV) || string.IsNullOrEmpty(filename)) {
                                string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                                MessageBox.Show(status_error, $"{button.Text} :: NRBSUP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else {
                                try {
                                    DataTable dtQueryRes = await db.GetDataTableAsync(queryForCSV);
                                    _berkas.DataTable2CSV(dtQueryRes, filename, seperator);
                                    // _berkas.ListFileForZip.Add(filename);
                                }
                                catch (Exception ex) {
                                    MessageBox.Show(ex.Message, $"{button.Text} :: NRBSUP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }

                        // _berkas.ZipListFileInFolder(zipFileName);
                        _berkas.ZipAllFileInFolder(zipFileName);
                        TargetKirim += JumlahServerKirimZip;

                        ftpFileKirim.Add(zipFileName);
                    }

                    BerhasilKirim += (await _dcFtpT.KirimSelectedZip("MDHO", ftpFileKirim, reportLog: true)).Success.Count; // *.ZIP Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
