﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: UI Navigation Proses Harian
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

    public sealed partial class CProsesHarian : CNavigations {

        public List<Button> ButtonMenuHarianList { get; } = new List<Button>();

        public CProsesHarian(ILogger logger, IDb db) : base(logger, db) {
            InitializeComponent();
            OnInit();
        }

        public DateTimePicker DateTimePickerHarianAwal => dateTimePickerHarianAwal;

        public DateTimePicker DateTimePickerHarianAkhir => dateTimePickerHarianAkhir;

        public CheckBox ChkPomgg => chkPomgg;

        private void OnInit() {
            Dock = DockStyle.Fill;
            InitializeButtonProsesHarian();
            RefreshKalender();
        }

        private void RefreshKalender() {
            dateTimePickerHarianAkhir.MaxDate = DateTime.Now;
            dateTimePickerHarianAwal.MaxDate = dateTimePickerHarianAkhir.MaxDate;
            dateTimePickerHarianAkhir.MinDate = dateTimePickerHarianAwal.Value;
        }

        private void CProsesHarian_Load(object sender, EventArgs e) {
            AddButtonToMainPanel(Parent.Parent, ButtonMenuHarianList, flowLayoutPanelProsesHarian);
        }

        private void DateTimePickerHarianAwal_ValueChanged(object sender, EventArgs e) {
            if (dateTimePickerHarianAwal.Value > dateTimePickerHarianAkhir.Value) {
                dateTimePickerHarianAkhir.Value = dateTimePickerHarianAwal.Value;
            }
            RefreshKalender();
        }

        private void DateTimePickerHarianAkhir_ValueChanged(object sender, EventArgs e) {
            RefreshKalender();
        }

        private void InitializeButtonProsesHarian() {
            // Button Buat Handle Ex. `\Logics\***_.cs`, Name => Gunakan Nama `class` Dan Di `sealed`
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianDataHarian", Text = "Data Harian" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianPldc", Text = "Data PLDC" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianIcho", Text = "Data ICHO" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianIrpc", Text = "Data IRPC" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianFingerScan", Text = "Data FingerScan" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianTaxFull", Text = "Data Tax Full" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianTaxRe", Text = "Data Tax Re:" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianTrfRekonOracle", Text = "Trf Rekon Oracle" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianEndorsement", Text = "Data Endorsement" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianRealToc", Text = "Data TOC" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianBpbProc", Text = "Data BPB Proc" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianNpkBap", Text = "Data NPK BAP" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianBrdRekon", Text = "Data BRD Rekon" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianTaxNonFad", Text = "Data TAX Non-FAD" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianAmta", Text = "Data AMTA" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianNrbSup", Text = "Data NRB Sup" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianPomGg", Text = "Data POMGG" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianPom", Text = "Data POM" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianPou", Text = "Data POU" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianExpPlano", Text = "Data EXP Plano" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianMd", Text = "Data MD" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianBtb", Text = "Data BTB" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianDpo", Text = "Data DPO" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianRegSto02", Text = "Data REGSTO02" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianBo", Text = "Data BO" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianNrbkPrimeBread", Text = "NRBK Prime Bread" });
        }

    }

}
