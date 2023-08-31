﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Pengaturan Aplikasi
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

namespace DcTransferFtpNew.Utilities {

    public interface IApp : IApplication {
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        void Exit();
        string Author { get; }
        List<string> ListDcCanUse { get; }
    }

    public sealed class CApp : CApplication, IApp {

        public int ScreenWidth { get; }
        public int ScreenHeight { get; }

        public string Author { get; }

        public List<string> ListDcCanUse { get; }

        public CApp() {
            ScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            ScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            Author = "B. Bias A. Ch. :: bias@indomaret.co.id" + Environment.NewLine + "Sulistyo C. :: chandrianto@indomaret.co.id";
            ListDcCanUse = new List<string> { "HO", "INDUK", "DEPO", "SEWA" };
        }

        public void Exit() => Application.Exit();

    }

}
