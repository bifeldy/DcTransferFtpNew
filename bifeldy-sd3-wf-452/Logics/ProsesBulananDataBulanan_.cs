/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Proses Bulanan Data Bulanan
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.DSI_WS;
using DcTransferFtpNew.GetAnalisaDSIHO;
using DcTransferFtpNew.GetDataAntarDC;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Utilities;
using DcTransferFtpNew.Models;

namespace DcTransferFtpNew.Logics {

    public interface IProsesBulananDataBulanan : ILogics { }

    public sealed class CProsesBulananDataBulanan : CLogics, IProsesBulananDataBulanan {

        private readonly IConfig _config;
        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IConverter _converter;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesBulananDataBulanan(
            IConfig config,
            IApp app,
            ILogger logger,
            IConverter converter,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db, berkas) {
            _config = config;
            _app = app;
            _logger = logger;
            _converter = converter;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override async Task Run(object sender, EventArgs e, Control currentControl) {
            PrepareBulanan(sender, e, currentControl);
            await Task.Run(async () => {
                if (await IsPeriodeLastMonth()) {
                    _berkas.DeleteOldFilesInFolder(_berkas.TempFolderPath, 0);
                    JumlahServerKirimCsv = 2;

                    List<string> lsCsvToZip = new List<string>();

                    string csvFileName = null;
                    string fileTimeIdBulanGFormat = $"{datePeriode:yyMM}";
                    string varDCEXT = await _db.GetDcExt();
                    string VAR_KODE_DC = await _db.GetKodeDc();

                    //
                    // v1062 di commend gk tw fungsi nya buat apa ?? (Sulis,01/03/2021)
                    //
                    // string listAntarDc = "LISTANTARDC";
                    // try {
                    //     GetDataAntarDC.Service wsGetAntarDC = new GetDataAntarDC.Service {
                    //         Url = await _db.GetURLWebService(listAntarDc) ?? _config.Get<string>("WsListAntarDc", _app.GetConfig("ws_list_antar_dc"));
                    //     };
                    //     string sValueGet = wsGetAntarDC.GetDataAntarDC();
                    // 
                    //     List<DataAntarDC> objListAntarDC = _converter.JsonToObj<List<DataAntarDC>>(sValueGet);
                    //     if (objListAntarDC.Count > 0) {
                    //         string tabel = "DC_TABEL_DC_TEMP";
                    //         DataTable dtInsert = _converter.ListToDataTable(objListAntarDC, tabel);
                    //         await _db.TruncateTableOraPg(tabel);
                    //         await _db.BulkInsertIntoOraPg(tabel, dtInsert);
                    //     }
                    // }
                    // catch (Exception ex1) {
                    //     _logger.WriteError(ex1);
                    //     MessageBox.Show(ex.Message, $"Web Service {listAntarDc}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // }
                    //

                    string procName1 = "Id_Bulan_G_Evo";
                    CDbExecProcResult res1 = await _db.CALL_ID_BULAN_G_EVO(procName1, datePeriode);
                    if (res1 == null || !res1.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName1}");
                    }

                    csvFileName = $"ID{fileTimeIdBulanGFormat}.{varDCEXT}";
                    await _qTrfCsv.CreateCSVFile("ID", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"AP{fileTimeIdBulanGFormat}.CSV";
                    await _qTrfCsv.CreateCSVFile("AP", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"TK1{fileTimeIdBulanGFormat}.CSV";
                    await _qTrfCsv.CreateCSVFile("TK1S", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"TK2{fileTimeIdBulanGFormat}.CSV";
                    await _qTrfCsv.CreateCSVFile("TK2", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"TK3S{fileTimeIdBulanGFormat}.CSV";
                    await _qTrfCsv.CreateCSVFile("TK3S", csvFileName, addToQueueForZip: false);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"{await _db.GetKodeDc()}{fileTimeIdBulanGFormat}.CSV";
                    await _qTrfCsv.CreateCSVFile("GXXX", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"MATI{fileTimeIdBulanGFormat}.CSV";
                    await _qTrfCsv.CreateCSVFile("MATI", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"HPPDC_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("HPPDC", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"LBDC_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("LBDC", csvFileName);
                    TargetKirim += JumlahServerKirimCsv;

                    // Tambahan untuk buat file untuk TAXTEMP, Penjualan, dan BPB ATK (Sulis 09/10/2018)
                    string procName2 = "CREATE_TAXTEMPBLN_EVO";
                    CDbExecProcResult res2 = await _db.CALL_DataBulananCentralisasiHO(procName2, $"{datePeriode:yyyyMM}");
                    if (res2 == null || !res2.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName2}");
                    }

                    csvFileName = $"TAXTEMP_SUM_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("TAXTEMP_SUM", csvFileName, addToQueueForZip: false);
                    lsCsvToZip.Add("TAXTEMP_SUM");
                    // TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"TAXTEMP_DETAIL_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("TAXTEMP_DETAIL", csvFileName, addToQueueForZip: false);
                    lsCsvToZip.Add("TAXTEMP_DETAIL");
                    // TargetKirim += JumlahServerKirimCsv;

                    string procName3 = "CREATE_PJBLN_EVO";
                    CDbExecProcResult res3 = await _db.CALL_DataBulananCentralisasiHO(procName3, $"{datePeriode:yyyyMM}");
                    if (res3 == null || !res3.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName3}");
                    }

                    csvFileName = $"PJ_DC_SUM_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("PJ_DC_SUM", csvFileName, addToQueueForZip: false);
                    lsCsvToZip.Add("PJ_DC_SUM");
                    // TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"PJ_DC_DET_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("PJ_DC_DET", csvFileName, addToQueueForZip: false);
                    lsCsvToZip.Add("PJ_DC_DET");
                    // TargetKirim += JumlahServerKirimCsv;

                    string procName4 = "CREATE_BPBATKBLN_EVO";
                    CDbExecProcResult res4 = await _db.CALL_DataBulananCentralisasiHO(procName4, $"{datePeriode:yyyyMM}");
                    if (res4 == null || !res4.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName4}");
                    }

                    csvFileName = $"DCATK_SUM_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("DCATK_SUM", csvFileName, addToQueueForZip: false);
                    lsCsvToZip.Add("DCATK_SUM");
                    // TargetKirim += JumlahServerKirimCsv;

                    csvFileName = $"DCATK_DET_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("DCATK_DET", csvFileName, addToQueueForZip: false);
                    lsCsvToZip.Add("DCATK_DET");
                    // TargetKirim += JumlahServerKirimCsv;

                    // Tambahan data JKM 03/10/2018 Sulis
                    string procName5 = "GET_JKM_EVO";
                    CDbExecProcResult res5 = await _db.CALL__P_TGL(procName5, datePeriode);
                    if (res5 == null || !res5.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName5}");
                    }

                    string jkm = $"IDM_GL_DEL_TOKO_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("JKM", jkm, addToQueueForZip: false);
                    TargetKirim += 1;

                    // Tambahan insert data DSI 22/03/2019 Sulis
                    string dsiWsToko = "DSI_WSTOKO";
                    try {
                        DSI_WS.DSI_WS dsi_ws = new DSI_WS.DSI_WS {
                            Url = await _db.GetURLWebService(dsiWsToko) ?? _config.Get<string>("WsDsi", _app.GetConfig("ws_dsi"))
                        };
                        string responseDsiDetail = dsi_ws.Get_DSIDetail(datePeriode);

                        List<DataDSI_WS> objListDataDSIWS = _converter.JsonToObj<List<DataDSI_WS>>(responseDsiDetail);
                        if (objListDataDSIWS.Count > 0) {
                            string tabel = $"DC_{dsiWsToko}";
                            DataTable dtInsert = _converter.ListToDataTable(objListDataDSIWS, tabel);
                            await _db.BulananDeleteDcDsiWsToko(fileTimeIdBulanGFormat);
                            await _db.OraPg_BulkInsertInto(tabel, dtInsert);
                        }
                    }
                    catch (Exception ex2) {
                        _logger.WriteError(ex2);
                        MessageBox.Show(ex2.Message, $"Web Service {dsiWsToko}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    string procName6 = "GET_DSI_EVO";
                    CDbExecProcResult res6 = await _db.CALL_DataBulananCentralisasiHO(procName6, $"{datePeriode:yyyyMM}");
                    if (res6 == null || !res6.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName6}");
                    }

                    string dsiWsDc = "DSI_WSDC";
                    try {
                        GetAnalisaDSIHO.Service dsi_ho = new GetAnalisaDSIHO.Service {
                            Url = await _db.GetURLWebService(dsiWsDc) ?? _config.Get<string>("WsDsiHo", _app.GetConfig("ws_dsi_ho"))
                        };
                        DataTable dtDCAnalisa = await _db.BulananDsiGetDataTable(int.Parse($"{datePeriode:yyyyMM}"));

                        List<DataDSI_ANALISA> lsDCAnalisa = _converter.DataTableToList<DataDSI_ANALISA>(dtDCAnalisa);
                        string dcAnalisa = _converter.ObjectToJson(lsDCAnalisa);
                        string responseDSIHO = dsi_ho.SendDSI(dcAnalisa);
                    }
                    catch (Exception ex3) {
                        _logger.WriteError(ex3);
                        MessageBox.Show(ex3.Message, $"Web Service {dsiWsDc}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    string procName7 = "TRF_NBRMRBREAD_EVO";
                    CDbExecProcResult res7 = await _db.CALL__P_TGL(procName7, datePeriode);
                    if (res7 == null || !res7.STATUS) {
                        throw new Exception($"Gagal Menjalankan Procedure {procName7}");
                    }

                    string nbrMrBread = $"KOMPENSASI_NBR_{VAR_KODE_DC}_{datePeriode:MMM-yy}.CSV";
                    await _qTrfCsv.CreateCSVFile("NBRMRBREAD", nbrMrBread, addToQueueForZip: false);
                    TargetKirim += 1;

                    _berkas.BackupAllFilesInTempFolder();

                    string zipFileName = $"{VAR_KODE_DC}_SENTRAL_{datePeriode:MMM-yy}.ZIP";
                    List<string> listFileNameToZip = await _qTrfCsv.GetFileNameMulti(lsCsvToZip);
                    _berkas.ZipListFileInFolder(zipFileName, listFileNameToZip);
                    foreach (string fileNameInZipToBeDeleted in listFileNameToZip) {
                        _berkas.DeleteSingleFileInFolder(fileNameInZipToBeDeleted);
                    }
                    TargetKirim += 1;

                    BerhasilKirim += (await _dcFtpT.KirimAllCsv("LOCAL")).Success.Count; // *.CSV Sebanyak :: TargetKirim
                    BerhasilKirim += (await _dcFtpT.KirimAllCsvAtauZipFtpDev("Data Bulanan")).Success.Count; // *.CSV Sebanyak :: TargetKirim

                    csvFileName = await _db.Q_TRF_CSV__GET("q_namafile", "JKM") ?? jkm;
                    BerhasilKirim += (await _dcFtpT.KirimSingleCsv("TTF", csvFileName)).Success.Count; // *.CSV Sebanyak :: 1
                    BerhasilKirim += (await _dcFtpT.KirimSingleZip("TTF", zipFileName)).Success.Count; // *.ZIP Sebanyak :: 1

                    csvFileName = await _db.Q_TRF_CSV__GET("q_namafile", "NBRMRBREAD") ?? nbrMrBread;
                    BerhasilKirim += (await _dcFtpT.KirimSingleCsv("NBRMRBREAD", csvFileName)).Success.Count; // *.CSV Sebanyak :: 1

                    _berkas.CleanUp();
                }
            });
            CheckHasilKiriman();
        }

    }

}
