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
using System.Threading.Tasks;
using System.Windows.Forms;

using DcTransferFtpNew.Handlers;

namespace DcTransferFtpNew.Abstractions {

    public interface ILogics {
        Task Run(object sender, EventArgs e, Control currentControl);
    }

    public abstract class CLogics : ILogics {

        private readonly IDb _db;

        protected int TargetKirim = 0;
        protected int BerhasilKirim = 0;

        public CLogics(IDb db) {
            _db = db;
        }

        public abstract Task Run(object sender, EventArgs e, Control currentControl);

        protected bool IsDateRangeValid(DateTime dateStart, DateTime dateEnd) {
            return dateStart <= dateEnd;
        }

        protected bool IsDateRangeSameMonth(DateTime dateStart, DateTime dateEnd) {
            return dateStart.Month == dateEnd.Month;
        }

        protected async Task<bool> IsDateEndYesterday(DateTime dateEnd, int lastDay = 1) {
            DateTime currentDay = await _db.GetYesterdayDate(lastDay);
            if (dateEnd <= currentDay) {
                return true;
            }
            throw new Exception($"Max Tanggal Akhir Adalah Hari Ini - {lastDay} Hari!");
        }

    }

}
