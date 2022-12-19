/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Harian Data PLDC
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Navigations;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Logics {

    public interface IProsesHarianDataPldc : ILogics { }

    public sealed class CProsesHarianDataPldc : CLogics, IProsesHarianDataPldc {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesHarianDataPldc(
            IApp app,
            ILogger logger,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db) {
            _app = app;
            _logger = logger;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            CProsesHarian prosesHarian = (CProsesHarian)currentControl;
            Button button = (Button) sender;
            button.BackColor = Color.FromArgb(255, 207, 223);
            DateTime dateStart = prosesHarian.DateTimePickerHarianAwal.Value.Date;
            DateTime dateEnd = prosesHarian.DateTimePickerHarianAkhir.Value.Date;
            await Task.Run(async () => {
                if (IsDateRangeValid(dateStart, dateEnd) && IsDateRangeSameMonth(dateStart, dateEnd)) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    TargetKirim = 0;
                    BerhasilKirim = 0;

                    string fileTimeBRDFormat2Hariana = $"{dateStart:MM}";
                    string DBFformat = $"{dateStart:MM}";
                    string targetFileName = null;

                    string varDcExt = await _db.GetDcExt();

                    int jumlahHari = (int)((dateEnd - dateStart).TotalDays + 1);
                    _logger.WriteInfo(GetType().Name, $"{dateStart:MM/dd/yyyy} - {dateEnd:MM/dd/yyyy} ({jumlahHari} Hari)");

                    for (int i = 0; i < jumlahHari; i++) {
                        DateTime xDate = dateStart.AddDays(i);

                        string procName = "BUAT_PL_DC_EVO";
                        CDbExecProcResult res = await _db.CALL__P_TGL(procName, xDate);
                        if (res == null || !res.STATUS) {
                            throw new Exception($"Gagal Menjalankan Procedure {procName}");
                        }

                        targetFileName = $"DC{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        (bool success1, bool addQueue1) = await _qTrfCsv.CreateCSVFile(targetFileName, "DC");
                        if (success1 && addQueue1) {
                            TargetKirim++;
                        }

                        targetFileName = $"ST{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        (bool success2, bool addQueue2) = await _qTrfCsv.CreateCSVFile(targetFileName, "ST");
                        if (success2 && addQueue2) {
                            TargetKirim++;
                        }

                        targetFileName = $"SX{fileTimeBRDFormat2Hariana}{xDate:dd}G.{varDcExt}";
                        (bool success3, bool addQueue3) = await _qTrfCsv.CreateCSVFile(targetFileName, "SX");
                        if (success3 && addQueue3) {
                            TargetKirim++;
                        }
                    }

                    string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "DC");
                    int totalFileInZip = _berkas.ZipListFileInTempFolder(zipFileName);

                    BerhasilKirim += await _dcFtpT.KirimFtpLocal(); // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += await _dcFtpT.KirimFtpDev("PLDC", zipFileName, true); // *.ZIP Sebanyak :: 1
                    BerhasilKirim += await _dcFtpT.KirimFtpEis(zipFileName); // *.ZIP Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman(button.Text);
        }

    }

}
