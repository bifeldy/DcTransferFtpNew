/**
* 
* Author       :: Basilius Bias Astho Christyono
* Mail         :: bias@indomaret.co.id
* Phone        :: (+62) 889 236 6466
* 
* Department   :: IT SD 03
* Mail         :: bias@indomaret.co.id
* 
* Catatan      :: Model DataDSI_WS
*              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
* 
*/

namespace DcTransferFtpNew.Models {

    public sealed class DataDSI_WS {
        public string KD_DC { get; set; }
        public decimal KD_PLU { get; set; }
        public decimal SALDO_AWAL { get; set; }
        public decimal SALDO_AKHIR { get; set; }
        public decimal HPP_TOKO { get; set; }
        public decimal STOCK_MAX { get; set; }
        public decimal TOKO_AKTIF { get; set; }
        public decimal THNBLN { get; set; }
    }

}
