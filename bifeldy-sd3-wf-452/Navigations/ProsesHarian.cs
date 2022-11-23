﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
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

using DcTransferFtpNew.Abstractions;

namespace DcTransferFtpNew.Navigations {

    public sealed partial class CProsesHarian : CNavigations {

        public List<Button> ButtonMenuHarianList { get; } = new List<Button>();

        public CProsesHarian() {
            InitializeComponent();
            OnInit();
        }

        public DateTimePicker DateTimePickerHarianAwal => dateTimePickerHarianAwal;

        public DateTimePicker DateTimePickerHarianAkhir => dateTimePickerHarianAkhir;

        private void OnInit() {
            Dock = DockStyle.Fill;
            InitializeButtonProsesHarian();
        }

        private void CProsesHarian_Load(object sender, EventArgs e) {
            AddButtonToMainPanel(Parent.Parent, ButtonMenuHarianList, flowLayoutPanelProsesHarian);
        }

        private void InitializeButtonProsesHarian() {
            // Button Buat Handle Ex. `\Logics\***_.cs`, Name => Gunakan Nama `class` Dan Di `sealed`
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianDataHarian", Text = "Data Harian" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianDataPldc", Text = "Data PLDC" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianDataIcho", Text = "Data ICHO" });
            ButtonMenuHarianList.Add(new Button() { Name = "CProsesHarianDataIrpc", Text = "Data IRPC" });
        }

    }

}
