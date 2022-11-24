/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data IRPC
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Navigations;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianDataIrpc : ILogics { }

    public sealed class CProsesHarianDataIrpc : CLogics, IProsesHarianDataIrpc {

        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianDataIrpc(
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IDcFtpT dc_ftp_t
        ) : base(db) {
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            CProsesHarian prosesHarian = (CProsesHarian)currentControl;
            _logger.ClearLog();
            Button button = (Button)sender;
            button.BackColor = Color.FromArgb(255, 207, 223);
            DateTime dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date;
            DateTime dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date;
            await Task.Run(async () => {
                string infoMessage = null;
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd) && await IsDateEndYesterday(dateEnd)) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteLog(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        CDbExecProcResult res = await _db.CALL__P_TGL("INS_BPBNRB_SUPROTI", xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception("Gagal Menjalankan Procedure");
                        }

                        DataTable dtQuery = await _db.GetIrpc(xDate);
                        string targetFileName = $"IRPC{await _db.GetKodeDc()}{xDate:ddMMyyyyHHmm}.CSV";
                        string seperator = ",";
                        if (_berkas.DataTable2CSV(dtQuery, targetFileName, seperator)) {
                            // _berkas.ListFileForZip.Add(targetFileName);
                            TargetKirim++;
                        }
                    }

                    // string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "IRPC");
                    // int totalFileInZip = _berkas.ZipListFileInTempFolder(zipFileName);

                    BerhasilKirim += await _dcFtpT.KirimFtpIrpc(); // *.CSV Sebanyak :: TargetKirim

                    _berkas.CleanUp();
                }
                if (string.IsNullOrEmpty(infoMessage)) {
                    if (BerhasilKirim == 0 || TargetKirim == 0) {
                        infoMessage = $"Ada Masalah, Belum Ada {button.Text} Yang Diproses !!";
                    }
                    else if (BerhasilKirim < TargetKirim && TargetKirim > 0) {
                        infoMessage = $"Ada Beberapa Proses {button.Text} Yang Gagal !!";
                    }
                    else if (BerhasilKirim == TargetKirim && TargetKirim > 0) {
                        infoMessage = $"{button.Text} Sukses !!";
                    }
                    else {
                        infoMessage = $"{button.Text} Error !!";
                    }
                }
                MessageBox.Show(infoMessage, button.Text);
            });
        }

    }

}
