/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
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

    public interface IProsesBulananTransferMstxhg : ILogics { }

    public sealed class CProsesBulananTransferMstxhg : CLogics, IProsesBulananTransferMstxhg {

        private readonly IApp _app;
        private readonly IConverter _converter;
        private readonly IDb _db;
        private readonly IBerkas _berkas;
        private readonly IQTrfCsv _qTrfCsv;
        private readonly IDcFtpT _dcFtpT;

        public CProsesBulananTransferMstxhg(
            IApp app,
            IConverter converter,
            IDb db,
            IBerkas berkas,
            IQTrfCsv q_trf_csv,
            IDcFtpT dc_ftp_t
        ) : base(db) {
            _app = app;
            _converter = converter;
            _db = db;
            _berkas = berkas;
            _qTrfCsv = q_trf_csv;
            _dcFtpT = dc_ftp_t;
        }

        public override Task Run(object sender, EventArgs e, Control currentControl) {
            throw new NotImplementedException("Fitur Belum Di-Implementasi-Kan ...");
        }

    }

}
