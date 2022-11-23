/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
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

using Autofac;

using bifeldy_sd3_lib_452;

using DcTransferFtpNew.Forms;
using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Logics;
using DcTransferFtpNew.Navigations;
using DcTransferFtpNew.Panels;
using DcTransferFtpNew.Utilities;

namespace DcTransferFtpNew {

    static class CProgram {

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
                    SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

                    // Dependency Injection
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    Bifeldyz = new Bifeldy(args);

                    // Classes As Interfaces
                    Bifeldyz.RegisterDiClassAsInterfaceByNamespace(assembly, new string[] { "Handlers", "Utilities" });
                    // Bifeldyz.RegisterDiClassAsInterface<CApp, IApp>();
                    // Bifeldyz.RegisterDiClassAsInterface<CDb, IDb>();
                    // Bifeldyz.RegisterDiClassAsInterface<CMenuNavigations, IMenuNavigations>();
                    // Bifeldyz.RegisterDiClassAsInterface<CQTrfCsv, IQTrfCsv>();
                    // Bifeldyz.RegisterDiClassAsInterface<CDcFtpT, IDcFtpT>();

                    // Classes Only -- Named, Access by String
                    Bifeldyz.RegisterDiClassNamedByNamespace(assembly, new string[] { "Logics", "Navigations" });
                    // Bifeldyz.RegisterDiClassNamed<CProsesHarianDataPldc>();
                    // Bifeldyz.RegisterDiClassNamed<CProsesHarianDataHarian>();
                    // Bifeldyz.RegisterDiClassNamed<CProsesHarian>();

                    // Classes Only
                    Bifeldyz.RegisterDiClassByNamespace(assembly, new string[] { /* "Forms", */ "Panels" });
                    // Bifeldyz.RegisterDiClass<CMainPanel>();
                    // Bifeldyz.RegisterDiClass<CLogin>();
                    // Bifeldyz.RegisterDiClass<CCekProgram>();
                    // Bifeldyz.RegisterDiClass<CDbSelector>();

                    // Khusus Form Bisa Di Bikin Independen, Jadinya Gak Wajib Masuk Ke DI
                    // Misal :: Di Buat Dan Di Panggil Dari From Lain
                    //     Form CReportLaporan reportLaporan = new CReportLaporan();
                    //     reportLaporan.SetLaporan(dataTable, paramList, rdlcPath, dataSetName)
                    //     reportLaporan.Show();
                    //     reportLaporan.Close();
                    Bifeldyz.RegisterDiClass<CMainForm>();

                    using (ILifetimeScope lifetimeScope = Bifeldyz.BeginLifetimeScope()) {
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
