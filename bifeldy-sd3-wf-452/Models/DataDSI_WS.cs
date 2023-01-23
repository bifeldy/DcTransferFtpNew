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
        public dynamic KD_PLU { get; set; }
        public dynamic SALDO_AWAL { get; set; }
        public dynamic SALDO_AKHIR { get; set; }
        public dynamic HPP_TOKO { get; set; }
        public dynamic STOCK_MAX { get; set; }
        public dynamic TOKO_AKTIF { get; set; }
        public dynamic THNBLN { get; set; }
    }

}
