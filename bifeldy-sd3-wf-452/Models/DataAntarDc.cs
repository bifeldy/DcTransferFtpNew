/**
* 
* Author       :: Basilius Bias Astho Christyono
* Mail         :: bias@indomaret.co.id
* Phone        :: (+62) 889 236 6466
* 
* Department   :: IT SD 03
* Mail         :: bias@indomaret.co.id
* 
* Catatan      :: Model DataAntarDC
*              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
* 
*/

using System;

namespace DcTransferFtpNew.Models {

    public sealed class DataAntarDC {
        public string TBL_DC_KODE { get; set; }
        public string TBL_DC_NAMA { get; set; }
        public string TBL_TAG_ERROR_BELI { get; set; }
        public string TBL_NPWP_DC { get; set; }
        public string TBL_CABANG_KODE { get; set; }
        public string TBL_CABANG_NAMA { get; set; }
        public int TBL_DCID { get; set; }
        public string TBL_GUDANG_KODE { get; set; }
        public string TBL_GUDANG_NAMA { get; set; }
        public int TBL_GUDANGID { get; set; }
        public string TBL_GUDANG_TYPE { get; set; }
        public string TBL_LOKASI_KODE { get; set; }
        public string TBL_LOKASI_NAMA { get; set; }
        public int TBL_LOKASIID { get; set; }
        public string TBL_LOKASI_TYPE { get; set; }
        public DateTime TBL_UPDREC_DATE { get; set; }
    }

}
