/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Handler Click Per Button Menu
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using DcTransferFtpNew.Panels;

namespace DcTransferFtpNew.Handlers {

    public interface IMenuNavigations {
        List<Button> ButtonMenuNavList { get; }
        void AddButtonToPanel(Control currentControl);
    }

    public sealed class CMenuNavigations : IMenuNavigations {

        public delegate void MyFunction(bool x);

        public List<Button> ButtonMenuNavList { get; } = new List<Button>();

        public CMenuNavigations() {
            InitializeNavButtonMenu();
        }

        private void InitializeNavButtonMenu() {
            // Button Buat UI Panel Ex. `\Navigations\***_.cs`, Name => Gunakan Nama `class` Dan Di `sealed`
            ButtonMenuNavList.Add(new Button() { Name = "CProsesHarian", Text = "PROSES HARIAN" });
            ButtonMenuNavList.Add(new Button() { Name = "CProsesBulanan", Text = "PROSES BULANAN" });
            ButtonMenuNavList.Add(new Button() { Name = "CTransfer", Text = "TRANSFER SETTINGS" });
            ButtonMenuNavList.Add(new Button() { Name = "CHapus", Text = "HAPUS BERKAS" });
        }

        public void AddButtonToPanel(Control currentControl) {
            CMainPanel mainPanel = (CMainPanel) currentControl;
            foreach (Button buttonMenuItem in ButtonMenuNavList) {
                // If Antisipasi Re-Render Akibat Garbage Collector Jalan Saat Minimize -> Normal Window Trigger *_Load();
                if (!mainPanel.NavMenu.Controls.ContainsKey(buttonMenuItem.Name)) {
                    buttonMenuItem.BackColor = SystemColors.ControlLight;
                    buttonMenuItem.FlatStyle = FlatStyle.Flat;
                    buttonMenuItem.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    buttonMenuItem.ForeColor = SystemColors.ControlText;
                    buttonMenuItem.Size = new Size(167, 31);
                    buttonMenuItem.TextAlign = ContentAlignment.MiddleLeft;
                    buttonMenuItem.Click += (object sender, EventArgs e) => {
                        mainPanel.SetIdleBusyStatus(false);

                        // Re-Set Button Color
                        foreach (Control navMenuItem in mainPanel.NavMenu.Controls) {
                            if (navMenuItem is Button) {
                                navMenuItem.BackColor = SystemColors.ControlLight;
                            }
                        }
                        Button btnNavMenu = (Button) sender;
                        btnNavMenu.BackColor = Color.FromArgb(255, 207, 223);

                        // Change `navContent`
                        try {
                            if (!mainPanel.NavContent.Controls.ContainsKey(buttonMenuItem.Name)) {
                                UserControl ctrl = (UserControl)CProgram.Bifeldyz.ResolveNamed(buttonMenuItem.Name);
                                mainPanel.NavContent.Controls.Add(ctrl);
                            }
                            mainPanel.NavContent.Controls[buttonMenuItem.Name].BringToFront();
                        }
                        catch (Exception ex) {
                            MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        mainPanel.SetIdleBusyStatus(true);
                    };
                    mainPanel.NavMenu.Controls.Add(buttonMenuItem);
                }
            }
        }

    }

}