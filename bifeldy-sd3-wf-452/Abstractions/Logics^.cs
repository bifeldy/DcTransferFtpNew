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
            dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date;
            dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date;
        }

        protected bool IsDateRangeValid(DateTime dateStart, DateTime dateEnd) {
            return dateStart <= dateEnd ? true : throw new Exception($"Tanggal Mulai Harus Lebih Kecil Dari Tanggal Akhir");
        }

        protected bool IsDateRangeSameMonth(DateTime dateStart, DateTime dateEnd) {
            return dateStart.Month == dateEnd.Month ? true : throw new Exception($"Hanya Bisa Di (1) Bulan yang Sama");
        }

        protected bool IsDateStartEndSame(DateTime dateStart, DateTime dateEnd) {
            return dateStart == dateEnd ? true : throw new Exception($"Hanya Bisa Di (1) Hari yang Sama");
        }

        protected async Task<bool> IsDateEndYesterday(DateTime dateEnd, int lastDay = 1) {
            DateTime currentDay = await _db.GetYesterdayDate(lastDay);
            return dateEnd <= currentDay ? true : throw new Exception($"Max Tanggal Akhir Adalah Hari Ini - {lastDay} Hari!");
        }

        protected void CheckHasilKiriman() {
            if (string.IsNullOrEmpty(InfoMessage)) {
                if (BerhasilKirim == 0 || TargetKirim == 0) {
                    InfoMessage = $"Ada Masalah, Belum Ada {button.Text} Yang Diproses !!";
                }
                else if (BerhasilKirim < TargetKirim && TargetKirim > 0) {
                    InfoMessage = $"Ada Beberapa Proses {button.Text} Yang Gagal !!";
                }
                else if (BerhasilKirim >= TargetKirim && TargetKirim > 0) {
                    InfoMessage = $"{button.Text} Sukses !!";
                }
                else {
                    InfoMessage = $"{button.Text} Error !!";
                }
            }
            MessageBox.Show(InfoMessage, button.Text);
        }

    }

}
