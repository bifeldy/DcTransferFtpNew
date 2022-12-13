/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: UI Navigation Proses Bulanan
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Abstractions;
using DcTransferFtpNew.Handlers;

namespace DcTransferFtpNew.Navigations {

    public sealed partial class CProsesBulanan : CNavigations {

        public List<Button> ButtonMenuBulananList { get; } = new List<Button>();

        public CProsesBulanan(ILogger logger, IDb db) : base(logger, db) {
            InitializeComponent();
            OnInit();
        }

        public DateTimePicker DateTimePickerBulanan => DateTimePickerBulanan;

        private void OnInit() {
            Dock = DockStyle.Fill;
            InitializeButtonProsesBulanan();
        }

        private void CProsesBulanan_Load(object sender, EventArgs e) {
            AddButtonToMainPanel(Parent.Parent, ButtonMenuBulananList, flowLayoutPanelProsesBulanan);
        }

        private void InitializeButtonProsesBulanan() {
            // Button Buat Handle Ex. `\Logics\***_.cs`, Name => Gunakan Nama `class` Dan Di `sealed`
            ButtonMenuBulananList.Add(new Button() { Name = "CProsesBulananDataBulanan", Text = "Data Bulanan" });
            ButtonMenuBulananList.Add(new Button() { Name = "CProsesBulananTransferMstxhg", Text = "Transfer MSTXHG" });
        }

    }

}
