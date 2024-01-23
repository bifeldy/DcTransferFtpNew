/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian AMTA
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianAmta : ILogics { }

    public sealed class CProsesHarianAmta : CLogics, IProsesHarianAmta {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianAmta(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IDcFtpT dc_ftp_t
        ) : base(db, berkas) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareHarian(sender, e, currentControl);
            await Task.Run(async () => {
                if (IsDateRangeValid()) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    JumlahServerKirimCsv = 2;

                    // { "namafile1", "columnDb1" }, { "namafile2", "columnDb2" }, ..., { "namafile*", "columnDb*" };
                    IDictionary<string, string> ftpFileKirim = new Dictionary<string, string>();

                    // Hanya Proses Data 1 Hari Tanggar Di Pilih
                    dateEnd = dateStart;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    string procName = "GET_AMTA_EVO";
                    CDbExecProcResult res = await _db.CALL__P_TGL(procName, dateStart);
                    if (res == null || !res.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName}");
                    }
                    await _db.InsertNewDcAmtaLog(dateStart);

                    /* AMTA_JADWAL_WEEKLY */

                    await _db.UpdateDcDcAmtaLog($"START_WEEKLY = NOW()", dateStart);

                    string seperator1 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_JADWAL_WEEKLY");
                    string queryForCSV1 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_JADWAL_WEEKLY");
                    string filename1 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_JADWAL_WEEKLY");

                    if (string.IsNullOrEmpty(seperator1) || string.IsNullOrEmpty(queryForCSV1) || string.IsNullOrEmpty(filename1)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_JADWAL_WEEKLY", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_WEEKLY = '{filename1}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV1);

                            _berkas.DataTable2CSV(dtQueryRes, filename1, seperator1);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename1, "STATUS_WEEKLY");
                            await _db.UpdateDcDcAmtaLog($"STATUS_WEEKLY = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_JADWAL_WEEKLY", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_WEEKLY = NOW()", dateStart);

                    /* AMTA_JADWAL_WEEKLY_TOKO */

                    await _db.UpdateDcDcAmtaLog($"START_EXCLBULFRAC = NOW()", dateStart);

                    string seperator2 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_JADWAL_WEEKLY_TOKO");
                    string queryForCSV2 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_JADWAL_WEEKLY_TOKO");
                    string filename2 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_JADWAL_WEEKLY_TOKO");

                    if (string.IsNullOrEmpty(seperator2) || string.IsNullOrEmpty(queryForCSV2) || string.IsNullOrEmpty(filename2)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_JADWAL_WEEKLY_TOKO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_EXCLBULFRAC = '{filename2}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV2);

                            _berkas.DataTable2CSV(dtQueryRes, filename2, seperator2);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename2, "STATUS_EXCLBULFRAC");
                            await _db.UpdateDcDcAmtaLog($"STATUS_EXCLBULFRAC = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_JADWAL_WEEKLY_TOKO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_EXCLBULFRAC = NOW()", dateStart);

                    /* AMTA_PLANO */

                    await _db.UpdateDcDcAmtaLog($"START_PLANO = NOW()", dateStart);

                    string seperator3 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_PLANO");
                    string queryForCSV3 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_PLANO");
                    string filename3 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_PLANO");

                    if (string.IsNullOrEmpty(seperator3) || string.IsNullOrEmpty(queryForCSV3) || string.IsNullOrEmpty(filename3)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_PLANO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_PLANO = '{filename3}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV3);

                            _berkas.DataTable2CSV(dtQueryRes, filename3, seperator3);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename3, "STATUS_PLANO");
                            await _db.UpdateDcDcAmtaLog($"STATUS_PLANO = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_PLANO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_PLANO = NOW()", dateStart);

                    /* AMTA_ITEM_DEPO */

                    await _db.UpdateDcDcAmtaLog($"START_ITEMDEPO = NOW()", dateStart);

                    string seperator4 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_ITEM_DEPO");
                    string queryForCSV4 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_ITEM_DEPO");
                    string filename4 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_ITEM_DEPO");

                    if (string.IsNullOrEmpty(seperator4) || string.IsNullOrEmpty(queryForCSV4) || string.IsNullOrEmpty(filename4)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_ITEM_DEPO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_ITEMDEPO = '{filename4}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV4);

                            _berkas.DataTable2CSV(dtQueryRes, filename4, seperator4);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename4, "STATUS_ITEMDEPO");
                            await _db.UpdateDcDcAmtaLog($"STATUS_ITEMDEPO = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_ITEM_DEPO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_ITEMDEPO = NOW()", dateStart);

                    /* AMTA_JADWAL_BULKY */

                    await _db.UpdateDcDcAmtaLog($"START_BULFRAC = NOW()", dateStart);

                    string seperator5 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_JADWAL_BULKY");
                    string queryForCSV5 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_JADWAL_BULKY");
                    string filename5 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_JADWAL_BULKY");

                    if (string.IsNullOrEmpty(seperator5) || string.IsNullOrEmpty(queryForCSV5) || string.IsNullOrEmpty(filename5)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_JADWAL_BULKY", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_BULFRAC = '{filename5}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV5);

                            _berkas.DataTable2CSV(dtQueryRes, filename5, seperator5);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename5, "STATUS_BULFRAC");
                            await _db.UpdateDcDcAmtaLog($"STATUS_BULFRAC = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_JADWAL_BULKY", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_BULFRAC = NOW()", dateStart);

                    /* AMTA_TOKO_KHUSUS */

                    await _db.UpdateDcDcAmtaLog($"START_TOKOKHUSUS = NOW()", dateStart);

                    string seperator6 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_TOKO_KHUSUS");
                    string queryForCSV6 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_TOKO_KHUSUS");
                    string filename6 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_TOKO_KHUSUS");

                    if (string.IsNullOrEmpty(seperator6) || string.IsNullOrEmpty(queryForCSV6) || string.IsNullOrEmpty(filename6)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_TOKO_KHUSUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_TOKOKHUSUS = '{filename6}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV6);

                            _berkas.DataTable2CSV(dtQueryRes, filename6, seperator6);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename6, "STATUS_TOKOKHUSUS");
                            await _db.UpdateDcDcAmtaLog($"STATUS_TOKOKHUSUS = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_TOKO_KHUSUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_TOKOKHUSUS = NOW()", dateStart);

                    /* AMTA_ST */

                    await _db.UpdateDcDcAmtaLog($"START_ST = NOW()", dateStart);

                    string seperator7 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_ST");
                    string queryForCSV7 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_ST");
                    string filename7 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_ST");

                    if (string.IsNullOrEmpty(seperator7) || string.IsNullOrEmpty(queryForCSV7) || string.IsNullOrEmpty(filename7)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_ST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_ST = '{filename7}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV7);

                            _berkas.DataTable2CSV(dtQueryRes, filename7, seperator7);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename7, "STATUS_ST");
                            await _db.UpdateDcDcAmtaLog($"STATUS_ST = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_ST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_ST = NOW()", dateStart);

                    /* AMTA_SPLITDPD */

                    await _db.UpdateDcDcAmtaLog($"START_SPLITDPD = NOW()", dateStart);

                    string seperator8 = await _db.Q_TRF_CSV__GET("q_seperator", "AMTA_SPLITDPD");
                    string queryForCSV8 = await _db.Q_TRF_CSV__GET("q_query", "AMTA_SPLITDPD");
                    string filename8 = await _db.Q_TRF_CSV__GET("q_namafile", "AMTA_SPLITDPD");

                    if (string.IsNullOrEmpty(seperator8) || string.IsNullOrEmpty(queryForCSV8) || string.IsNullOrEmpty(filename8)) {
                        string status_error = "Data CSV (Separator / Query / Nama File) Tidak Lengkap!";
                        MessageBox.Show(status_error, $"{button.Text} :: AMTA_SPLITDPD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        await _db.UpdateDcDcAmtaLog($"FILE_SPLITDPD = '{filename8}'", dateStart);

                        try {
                            DataTable dtQueryRes = await _db.OraPg_GetDataTable(queryForCSV8);

                            _berkas.DataTable2CSV(dtQueryRes, filename8, seperator8);
                            // _berkas.ListFileForZip.Add(filename);
                            TargetKirim += JumlahServerKirimCsv;

                            ftpFileKirim.Add(filename8, "STATUS_SPLITDPD");
                            await _db.UpdateDcDcAmtaLog($"STATUS_SPLITDPD = ''", dateStart);
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, $"{button.Text} :: AMTA_SPLITDPD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    await _db.UpdateDcDcAmtaLog($"END_SPLITDPD = NOW()", dateStart);

                    /* ** */

                    CFtpResultInfo ftpResultInfo1 = await _dcFtpT.KirimAllCsv("AMTA", reportLog: true);
                    List<CFtpResultSendGet> resAll1 = ftpResultInfo1.Success.Concat(ftpResultInfo1.Fail).ToList();
                    foreach (CFtpResultSendGet resAll in resAll1) {
                        string fileName = resAll.FileInformation.Name;
                        string columnDb = ftpFileKirim[fileName];
                        bool success = resAll.FtpStatusSendGet;
                        await _db.UpdateDcDcAmtaLog($"{columnDb} = {columnDb} || 'AMTA {(success ? "Ok" : "Gagal")}'", dateStart);
                    }
                    BerhasilKirim += ftpResultInfo1.Success.Count; // *.CSV Sebanyak :: TargetKirim

                    CFtpResultInfo ftpResultInfo2 = await _dcFtpT.KirimAllCsv("WEBREKAP", reportLog: true);
                    List<CFtpResultSendGet> resAll2 = ftpResultInfo2.Success.Concat(ftpResultInfo2.Fail).ToList();
                    foreach (CFtpResultSendGet resAll in resAll2) {
                        string fileName = resAll.FileInformation.Name;
                        string columnDb = ftpFileKirim[fileName];
                        bool success = resAll.FtpStatusSendGet;
                        await _db.UpdateDcDcAmtaLog($"{columnDb} = {columnDb} || ' - ' || 'WEBREKAP {(success ? "Ok" : "Gagal")}'", dateStart);
                    }
                    BerhasilKirim += ftpResultInfo2.Success.Count; // *.CSV Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
