/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian BO
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

    public interface IProsesHarianBo : ILogics { }

    public sealed class CProsesHarianBo : CLogics, IProsesHarianBo {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IDcFtpT _dcFtpT;
        private readonly IBranchCabangHandler _branchCabang;

        public CProsesHarianBo(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IDcFtpT dc_ftp_t,
            IBranchCabangHandler branchCabang
        ) : base(db, csv, zip) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _csv = csv;
            _zip = zip;
            _dcFtpT = dc_ftp_t;
            _branchCabang = branchCabang;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid() && IsDateRangeSameMonth()) {
                    _berkas.BackupAllFilesInFolder(_csv.CsvFolderPath);
                    _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, 0);
                    _berkas.BackupAllFilesInFolder(_zip.ZipFolderPath);
                    _berkas.DeleteOldFilesInFolder(_zip.ZipFolderPath, 0);
                    JumlahServerKirimCsv = 1;
                    JumlahServerKirimZip = 1;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    string kodeDCInduk = await _db.GetKodeDCInduk();
                    List<DC_TABEL_V> listBranchDbInfo = await _branchCabang.GetListBranchDbInformation(kodeDCInduk);

                    string zipAvgCostFileName = null;

                    List<string> csvFileKirimJarak = new List<string>();
                    List<string> csvFileKirimExpedisi = new List<string>();

                    string avgCostFolder = Path.Combine(_csv.CsvFolderPath, $"BO_AVGCOST");
                    if (!Directory.Exists(avgCostFolder)) {
                        Directory.CreateDirectory(avgCostFolder);
                    }
                    _berkas.DeleteOldFilesInFolder(avgCostFolder, 0);

                    string jarakFolder = Path.Combine(_csv.CsvFolderPath, $"BO_JARAK");
                    if (!Directory.Exists(jarakFolder)) {
                        Directory.CreateDirectory(jarakFolder);
                    }
                    _berkas.DeleteOldFilesInFolder(jarakFolder, 0);

                    string expedisiFolder = Path.Combine(_csv.CsvFolderPath, $"BO_EXPEDISI");
                    if (!Directory.Exists(expedisiFolder)) {
                        Directory.CreateDirectory(expedisiFolder);
                    }
                    _berkas.DeleteOldFilesInFolder(expedisiFolder, 0);

                    // File avg cost sifatnya bulanan
                    // jadi jeda tanggal gk ngaruh karena file formanya nya YYYYMM jadi cukup 1 file dalam jeda
                    // tapi untuk file jarak HARIAN ikutin jumlah harinya

                    foreach (DC_TABEL_V lbdi in listBranchDbInfo) {
                        bool isAvgCostMonthlyCreated = false;

                        CDatabase lbdiDbOraPg = null;
                        if (lbdi.FLAG_DBPG == "Y") {
                            lbdiDbOraPg = _db.NewExternalConnectionPg(lbdi.DBPG_IP, lbdi.DBPG_PORT, lbdi.DBPG_USER, lbdi.DBPG_PASS, lbdi.DBPG_NAME);
                        }
                        else {
                            lbdiDbOraPg = _db.NewExternalConnectionOra(lbdi.IP_DB, lbdi.DB_PORT, lbdi.DB_USER_NAME, lbdi.DB_PASSWORD, lbdi.DB_SID);
                        }

                        for (int i = 0; i < jumlahHari; i++) {
                            DateTime xDate = dateStart.AddDays(i);

                            List<CDbQueryParamBind> bo = new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "bo", VALUE = "BO" }
                            };

                            string procName = await lbdiDbOraPg.ExecScalarAsync<string>(
                                $@"SELECT FILE_PROCEDURE FROM DC_FILE_SCHEDULER_T WHERE file_key = :bo",
                                bo
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

                            if (!isAvgCostMonthlyCreated) {

                                List<CDbQueryParamBind> dcAvgCost = new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "dc_avg_cost", VALUE = "DCAVGCOST" }
                                };

                                if (lbdi.TBL_DC_KODE == kodeDCInduk) {
                                    zipAvgCostFileName = await lbdiDbOraPg.ExecScalarAsync<string>(
                                        $@"
                                            SELECT {(lbdi.FLAG_DBPG == "Y" ? "COALESCE" : "NVL")}(q_namazip, q_namafile)
                                            FROM Q_TRF_CSV WHERE q_filename = :dc_avg_cost
                                        ",
                                        dcAvgCost
                                    );
                                }

                                string seperator = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_seperator FROM Q_TRF_CSV WHERE q_filename = :dc_avg_cost",
                                    dcAvgCost
                                );
                                string queryForCSV = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_query FROM Q_TRF_CSV WHERE q_filename = :dc_avg_cost",
                                    dcAvgCost
                                );
                                string filename = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_namafile FROM Q_TRF_CSV WHERE q_filename = :dc_avg_cost",
                                    dcAvgCost
                                );

                                if (string.IsNullOrEmpty(seperator) || string.IsNullOrEmpty(queryForCSV) || string.IsNullOrEmpty(filename)) {
                                    string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                                    MessageBox.Show(status_error, $"{button.Text} :: DCAVGCOST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else {
                                    try {
                                        DataTable dtQueryRes = await lbdiDbOraPg.GetDataTableAsync(queryForCSV);
                                        _csv.DataTable2CSV(dtQueryRes, filename, seperator, avgCostFolder);
                                        // _zip.ListFileForZip.Add(filename);
                                    }
                                    catch (Exception ex) {
                                        MessageBox.Show(ex.Message, $"{button.Text} :: DCAVGCOST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }

                                isAvgCostMonthlyCreated = true;
                            }

                            // Hanya Dc Tertentu
                            string jenisDc = await lbdiDbOraPg.ExecScalarAsync<string>("SELECT TBL_JENIS_DC FROM DC_TABEL_DC_T");

                            // Induk Saja
                            if (jenisDc == "INDUK") {
                                string csvFileNameJarak = $"JARAK{lbdi.TBL_DC_KODE}{xDate:yyyyMMdd}.CSV";

                                List<CDbQueryParamBind> jarak = new List<CDbQueryParamBind> {
                                        new CDbQueryParamBind { NAME = "jarak", VALUE = "JARAK" }
                                    };

                                string seperatorJarak = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_seperator FROM Q_TRF_CSV WHERE q_filename = :jarak",
                                    jarak
                                );
                                string queryForCSVJarak = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_query FROM Q_TRF_CSV WHERE q_filename = :jarak",
                                    jarak
                                );
                                string filenameJarak = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_namafile FROM Q_TRF_CSV WHERE q_filename = :jarak",
                                    jarak
                                );

                                if (!string.IsNullOrEmpty(seperatorJarak) && !string.IsNullOrEmpty(queryForCSVJarak) && !string.IsNullOrEmpty(filenameJarak)) {
                                    try {
                                        DataTable dtQueryRes = await lbdiDbOraPg.GetDataTableAsync(queryForCSVJarak);
                                        _csv.DataTable2CSV(dtQueryRes, filenameJarak, seperatorJarak, jarakFolder);
                                        // _zip.ListFileForZip.Add(filename);
                                        TargetKirim += JumlahServerKirimCsv;

                                        csvFileKirimJarak.Add(filenameJarak);
                                    }
                                    catch (Exception ex) {
                                        MessageBox.Show(ex.Message, $"{button.Text} :: JARAK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }

                            // Lebih Dari 1 Jenis
                            List<string> multiJenisDc = new List<string> { "INDUK", "DEPO" };
                            if (multiJenisDc.Contains(jenisDc)) {
                                string csvFileNameExpedisi = $"EXPEDISI{lbdi.TBL_DC_KODE}{xDate:yyyyMMdd}.CSV";

                                List<CDbQueryParamBind> expedisi = new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "expedisi", VALUE = "TOKOEXPEDISI" }
                                };

                                string seperatorExpedisi = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_seperator FROM Q_TRF_CSV WHERE q_filename = :expedisi",
                                    expedisi
                                );
                                string queryForCSVExpedisi = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_query FROM Q_TRF_CSV WHERE q_filename = :expedisi",
                                    expedisi
                                );
                                string filenameExpedisi = await lbdiDbOraPg.ExecScalarAsync<string>(
                                    $@"SELECT q_namafile FROM Q_TRF_CSV WHERE q_filename = :expedisi",
                                    expedisi
                                );

                                if (!string.IsNullOrEmpty(seperatorExpedisi) && !string.IsNullOrEmpty(queryForCSVExpedisi) && !string.IsNullOrEmpty(filenameExpedisi)) {
                                    try {
                                        DataTable dtQueryRes = await lbdiDbOraPg.GetDataTableAsync(queryForCSVExpedisi);
                                        _csv.DataTable2CSV(dtQueryRes, filenameExpedisi, seperatorExpedisi, expedisiFolder);
                                        // _zip.ListFileForZip.Add(filename);
                                        TargetKirim += JumlahServerKirimCsv;

                                        csvFileKirimExpedisi.Add(filenameExpedisi);
                                    }
                                    catch (Exception ex) {
                                        MessageBox.Show(ex.Message, $"{button.Text} :: EXPEDISI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                        }
                    }

                    _zip.ZipAllFileInFolder(zipAvgCostFileName, avgCostFolder);
                    TargetKirim += JumlahServerKirimZip;

                    BerhasilKirim += (await _dcFtpT.KirimSingleZip("MDHO", zipAvgCostFileName, reportLog: true)).Success.Count; // *.ZIP Sebanyak :: 1
                    BerhasilKirim += (await _dcFtpT.KirimSelectedCsv("MDHO", csvFileKirimJarak, jarakFolder, true)).Success.Count; // *.ZIP Sebanyak :: csvFileKirim
                    BerhasilKirim += (await _dcFtpT.KirimSelectedCsv("MDHO", csvFileKirimExpedisi, expedisiFolder, true)).Success.Count; // *.ZIP Sebanyak :: csvFileKirim

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
