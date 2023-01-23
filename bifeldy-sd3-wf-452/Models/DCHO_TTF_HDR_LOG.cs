/**
* 
* Author       :: Basilius Bias Astho Christyono
* Mail         :: bias@indomaret.co.id
* Phone        :: (+62) 889 236 6466
* 
* Department   :: IT SD 03
* Mail         :: bias@indomaret.co.id
* 
* Catatan      :: Model Tabel DCHO_TTF_HDR_LOG
*              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
* 
*/

using System;

namespace DcTransferFtpNew.Models {

    public sealed class DCHO_TTF_HDR_LOG {
        public string TBL_DC_KODE { get; set; }
        public DateTime TGL_DOC { get; set; }
        public DateTime TGL_PROSES { get; set; }
        public DateTime START_TAX { get; set; }
        public DateTime STOP_TAX { get; set; }
        public string STATUS_TAX { get; set; }
        public dynamic JML_BPB_TAX { get; set; }
        public dynamic JML_NRB_TAX { get; set; }
        public DateTime START_BPB { get; set; }
        public DateTime STOP_BPB { get; set; }
        public string STATUS_BPB { get; set; }
        public dynamic JML_BPB_OK { get; set; }
        public dynamic JML_BPB_FAIL { get; set; }
        public DateTime START_NRB { get; set; }
        public DateTime STOP_NRB { get; set; }
        public string STATUS_NRB { get; set; }
        public dynamic JML_NRB_OK { get; set; }
        public dynamic JML_NRB_FAIL { get; set; }
        public string FILE_ZIP { get; set; }
        public string STATUS_TRF { get; set; }
        public DateTime TGL_TRF { get; set; }
        public string FILE_TAX { get; set; }
    }

}
