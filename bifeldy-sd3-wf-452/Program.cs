﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Entry Point
 * 
 */

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using bifeldy_sd3_lib_452;

using DcTransferFtpNew.Forms;
using DcTransferFtpNew.SqlServerTypes;

namespace DcTransferFtpNew {

    public static class CProgram {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static Bifeldy Bifeldyz { get; set; }

        [STAThread] // bifeldy-sd3-wf-452.exe -arg0 arg1 --arg2 "a r g 3"
        public static void Main(string[] args) {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] allProcess = Process.GetProcessesByName(currentProcess.ProcessName);
            using (Mutex mutex = new Mutex(true, currentProcess.MainModule.ModuleName, out bool createdNew)) {
                if (createdNew) {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Report Viewer
                    CLoader.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

                    // Dependency Injection
                    Bifeldyz = new Bifeldy(args);

                    // Classes As Interfaces
                    // Bifeldyz.RegisterDiClassAsInterface<CClass, IInterface>();
                    Bifeldyz.RegisterDiClassAsInterfaceByNamespace(Assembly.GetExecutingAssembly(), new string[] {
                        "DcTransferFtpNew.Handlers",
                        "DcTransferFtpNew.Logics",
                        "DcTransferFtpNew.Utilities"
                    });

                    // Classes Only -- Named, Access by String
                    // Bifeldyz.RegisterDiClassNamed<CClass>();
                    Bifeldyz.RegisterDiClassNamedByNamespace(Assembly.GetExecutingAssembly(), new string[] {
                        "DcTransferFtpNew.Logics",
                        "DcTransferFtpNew.Navigations"
                    });

                    // Pendaftaran Manual Secara Paksa Karena Mau Di Pakai Dari Constructor Juga Sebagai Interface
                    // Atau Tambahin Saja Semua "Logics" Ke "RegisterDiClassAsInterfaceByNamespace(...)" Di Atas
                    // Bifeldyz.RegisterDiClassAsInterface<CProsesHarianTaxFull, IProsesHarianTaxFull>();

                    // Classes Only
                    // Bifeldyz.RegisterDiClass<CClass>();
                    Bifeldyz.RegisterDiClassByNamespace(Assembly.GetExecutingAssembly(), new string[] {
                        /* "DcTransferFtpNew.Forms", */
                        "DcTransferFtpNew.Panels"
                    });

                    // Khusus Form Bisa Di Bikin Independen, Jadinya Gak Wajib Masuk Ke Dependency Injection (DI), Form Utama Yang Wajib DI
                    // Kalau Singleton Form Di Panggil Via Resolve DI, Saat Di Close(); Kena Dispose GC, Tidak Bisa Resolve Lagi
                    // Isi Parameter Dengan `false` Supaya Tidak Dapat Object Baru Setiap Instance (Bukan Singleton)
                    //
                    // Misal :: Di Buat Dan Di Panggil Dari From Lain
                    //
                    // Tanpa DI, Tidak Bisa Lanjut Pakai DI Juga Di Constructor
                    //
                    //     CReportLaporan reportLaporan = new CReportLaporan();
                    //
                    // Kalau Via Resolve DI, Enaknya Bisa Lanjut Pakai DI Juga Di Constructor
                    //
                    //     CReportLaporan reportLaporan = CProgram.Bifeldyz.ResolveClass<CReportLaporan>();
                    //
                    Bifeldyz.RegisterDiClass<CMainForm>(false);
                    Bifeldyz.RegisterDiClass<CReportLaporan>(false);

                    using (dynamic lifetimeScope = Bifeldyz.BeginLifetimeScope()) {
                        Application.Run(Bifeldyz.ResolveClass<CMainForm>());
                    }
                }
                else {
                    foreach (Process process in allProcess) {
                        if (process.Id != currentProcess.Id) {
                            SetForegroundWindow(process.MainWindowHandle);
                            MessageBox.Show(
                                "Program Saat Ini Sudah Berjalan, Cek System Tray Icon Kanan Bawah Windows",
                                currentProcess.MainModule.ModuleName,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            break;
                        }
                    }
                }
            }
        }

    }

}
