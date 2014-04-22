namespace ThemePreviewer.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public class ProgressBarEx : ProgressBar
    {
        private ProgressBarState state = ProgressBarState.Normal;

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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            NativeMethods.SendMessage(
                Handle, NativeMethods.PBM_SETSTATE, (IntPtr)state, IntPtr.Zero);
        }
    }
}
