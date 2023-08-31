/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Model Tabel DC_FTP_T
 *              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
 * 
 */

namespace DcTransferFtpNew.Models {

    public sealed class DC_FTP_T {
        public string PGA_TYPE { get; set; }
        public string PGA_IPADDRESS { get; set; }
        public string PGA_PORTNUMBER { get; set; }
        public string PGA_USERNAME { get; set; }
        public string PGA_PASSWORD { get; set; }
        public string PGA_FOLDER { get; set; }
        public string PGA_GD_CODE { get; set; }
    }

}
