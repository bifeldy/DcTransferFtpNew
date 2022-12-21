﻿/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Mail         :: bias@indomaret.co.id
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Custom Toolbox FlowLayoutPanel Yang Auto Scoll Mental Ke Atas
 *              :: https://stackoverflow.com/questions/6443086/prevent-flowlayoutpanel-scrolling-to-updated-control
 * 
 */

using System.Windows.Forms;

namespace DcTransferFtpNew.Components {

    public sealed class FixAutoScrollFlowLayoutPanel : FlowLayoutPanel {

        protected override System.Drawing.Point ScrollToControl(Control activeControl) {
            // return base.ScrollToControl(activeControl);
            return this.AutoScrollPosition;
        }

    }

}