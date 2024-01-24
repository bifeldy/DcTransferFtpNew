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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using bifeldy_sd3_lib_452.Extensions;
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Utilities {

    public interface IKafkaFile {
        Task<(string, string)> GetHostIpPortAndTopic(string type);
        Task KirimFile(string hostPort, string topic, string folderName, string fileName, DateTime fileDate, string fileKeterangan = null);
    }

    public sealed class CKafkaFile : IKafkaFile {

        private readonly IApp _app;
        private readonly IStream _stream;
        private readonly IKafka _kafka;
        private readonly IChiper _chiper;
        private readonly IDb _db;

        public CKafkaFile(IApp app, IStream stream, IKafka kafka, IChiper chiper, IDb db) {
            _app = app;
            _stream = stream;
            _kafka = kafka;
            _chiper = chiper;
            _db = db;
        }

        public async Task<(string, string)> GetHostIpPortAndTopic(string type) {
            string hostPort = "172.31.2.122:9092";
            // Perlukah Pakai Baca Ke Tabel (?)
            return (hostPort, $"{(_app.DebugMode ? "TEST_" : "")}TRANSFER_{type}_{await _db.GetKodeDc()}");
        }

        public async Task KirimFile(string hostPort, string topic, string folderName, string fileName, DateTime fileDate, string fileKeterangan = null) {
            string filePath = Path.Combine(folderName, fileName);
            List<KafkaMessage<string, dynamic>> km = new List<KafkaMessage<string, dynamic>>();
            List<byte[]> lsb = _stream.ReadFileAsBinaryChunk(filePath, (int) Math.Pow(2, 18)); // 262144 KB = 256 KiB
            string sha1 = _chiper.CalculateSHA1(filePath);
            string crc32 = _chiper.CalculateCRC32(filePath);
            for (int i = 0; i <= lsb.Count; i++) {
                if (i == 0) {
                    decimal fullSize = 0;
                    lsb.ForEach(d => fullSize += d.Length);
                    KafkaFileFull kafkaFull = new KafkaFileFull {
                        file_name = fileName,
                        file_byte_size = fullSize,
                        file_date = fileDate,
                        file_keterangan = fileKeterangan,
                        sha1_full = sha1,
                        crc32_full = crc32,
                        total_chunk = lsb.Count,
                        program_name = _app.AppName,
                        program_type = "MANUAL"
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
