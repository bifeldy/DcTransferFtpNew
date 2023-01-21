/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Template UI Navigations
 *              :: Hanya Untuk Inherit
 *              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using DcTransferFtpNew.Handlers;
using DcTransferFtpNew.Panels;

namespace DcTransferFtpNew.Abstractions {

    // Tidak Bisa Dibuat Menjadi `abstract class` Karena Ini Tampilan
    public class CNavigations : UserControl {

        private readonly ILogger _logger;
        private readonly IDb _db;

        public CNavigations() { }

        public CNavigations(ILogger logger, IDb db) {
            _logger = logger;
            _db = db;
        }

        protected void AddButtonToMainPanel(Control panel, List<Button> buttonList, FlowLayoutPanel flowLayoutPanel) {
            CMainPanel mainPanel = (CMainPanel) panel;
            foreach (Button button in buttonList) {
                // If Antisipasi Re-Render Akibat Garbage Collector Jalan Saat 'Minimize' -> 'Normal' Window Trigger *_Load();
                if (!flowLayoutPanel.Controls.ContainsKey(button.Name)) {
                    button.BackColor = SystemColors.ControlLight;
                    button.FlatStyle = FlatStyle.Flat;
                    button.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    button.ForeColor = SystemColors.ControlText;
                    button.Size = new Size(158, 31);
                    button.TextAlign = ContentAlignment.MiddleCenter;
                    button.Click += async (object sender, EventArgs e) => {
                        mainPanel.SetIdleBusyStatus(false);

                        // Re-Set Button Color
                        foreach (Control flowLayoutPanelItem in flowLayoutPanel.Controls) {
                            if (flowLayoutPanelItem is Button) {
                                flowLayoutPanelItem.BackColor = SystemColors.ControlLight;
                            }
                        }
                        Button buttonSender = (Button) sender;
                        buttonSender.BackColor = Color.FromArgb(255, 207, 223);

                        try {
                            CLogics cls = (CLogics) CProgram.Bifeldyz.ResolveNamed(buttonSender.Name);
                            mainPanel.LogInfo.Text = string.Empty;
                            // await _db.MarkBeforeCommitRollback();
                            await cls.Run(sender, e, this);
                            // _db.MarkSuccessCommitAndClose();
                        }
                        catch (Exception ex) {
                            // _db.MarkFailedRollbackAndClose();
                            // _logger.WriteError(ex);
                            MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        buttonSender.BackColor = SystemColors.ControlLight;

                        mainPanel.SetIdleBusyStatus(true);
                    };
                    flowLayoutPanel.Controls.Add(button);
                }
            }
        }

    }

}
