/**
* 
* Author       :: Basilius Bias Astho Christyono
* Mail         :: bias@indomaret.co.id
* Phone        :: (+62) 889 236 6466
* 
* Department   :: IT SD 03
* Mail         :: bias@indomaret.co.id
* 
* Catatan      :: Model Kafka Message File Yang Mau Di Kirim JSON
*              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
* 
*/

namespace DcTransferFtpNew.Models {

    public sealed class KafkaFilePart {
        public decimal chunk_number { get; set; }
        public decimal chunk_byte_size { get; set; }
        public string hex_binary { get; set; }
    }

    public sealed class KafkaFileFull {
        public string file_name { get; set; }
        public decimal file_byte_size { get; set; }
        public string sha1_full { get; set; }
        public string crc32_full { get; set; }
        public decimal total_chunk { get; set; }
        public KafkaFilePart[] part_list { get; set; }
    }

}
