/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Tabel DC_FTP_T
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using FluentFTP;

using LogFileHO;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Utilities;
using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Handlers {

    public interface IDcFtpT {
        Task<CFtpResultInfo> KirimSingleCsv(string pga_type, string fileName, string folderPath = null, bool reportLog = false);
        Task<CFtpResultInfo> KirimSingleZip(string pga_type, string fileName, string folderPath = null, bool reportLog = false);
        Task<CFtpResultInfo> KirimAllCsv(string pga_type, string folderPath = null, bool reportLog = false);
        Task<CFtpResultInfo> KirimAllZip(string pga_type, string folderPath = null, bool reportLog = false);
        Task<CFtpResultInfo> KirimSelectedCsv(string pga_type, List<string> listCsvFileName, string folderPath = null, bool reportLog = false);
        Task<CFtpResultInfo> KirimSelectedZip(string pga_type, List<string> listZipFileName, string folderPath = null, bool reportLog = false);
        Task<CFtpResultInfo> KirimAllCsvAtauZipFtpDev(string procName, string zipFileName = null, bool reportLogHo = false, string folderPath = null);
    }

    public sealed class CDcFtpT : IDcFtpT {

        private readonly IApp _app;
        private readonly IDb _db;
        private readonly ILogger _logger;
        private readonly IFtp _ftp;
        private readonly IBerkas _berkas;
        private readonly ISftp _sftp;

        public CDcFtpT(IApp app, IDb db, ILogger logger, IFtp ftp, IBerkas berkas, ISftp sftp) {
            _app = app;
            _db = db;
            _logger = logger;
            _ftp = ftp;
            _berkas = berkas;
            _sftp = sftp;
        }

        private async Task<FTP_FILE_LOG_CUSTOM> CreateFileLogToHO(string namaFile, string jenisService, DC_FTP_T dc_ftp_t, string remotePath, long fileSize, string status) {
            return new FTP_FILE_LOG_CUSTOM {
                FTP_DC_KODE = await _db.GetKodeDc(),
                FTP_FILENAME = namaFile,
                FTP_TYPEFILE = jenisService,
                FTP_IP = dc_ftp_t.PGA_IPADDRESS,
                FTP_PORT = "21",
                FTP_USER = dc_ftp_t.PGA_USERNAME,
                FTP_PASS = dc_ftp_t.PGA_PASSWORD,
                FTP_LOCATION = remotePath,
                FTP_TIMELOG_TRF = DateTime.Now,
                FTP_FILESIZE_TRF = fileSize,
                FTP_NAMAPROG = _app.AppName,
                FTP_TYPEPROG = "MANUAL",
                FTP_STATUS = status
            };
        }

        private async Task<DC_FTP_T> GetFtpInfo(string pga_type) {
            DC_FTP_T ftpObject = null;
            DbDataReader dbDataReader = null;
            Exception exception = null;
            try {
                dbDataReader = await _db.GetFtpInfo(pga_type);
                if (dbDataReader == null) {
                    throw new Exception($"Alamat FTP {pga_type} Belum Terdaftar");
                }
                await dbDataReader.ReadAsync();
                ftpObject = new DC_FTP_T {
                    PGA_IPADDRESS = dbDataReader["PGA_IPADDRESS"].ToString(),
                    PGA_PORTNUMBER = dbDataReader["PGA_PORTNUMBER"].ToString(),
                    PGA_USERNAME = dbDataReader["PGA_USERNAME"].ToString(),
                    PGA_PASSWORD = dbDataReader["PGA_PASSWORD"].ToString(),
                    PGA_FOLDER = dbDataReader["PGA_FOLDER"].ToString()
                };
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                exception = ex;
            }
            finally {
                if (dbDataReader != null) {
                    dbDataReader.Close();
                }
                _db.CloseAllConnection();
            }
            if (exception != null) {
                throw exception;
            }
            return ftpObject;
        }

        private async Task<CFtpResultInfo> KirimSingleFileInFolder(string pga_type, string fileName, string folderPath, bool reportLog = false) {
            DC_FTP_T ftpInfo = await GetFtpInfo(pga_type);
            CFtpResultInfo ftpResultInfo = await _ftp.CreateFtpConnectionAndSendFtpFiles(
                ftpInfo.PGA_IPADDRESS,
                int.Parse(ftpInfo.PGA_PORTNUMBER),
                ftpInfo.PGA_USERNAME,
                ftpInfo.PGA_PASSWORD,
                ftpInfo.PGA_FOLDER,
                folderPath,
                fileName
            );
            if (reportLog) {
                LogTrf.LogTrf logTrf = new LogTrf.LogTrf();
                foreach (CFtpResultSendGet result in ftpResultInfo.Success) {
                    try {
                        logTrf.CatatStartTransfer(
                            result.FileInformation.Name,
                            _app.AppName,
                            ftpInfo.PGA_IPADDRESS,
                            ftpInfo.PGA_FOLDER,
                            result.FileInformation.Length
                        );
                    }
                    catch (Exception ex) {
                        _logger.WriteError(ex);
                    }
                }
            }
            return ftpResultInfo;
        }

        private async Task<CFtpResultInfo> KirimAllFilesInFolder(string pga_type, string folderPath, bool reportLog = false) {
            return await KirimSingleFileInFolder(pga_type, null, folderPath, reportLog);
        }

        private void PreviewResult(string pga_type, CFtpResultInfo ftpResultInfo) {
            string hasilKirim = ".: Berhasil Kirim :." + Environment.NewLine + Environment.NewLine;
            foreach (CFtpResultSendGet result in ftpResultInfo.Success) {
                hasilKirim += $"[+] {result.FileInformation.Name}" + Environment.NewLine;
            }
            hasilKirim += Environment.NewLine + ".: Gagal Kirim :." + Environment.NewLine + Environment.NewLine;
            foreach (CFtpResultSendGet result in ftpResultInfo.Fail) {
                hasilKirim += $"[-] {result.FileInformation.Name}" + Environment.NewLine;
            }
            if (ftpResultInfo.Success.Count + ftpResultInfo.Fail.Count > 0) {
                MessageBox.Show(hasilKirim, $"Pengiriman Ke :: {pga_type}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /* ** */

        public async Task<CFtpResultInfo> KirimSingleCsv(string pga_type, string csvFileName, string folderPath = null, bool reportLog = false) {
            CFtpResultInfo ftpResultInfo = await KirimSingleFileInFolder(pga_type, csvFileName, folderPath ?? _berkas.TempFolderPath, reportLog);
            PreviewResult(pga_type, ftpResultInfo);
            return ftpResultInfo;
        }

        public async Task<CFtpResultInfo> KirimSingleZip(string pga_type, string zipFileName, string folderPath = null, bool reportLog = false) {
            CFtpResultInfo ftpResultInfo = await KirimSingleFileInFolder(pga_type, zipFileName, folderPath ?? _berkas.ZipFolderPath, reportLog);
            PreviewResult(pga_type, ftpResultInfo);
            return ftpResultInfo;
        }

        public async Task<CFtpResultInfo> KirimAllCsv(string pga_type, string folderPath = null, bool reportLog = false) {
            CFtpResultInfo ftpResultInfo = await KirimAllFilesInFolder(pga_type, folderPath ?? _berkas.TempFolderPath, reportLog);
            PreviewResult(pga_type, ftpResultInfo);
            return ftpResultInfo;
        }

        public async Task<CFtpResultInfo> KirimAllZip(string pga_type, string folderPath = null, bool reportLog = false) {
            CFtpResultInfo ftpResultInfo = await KirimAllFilesInFolder(pga_type, folderPath ?? _berkas.ZipFolderPath, reportLog);
            PreviewResult(pga_type, ftpResultInfo);
            return ftpResultInfo;
        }

        public async Task<CFtpResultInfo> KirimSelectedCsv(string pga_type, List<string> listCsvFileName, string folderPath = null, bool reportLog = false) {
            CFtpResultInfo ftpResultInfo = new CFtpResultInfo();
            foreach (string fn in listCsvFileName) {
                CFtpResultInfo res = await KirimSingleFileInFolder(pga_type, fn, folderPath ?? _berkas.TempFolderPath, reportLog);
                foreach (CFtpResultSendGet r in res.Success) {
                    ftpResultInfo.Success.Add(r);
                }
                foreach (CFtpResultSendGet r in res.Fail) {
                    ftpResultInfo.Fail.Add(r);
                }
            }
            PreviewResult(pga_type, ftpResultInfo);
            return ftpResultInfo;
        }

        public async Task<CFtpResultInfo> KirimSelectedZip(string pga_type, List<string> listZipFileName, string folderPath = null, bool reportLog = false) {
            CFtpResultInfo ftpResultInfo = new CFtpResultInfo();
            foreach (string fn in listZipFileName) {
                CFtpResultInfo res = await KirimSingleFileInFolder(pga_type, fn, folderPath ?? _berkas.ZipFolderPath, reportLog);
                foreach (CFtpResultSendGet r in res.Success) {
                    ftpResultInfo.Success.Add(r);
                }
                foreach (CFtpResultSendGet r in res.Fail) {
                    ftpResultInfo.Fail.Add(r);
                }
            }
            PreviewResult(pga_type, ftpResultInfo);
            return ftpResultInfo;
        }

        /// <summary>
        /// Jika `zipFileName` Tidak NULL, Maka Hanya Akan Kirim 1 Berkas .ZIP Saja
        /// Jika `zipFileName` NULL, Maka Akan Kirim Semua Berkas .CSV
        /// </summary>

        public async Task<CFtpResultInfo> KirimAllCsvAtauZipFtpDev(string processName, string zipFileName = null, bool reportLogHo = false, string folderPath = null) {
            if (folderPath == null) {
                folderPath = _berkas.TempFolderPath;
            }
            CFtpResultInfo ftpResultInfo = new CFtpResultInfo();
            List<FTP_FILE_LOG_CUSTOM> logs = new List<FTP_FILE_LOG_CUSTOM>();
            DC_FTP_T ftpInfo = await GetFtpInfo("DEV");
            string dirPath = zipFileName == null ? folderPath : _berkas.ZipFolderPath;
            string remotePath = $"/u01/ftp/DC/{ftpInfo.PGA_FOLDER}/{await _db.GetKodeDc()}";
            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            if (zipFileName != null) {
                fileInfos = fileInfos.Where(f => f.Name.Contains(zipFileName)).ToArray();
            }
            foreach (FileInfo fi in fileInfos) {
                bool statusTransfer = _sftp.PutFile(
                    ftpInfo.PGA_IPADDRESS,
                    22,
                    ftpInfo.PGA_USERNAME,
                    ftpInfo.PGA_PASSWORD,
                    fi.FullName,
                    remotePath
                );
                CFtpResultSendGet resultGet = new CFtpResultSendGet() {
                    FtpStatusSendGet = statusTransfer ? FtpStatus.Success : FtpStatus.Failed,
                    FileInformation = fi
                };
                if (statusTransfer) {
                    ftpResultInfo.Success.Add(resultGet);
                }
                else {
                    ftpResultInfo.Fail.Add(resultGet);
                }
                logs.Add(
                    await CreateFileLogToHO(
                        fi.Name,
                        processName,
                        ftpInfo,
                        remotePath,
                        fi.Length,
                        statusTransfer ? "Berhasil" : "Gagal"
                    )
                );
            }
            if (reportLogHo && logs.Count > 0) {
                string HO = "HO";
                try {
                    // Transfer log ke HO (Sulis, v1054, 22/08/2019)
                    string urlWebServiceHO = await _db.GetURLWebService(HO); // ?? _app.GetConfig("ws_ho");
                    SenderLog senderLog = new SenderLog(urlWebServiceHO);
                    senderLog.CatatTransferLogToHO(logs);
                }
                catch (Exception ex) {
                    _logger.WriteError(ex);
                    MessageBox.Show(ex.Message, $"Web Service {HO} Tidak Tersedia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            PreviewResult("DEV", ftpResultInfo);
            return ftpResultInfo;
        }

    }

}
