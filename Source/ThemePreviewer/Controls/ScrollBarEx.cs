namespace ThemePreviewer.Controls
{
    using System.Drawing;
    using System.Windows.Forms;

    public class SizeBox : ScrollBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style |= NativeMethods.SBS_SIZEBOX;
                cp.Style |= NativeMethods.SBS_SIZEBOXBOTTOMRIGHTALIGN;
                return cp;
            }
        }

        protected override Size DefaultSize =>
            new Size(
                SystemInformation.VerticalScrollBarWidth,
                SystemInformation.HorizontalScrollBarHeight);
    }
}
