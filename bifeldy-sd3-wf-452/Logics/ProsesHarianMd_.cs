/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian MD
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Abstractions;
using bifeldy_sd3_lib_452.Handlers;
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianMd : ILogics { }

    public sealed class CProsesHarianMd : CLogics, IProsesHarianMd {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IDcFtpT _dcFtpT;
        private readonly IBranchCabang _branchCabang;

        public CProsesHarianMd(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IDcFtpT dc_ftp_t,
            IBranchCabang branchCabang
        ) : base(db, berkas) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _dcFtpT = dc_ftp_t;
            _branchCabang = branchCabang;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid() && IsDateRangeSameMonth()) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    string kodeDCInduk = await _db.GetKodeDCInduk();
                    List<DC_TABEL_V> listBranchDbInfo = await _branchCabang.GetListBranchDbInformation(kodeDCInduk);

                    List<string> ftpFileKirimSt = new List<string>();
                    List<string> ftpFileKirimSl = new List<string>();

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string stFolder = Path.Combine(_berkas.TempFolderPath, $"ST_{xDate:yyyy-MM-dd}");
                        if (!Directory.Exists(stFolder)) {
                            Directory.CreateDirectory(stFolder);
                        }
                        _berkas.DeleteOldFilesInFolder(stFolder, 0);

                        string slFolder = Path.Combine(_berkas.TempFolderPath, $"SL_{xDate:yyyy-MM-dd}");
                        if (!Directory.Exists(slFolder)) {
                            Directory.CreateDirectory(slFolder);
                        }
                        _berkas.DeleteOldFilesInFolder(slFolder, 0);

                        string zipFileNameSt = null;
                        string zipFileNameSl = null;

                        foreach (DC_TABEL_V lbdi in listBranchDbInfo) {
                            CDatabase lbdiDbOraPg = null;
                            if (lbdi.FLAG_DBPG == "Y") {
                                lbdiDbOraPg = _db.NewExternalConnectionPg(lbdi.DBPG_IP, lbdi.DBPG_PORT, lbdi.DBPG_USER, lbdi.DBPG_PASS, lbdi.DBPG_NAME);
                            }
                            else {
                                lbdiDbOraPg = _db.NewExternalConnectionOra(lbdi.IP_DB, lbdi.DB_PORT, lbdi.DB_USER_NAME, lbdi.DB_PASSWORD, lbdi.DB_SID);
                            }

                            List<CDbQueryParamBind> stMd = new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "st_md", VALUE = "ST_MD" }
                            };

                            string procName = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT FILE_PROCEDURE FROM DC_FILE_SCHEDULER_T WHERE file_key = :st_md",
                                stMd
                            );
                            CDbExecProcResult res = await lbdiDbOraPg.ExecProcedureAsync(
                                procName,
                                new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "P_TGL", VALUE = xDate }
                                }
                            );
                            if (res == null || !res.STATUS) {
                                throw new Exception($"Gagal Menjalankan Procedure {procName}");
                            }

                            if (lbdi.TBL_DC_KODE == kodeDCInduk) {
                                zipFileNameSt = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"
                                        SELECT {(lbdi.FLAG_DBPG == "Y" ? "COALESCE" : "NVL")}(q_namazip, q_namafile)
                                        FROM Q_TRF_CSV WHERE q_filename = :st_md
                                    ",
                                    stMd
                                );
                            }

                            /* ST */

                            string stMdSeperator = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_seperator FROM Q_TRF_CSV WHERE q_filename = :st_md",
                                stMd
                            );
                            string stMdQueryForCSV = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_query FROM Q_TRF_CSV WHERE q_filename = :st_md",
                                stMd
                            );
                            string stMdFilename = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_namafile FROM Q_TRF_CSV WHERE q_filename = :st_md",
                                stMd
                            );

                            if (string.IsNullOrEmpty(stMdSeperator) || string.IsNullOrEmpty(stMdQueryForCSV) || string.IsNullOrEmpty(stMdFilename)) {
                                string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                                MessageBox.Show(status_error, $"{button.Text} :: NRBSUP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else {
                                try {
                                    DataTable dtQueryRes = await lbdiDbOraPg.GetDataTableAsync(stMdQueryForCSV);
                                    _berkas.DataTable2CSV(dtQueryRes, stMdFilename, stMdSeperator, stFolder);
                                    // _berkas.ListFileForZip.Add(stMdFilename);
                                }
                                catch (Exception ex) {
                                    MessageBox.Show(ex.Message, $"{button.Text} :: ST_MD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }

                            /* JKB => ST */

                            List<CDbQueryParamBind> jkb = new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "jkb", VALUE = "JKB" }
                            };

                            string jkbSeperator = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_seperator FROM Q_TRF_CSV WHERE q_filename = :jkb",
                                jkb
                            );
                            string jkbQueryForCSV = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_query FROM Q_TRF_CSV WHERE q_filename = :jkb",
                                jkb
                            );
                            string jkbFilename = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_namafile FROM Q_TRF_CSV WHERE q_filename = :jkb",
                                jkb
                            );

                            if (!string.IsNullOrEmpty(jkbSeperator) && !string.IsNullOrEmpty(jkbQueryForCSV) && !string.IsNullOrEmpty(jkbFilename)) {
                                try {
                                    DataTable dtQueryRes = await lbdiDbOraPg.GetDataTableAsync(jkbQueryForCSV);
                                    _berkas.DataTable2CSV(dtQueryRes, jkbFilename, jkbSeperator, stFolder);
                                    // _berkas.ListFileForZip.Add(jkbFilename);
                                }
                                catch (Exception ex) {
                                    MessageBox.Show(ex.Message, $"{button.Text} :: JKB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }

                            /* SL */

                            List<CDbQueryParamBind> slMd = new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "sl_md", VALUE = "SL_MD" }
                            };

                            if (lbdi.TBL_DC_KODE == kodeDCInduk) {
                                zipFileNameSl = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"
                                        SELECT {(lbdi.FLAG_DBPG == "Y" ? "COALESCE" : "NVL")}(q_namazip, q_namafile)
                                        FROM Q_TRF_CSV WHERE q_filename = :sl_md
                                    ",
                                    slMd
                                );
                            }

                            string slMdSeperator = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_seperator FROM Q_TRF_CSV WHERE q_filename = :sl_md",
                                slMd
                            );
                            string slMdQueryForCSV = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_query FROM Q_TRF_CSV WHERE q_filename = :sl_md",
                                slMd
                            );
                            string slMdFilename = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_namafile FROM Q_TRF_CSV WHERE q_filename = :sl_md",
                                slMd
                            );

                            if (string.IsNullOrEmpty(slMdSeperator) || string.IsNullOrEmpty(slMdQueryForCSV) || string.IsNullOrEmpty(slMdFilename)) {
                                string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                                MessageBox.Show(status_error, $"{button.Text} :: NRBSUP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else {
                                try {
                                    DataTable dtQueryRes = await lbdiDbOraPg.GetDataTableAsync(slMdQueryForCSV);
                                    _berkas.DataTable2CSV(dtQueryRes, slMdFilename, slMdSeperator, slFolder);
                                    // _berkas.ListFileForZip.Add(slMdFilename);
                                }
                                catch (Exception ex) {
                                    MessageBox.Show(ex.Message, $"{button.Text} :: SL_MD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }

                        }

                        _berkas.ZipAllFileInFolder(zipFileNameSt, stFolder);
                        TargetKirim += 2;
                        ftpFileKirimSt.Add(zipFileNameSt);

                        _berkas.ZipAllFileInFolder(zipFileNameSl, slFolder);
                        TargetKirim += 1;
                        ftpFileKirimSl.Add(zipFileNameSl);
                    }

                    BerhasilKirim += (await _dcFtpT.KirimSelectedZip("MDHO", ftpFileKirimSt, reportLog: true)).Success.Count; // *.ZIP Sebanyak :: 2 * jumlahHari
                    BerhasilKirim += (await _dcFtpT.KirimSelectedZip("JKBWRC", ftpFileKirimSt, reportLog: true)).Success.Count; // *.ZIP Sebanyak :: 2 * jumlahHari
                    BerhasilKirim += (await _dcFtpT.KirimSelectedZip("MDHO", ftpFileKirimSl, reportLog: true)).Success.Count; // *.ZIP Sebanyak :: 1 * jumlahHari

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
