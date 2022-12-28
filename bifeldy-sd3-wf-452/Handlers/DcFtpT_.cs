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

using FluentFTP;

using LogFileHO;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Utilities;
using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Handlers {

    public interface IDcFtpT {
        Task<int> KirimSingleCsv(string pga_type, string fileName, string folderPath = null);
        Task<int> KirimSingleZip(string pga_type, string fileName, string folderPath = null);
        Task<int> KirimAllCsv(string pga_type, string folderPath = null);
        Task<int> KirimAllZip(string pga_type, string folderPath = null);
        Task<int> KirimSelectedCsv(string pga_type, List<string> listCsvFileName, string folderPath = null);
        Task<int> KirimSelectedZip(string pga_type, List<string> listZipFileName, string folderPath = null);
        Task<int> KirimFtpDev(string procName, string zipFileName = null, bool reportLogHo = false, string folderPath = null);
        Task<int> KirimFtpWithLog(string pgaType, string zipFileName = null, string folderPath = null);
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
                if (dbDataReader != null) {
                    await dbDataReader.ReadAsync();
                    ftpObject = new DC_FTP_T {
                        PGA_IPADDRESS = dbDataReader["PGA_IPADDRESS"].ToString(),
                        PGA_PORTNUMBER = dbDataReader["PGA_PORTNUMBER"].ToString(),
                        PGA_USERNAME = dbDataReader["PGA_USERNAME"].ToString(),
                        PGA_PASSWORD = dbDataReader["PGA_PASSWORD"].ToString(),
                        PGA_FOLDER = dbDataReader["PGA_FOLDER"].ToString()
                    };
                }
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

        private async Task<int> KirimSingleFileInFolder(string pga_type, string fileName, string folderPath) {
            DC_FTP_T ftpInfo = await GetFtpInfo(pga_type);
            List<CFtpResultSendGet> ftpResultSent = await _ftp.CreateFtpConnectionAndSendFtpFiles(
                ftpInfo.PGA_IPADDRESS,
                int.Parse(ftpInfo.PGA_PORTNUMBER),
                ftpInfo.PGA_USERNAME,
                ftpInfo.PGA_PASSWORD,
                ftpInfo.PGA_FOLDER,
                folderPath,
                fileName
            );
            return ftpResultSent.Where(r => r.FtpStatusSendGet == FtpStatus.Success).ToArray().Length;
        }

        private async Task<int> KirimAllFilesInFolder(string pga_type, string folderPath) {
            return await KirimSingleFileInFolder(pga_type, null, folderPath);
        }

        /* ** */

        public async Task<int> KirimSingleCsv(string pga_type, string csvFileName, string folderPath = null) {
            return await KirimSingleFileInFolder(pga_type, csvFileName, folderPath ?? _berkas.TempFolderPath);
        }

        public async Task<int> KirimSingleZip(string pga_type, string zipFileName, string folderPath = null) {
            return await KirimSingleFileInFolder(pga_type, zipFileName, folderPath ?? _berkas.ZipFolderPath);
        }

        public async Task<int> KirimAllCsv(string pga_type, string folderPath = null) {
            return await KirimAllFilesInFolder(pga_type, folderPath ?? _berkas.TempFolderPath);
        }

        public async Task<int> KirimAllZip(string pga_type, string folderPath = null) {
            return await KirimAllFilesInFolder(pga_type, folderPath ?? _berkas.ZipFolderPath);
        }

        public async Task<int> KirimSelectedCsv(string pga_type, List<string> listCsvFileName, string folderPath = null) {
            int terkirim = 0;
            foreach (string fn in listCsvFileName) {
                terkirim += await KirimSingleFileInFolder(pga_type, fn, folderPath ?? _berkas.TempFolderPath);
            }
            return terkirim;
        }

        public async Task<int> KirimSelectedZip(string pga_type, List<string> listZipFileName, string folderPath = null) {
            int terkirim = 0;
            foreach (string fn in listZipFileName) {
                terkirim += await KirimSingleFileInFolder(pga_type, fn, folderPath ?? _berkas.ZipFolderPath);
            }
            return terkirim;
        }

        /// <summary>
        /// Jika `zipFileName` Tidak NULL, Maka Hanya Akan Kirim 1 Berkas .ZIP Saja
        /// Jika `zipFileName` NULL, Maka Akan Kirim Semua Berkas .CSV
        /// </summary>

        public async Task<int> KirimFtpDev(string processName, string zipFileName = null, bool reportLogHo = false, string folderPath = null) {
            if (folderPath == null) {
                folderPath = _berkas.TempFolderPath;
            }
            int fileSent = 0;
            DC_FTP_T ftpInfo = await GetFtpInfo("DEV");
            string dirPath = zipFileName == null ? folderPath : _berkas.ZipFolderPath;
            string remotePath = $"/u01/ftp/DC/{ftpInfo.PGA_FOLDER}/{await _db.GetKodeDc()}";
            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            if (zipFileName != null) {
                fileInfos = fileInfos.Where(f => f.Name.Contains(zipFileName)).ToArray();
            }
            List<FTP_FILE_LOG_CUSTOM> logs = new List<FTP_FILE_LOG_CUSTOM>();
            foreach (FileInfo fi in fileInfos) {
                bool statusTransfer = _sftp.PutFile(
                    ftpInfo.PGA_IPADDRESS,
                    22,
                    ftpInfo.PGA_USERNAME,
                    ftpInfo.PGA_PASSWORD,
                    fi.FullName,
                    remotePath
                );
                if (statusTransfer) {
                    fileSent++;
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
                // Transfer log ke HO (Sulis, v1054, 22/08/2019)
                string urlWebServiceHO = await _db.GetURLWebService("HO") ?? _app.GetConfig("ws_ho");
                SenderLog senderLog = new SenderLog(urlWebServiceHO);
                senderLog.CatatTransferLogToHO(logs);
            }
            return fileSent;
        }

        public async Task<int> KirimFtpWithLog(string pgaType, string zipFileName = null, string folderPath = null) {
            if (folderPath == null) {
                folderPath = _berkas.TempFolderPath;
            }
            DC_FTP_T ftpInfo = await GetFtpInfo(pgaType);
            string dirPath = zipFileName == null ? folderPath : _berkas.ZipFolderPath;
            FtpClient ftpClient = await _ftp.CreateFtpConnection(
                ftpInfo.PGA_IPADDRESS,
                int.Parse(ftpInfo.PGA_PORTNUMBER),
                ftpInfo.PGA_USERNAME,
                ftpInfo.PGA_PASSWORD,
                ftpInfo.PGA_FOLDER
            );
            List<CFtpResultSendGet> ftpResultSent = await _ftp.SendFtpFiles(ftpClient, dirPath, zipFileName);
            LogTrf.LogTrf logTrf = new LogTrf.LogTrf();
            foreach (CFtpResultSendGet result in ftpResultSent) {
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
            return ftpResultSent.Where(r => r.FtpStatusSendGet == FtpStatus.Success).ToArray().Length;
        }

    }

}
