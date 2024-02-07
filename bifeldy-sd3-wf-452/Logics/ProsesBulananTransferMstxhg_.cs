/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Bulanan Mstxhg
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew.Logics {

    public interface IProsesBulananTransferMstxhg : ILogics { }

    public sealed class CProsesBulananTransferMstxhg : CLogics, IProsesBulananTransferMstxhg {

        private readonly IApp _app;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IZip _zip;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesBulananTransferMstxhg(
            IApp app,
            IDb db,
            IBerkas berkas,
            ICsv csv,
            IZip zip,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db, csv, zip) {
            _app = app;
            _db = db;
            _berkas = berkas;
            _csv = csv;
            _zip = zip;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareBulanan(sender, e, currentControl);
            await Task.Run(async () => {
                _berkas.DeleteOldFilesInFolder(_csv.CsvFolderPath, 0);
                JumlahServerKirimCsv = 1;
                JumlahServerKirimZip = 1;

                string fileTimeMSTXHGFormat = $"{dateStart:MM}";
                string fileTimeMSTXHGGFormat = $"{dateStart:yyMM}";
                string csvFileName = null;

                string procName = "TRF_MSTXHG_EVO";
                CDbExecProcResult res = await _db.CALL__P_TGL(procName, datePeriode);
                if (res == null || !res.STATUS) {
                    throw new Exception($"Gagal Menjalankan Procedure {procName}");
                }

                csvFileName = $"MSTXHG{fileTimeMSTXHGFormat}.CSV";
                await _qTrfCsv.CreateCSVFile("MSTXHG", csvFileName);
                TargetKirim += JumlahServerKirimCsv;

                string zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "MSTXHG");
                _zip.ZipListFileInFolder(zipFileName, _csv.CsvFolderPath);
                TargetKirim += JumlahServerKirimZip;

                BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauSingleZipKeFtpDev("MSTXHG", zipFileName, true)).Success.Count; // *.ZIP Sebanyak :: 1

                csvFileName = $"MSTXHGG{fileTimeMSTXHGGFormat}.CSV";
                await _qTrfCsv.CreateCSVFile("MSTXHGG", csvFileName);
                // TargetKirim += JumlahServerKirimCsv;

                zipFileName = await _db.Q_TRF_CSV__GET($"{(_app.IsUsingPostgres ? "COALESCE" : "NVL")}(q_namazip, q_namafile)", "MSTXHGG");
                _zip.ZipListFileInFolder(zipFileName, _csv.CsvFolderPath);
                TargetKirim += JumlahServerKirimZip;

                BerhasilKirim += (await _dcFtpT.KirimSingleZip("WRC", zipFileName)).Success.Count; // *.ZIP Sebanyak :: 1

                _berkas.CleanUp();
            });
            CheckHasilKiriman();
        }

    }

}
