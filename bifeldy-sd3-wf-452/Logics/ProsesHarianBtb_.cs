/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian BTB
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
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianBtb : ILogics { }

    public sealed class CProsesHarianBtb : CLogics, IProsesHarianBtb {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IDcFtpT _dcFtpT;
        private readonly IBranchCabang _branchCabang;

        public CProsesHarianBtb(
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
                if (IsDateRangeValid() && IsDateRangeSameMonth() && await IsDateStartMaxYesterday() && await IsDateEndMaxYesterday()) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    JumlahServerKirimZip = 1;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    string kodeDCInduk = await _db.GetKodeDCInduk();
                    List<DC_TABEL_V> listBranchDbInfo = await _branchCabang.GetListBranchDbInformation(kodeDCInduk);
                    List<string> ftpFileKirim = new List<string>();

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string tempFolder = Path.Combine(_berkas.TempFolderPath, $"BTB_{xDate:yyyy-MM-dd}");
                        if (!Directory.Exists(tempFolder)) {
                            Directory.CreateDirectory(tempFolder);
                        }
                        _berkas.DeleteOldFilesInFolder(tempFolder, 0);

                        string zipFileName = null;

                        foreach (DC_TABEL_V lbdi in listBranchDbInfo) {
                            CDatabase lbdiDbOraPg = null;
                            if (lbdi.FLAG_DBPG == "Y") {
                                lbdiDbOraPg = _db.NewExternalConnectionPg(lbdi.DBPG_IP, lbdi.DBPG_PORT, lbdi.DBPG_USER, lbdi.DBPG_PASS, lbdi.DBPG_NAME);
                            }
                            else {
                                lbdiDbOraPg = _db.NewExternalConnectionOra(lbdi.IP_DB, lbdi.DB_PORT, lbdi.DB_USER_NAME, lbdi.DB_PASSWORD, lbdi.DB_SID);
                            }

                            List<CDbQueryParamBind> btb = new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "btb", VALUE = "BTB" }
                            };

                            string procName = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT FILE_PROCEDURE FROM DC_FILE_SCHEDULER_T WHERE file_key = :btb",
                                btb
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
                                zipFileName = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"
                                        SELECT {(lbdi.FLAG_DBPG == "Y" ? "COALESCE" : "NVL")}(q_namazip, q_namafile)
                                        FROM Q_TRF_CSV WHERE q_filename = :btb
                                    ",
                                    btb
                                );
                            }

                            string seperator = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_seperator FROM Q_TRF_CSV WHERE q_filename = :btb",
                                btb
                            );
                            string queryForCSV = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_query FROM Q_TRF_CSV WHERE q_filename = :btb",
                                btb
                            );
                            string filename = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT q_namafile FROM Q_TRF_CSV WHERE q_filename = :btb",
                                btb
                            );

                            if (string.IsNullOrEmpty(seperator) || string.IsNullOrEmpty(queryForCSV) || string.IsNullOrEmpty(filename)) {
                                string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                                MessageBox.Show(status_error, $"{button.Text} :: BTB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else {
                                try {
                                    DataTable dtQueryRes = await lbdiDbOraPg.GetDataTableAsync(queryForCSV);
                                    _berkas.DataTable2CSV(dtQueryRes, filename, seperator, tempFolder);
                                    _berkas.ListFileForZip.Add(filename);
                                }
                                catch (Exception ex) {
                                    MessageBox.Show(ex.Message, $"{button.Text} :: BTB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }

                        _berkas.ZipListFileInFolder(zipFileName, folderPath: tempFolder);
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
