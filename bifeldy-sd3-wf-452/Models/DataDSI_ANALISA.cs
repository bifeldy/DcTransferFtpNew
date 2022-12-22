/**
* 
* Author       :: Basilius Bias Astho Christyono
* Mail         :: bias@indomaret.co.id
* Phone        :: (+62) 889 236 6466
* 
* Department   :: IT SD 03
* Mail         :: bias@indomaret.co.id
* 
* Catatan      :: Model DataDSI_ANALISA
*              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
* 
*/

using System;


namespace DcTransferFtpNew.Models {

    public sealed class DataDSI_ANALISA {
        public string KDDC { get; set; }
        public int PERIODE { get; set; }
        public int JML_HARI { get; set; }
        public int PLUID { get; set; }
        public string DIV { get; set; }
        public string DEP { get; set; }
        public string KAT { get; set; }
        public string TAG { get; set; }
        public string KD_SUPPLIER { get; set; }
        public string NM_SUPPLIER { get; set; }
        public int QTY_SLD_AWL { get; set; }
        public int RP_SLD_AWL { get; set; }
        public int QTY_SLD_AKHR { get; set; }
        public int RP_SLD_AKHR { get; set; }
        public int QTY_NPB { get; set; }
        public int RP_NPB { get; set; }
        public int DSI { get; set; }
        public DateTime UPDREC_DATE { get; set; }
        public int HPP_TOKO { get; set; }
    }

}
