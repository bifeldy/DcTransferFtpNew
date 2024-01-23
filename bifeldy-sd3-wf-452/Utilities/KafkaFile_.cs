/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Buat Kirim File Ke Kafka
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using bifeldy_sd3_lib_452.Extensions;
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Utilities {

    public interface IKafkaFile {
        Task KirimFile(string hostPort, string topic, string folderName, string fileName);
    }

    public sealed class CKafkaFile : IKafkaFile {

        private readonly IStream _stream;
        private readonly IKafka _kafka;
        private readonly IConverter _converter;
        private readonly IChiper _chiper;

        public CKafkaFile(IStream stream, IKafka kafka, IConverter converter, IChiper chiper) {
            _stream = stream;
            _kafka = kafka;
            _converter = converter;
            _chiper = chiper;
        }

        public async Task KirimFile(string hostPort, string topic, string folderName, string fileName) {
            string filePath = Path.Combine(folderName, fileName);
            List<KafkaMessage<string, dynamic>> km = new List<KafkaMessage<string, dynamic>>();
            List<byte[]> lsb = _stream.ReadFileAsBinaryChunk(filePath);
            string sha1 = _chiper.CalculateSHA1(filePath);
            string crc32 = _chiper.CalculateCRC32(filePath);
            for (int i = 0; i <= lsb.Count; i++) {
                if (i == 0) {
                    decimal fullSize = 0;
                    lsb.ForEach(d => fullSize += d.Length);
                    KafkaFileFull kafkaFull = new KafkaFileFull {
                        file_name = fileName,
                        file_byte_size = fullSize,
                        sha1_full = sha1,
                        crc32_full = crc32,
                        total_chunk = lsb.Count
                    };
                    km.Add(new KafkaMessage<string, dynamic> {
                        Key = $"{crc32}_{i}",
                        Value = kafkaFull
                    });
                }
                else {
                    KafkaFilePart kafkaPart = new KafkaFilePart {
                        chunk_number = i,
                        chunk_byte_size = lsb[i - 1].Length,
                        hex_binary = lsb[i - 1].ToStringHex()
                    };
                    km.Add(new KafkaMessage<string, dynamic> {
                        Key = $"{crc32}_{i}",
                        Value = kafkaPart
                    });
                }
            }
            await _kafka.ProduceSingleMultipleMessages(hostPort, topic, km);
        }

    }

}
