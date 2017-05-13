namespace ThemePreviewer.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public class ProgressBarEx : ProgressBar
    {
        private ProgressBarState state = ProgressBarState.Normal;
        private Orientation orientation = Orientation.Horizontal;

        [DefaultValue(ProgressBarState.Normal)]
        [Category("Behavior")]
        public ProgressBarState State
        {
            get { return state; }
            set
            {
                if (state == value)
                    return;

                state = value;

                if (IsHandleCreated)
                    NativeMethods.SendMessage(
                        Handle, NativeMethods.PBM_SETSTATE, (IntPtr)state, IntPtr.Zero);
            }
        }

        [DefaultValue(Orientation.Horizontal)]
        [Category("Appearance")]
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                if (orientation == value)
                    return;

                orientation = value;

                if (IsHandleCreated)
                    NativeMethods.SetWindowStyle(
                        this, NativeMethods.PBS_VERTICAL, Orientation == Orientation.Vertical);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                if (Orientation == Orientation.Vertical)
                    cp.Style |= NativeMethods.PBS_VERTICAL;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            NativeMethods.SendMessage(
                Handle, NativeMethods.PBM_SETSTATE, (IntPtr)state, IntPtr.Zero);
        }
    }
}
