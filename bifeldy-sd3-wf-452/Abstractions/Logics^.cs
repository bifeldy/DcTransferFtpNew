/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Template Untuk Semua Logic
 *              :: Hanya Untuk Inherit
 *              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Navigations;

namespace DcTransferFtpNew.Abstractions {

    public interface ILogics {
        Task Run(object sender, EventArgs e, Control currentControl);
    }

    public abstract class CLogics : ILogics {

        private readonly IDb _db;

        protected int TargetKirim = 0;
        protected int BerhasilKirim = 0;

        protected int JumlahServerKirimCsv = 0;
        protected int JumlahServerKirimZip = 0;

        protected string InfoMessage = null;

        protected Button button;

        protected DateTime dateStart = DateTime.MinValue;
        protected DateTime dateEnd = DateTime.MinValue;
        protected DateTime datePeriode = DateTime.MinValue;

        public CLogics(IDb db) {
            _db = db;
        }

        public abstract Task Run(object sender, EventArgs e, Control currentControl);

        private void SetBtnSenderSelected(object sender) {
            button = (Button) sender;
            button.BackColor = Color.FromArgb(255, 207, 223);
        }

        protected void PrepareHarian(object sender, EventArgs e, Control currentControl) {
            SetBtnSenderSelected(sender);
            CProsesHarian prosesHarian = (CProsesHarian) currentControl;
            dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date; // Without Time
            dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date; // Without Time
        }

        protected void PrepareBulanan(object sender, EventArgs e, Control currentControl) {
            SetBtnSenderSelected(sender);
            CProsesBulanan prosesBulanan = (CProsesBulanan) currentControl;
            datePeriode = prosesBulanan.DateTimePeriodeBulanan.Value.Date; // Always Using Current Day, Without Time
        }

        protected bool IsDateRangeValid() {
            return dateStart <= dateEnd ? true : throw new Exception($"Tanggal Mulai Harus Lebih Kecil Dari Tanggal Akhir");
        }

        protected bool IsDateRangeSameDay() {
            return dateStart == dateEnd ? true : throw new Exception($"Hanya Bisa Di (1) Hari Yang Sama");
        }

        protected bool IsDateRangeSameMonth() {
            return dateStart.Month == dateEnd.Month ? true : throw new Exception($"Hanya Bisa Di (1) Bulan Yang Sama");
        }

        protected async Task<bool> IsDateRangeToday() {
            DateTime toDay = await _db.GetCurrentDate();
            return (dateStart == toDay && dateEnd == toDay) ? true : throw new Exception($"Hanya Bisa Proses Tanggal Hari Ini = {toDay:dd-MMM-yyyy}");
        }

        protected bool IsDateStartEndSame() {
            return dateStart == dateEnd ? true : throw new Exception($"Hanya Bisa Di (1) Hari Yang Sama");
        }

        protected async Task<bool> IsDateStartMaxYesterday(int lastDay = 1) {
            DateTime yesterDay = await _db.GetYesterdayDate(lastDay);
            return dateStart <= yesterDay ? true : throw new Exception($"Max Tanggal Awal Adalah Hari Ini - {lastDay} Hari <= {yesterDay:dd-MMM-yyyy}");
        }

        protected async Task<bool> IsDateEndMaxYesterday(int lastDay = 1) {
            DateTime yesterDay = await _db.GetYesterdayDate(lastDay);
            return dateEnd <= yesterDay ? true : throw new Exception($"Max Tanggal Akhir Adalah Hari Ini - {lastDay} Hari <= {yesterDay:dd-MMM-yyyy}");
        }

        protected void CheckHasilKiriman() {
            MessageBoxIcon msgBxIco = MessageBoxIcon.Error;
            if (string.IsNullOrEmpty(InfoMessage)) {
                if (JumlahServerKirimCsv == 0 && JumlahServerKirimZip == 0 && BerhasilKirim == 0 && TargetKirim == 0) {
                    InfoMessage = $"Selesai Proses {button.Text}, Namun Tidak Ada File Yang Dikirim !!";
                    msgBxIco = MessageBoxIcon.Information;
                }
                else if (BerhasilKirim == 0 || TargetKirim == 0) {
                    InfoMessage = $"Ada Masalah, Belum Ada {button.Text} Yang Diproses !!";
                    msgBxIco = MessageBoxIcon.Warning;
                }
                else if (BerhasilKirim < TargetKirim && TargetKirim > 0) {
                    InfoMessage = $"Ada Beberapa Proses {button.Text} Yang Gagal !!";
                }
                else if (BerhasilKirim == TargetKirim && TargetKirim > 0) {
                    InfoMessage = $"{button.Text} Sukses !!";
                    msgBxIco = MessageBoxIcon.Information;
                }
                else {
                    InfoMessage = $"{button.Text} Error !!";
                }
            }
            MessageBox.Show(InfoMessage, button.Text, MessageBoxButtons.OK, msgBxIco);
        }

    }

}
