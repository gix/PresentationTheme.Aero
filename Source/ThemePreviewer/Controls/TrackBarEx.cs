namespace ThemePreviewer.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public class TrackBarEx : TrackBar
    {
        private bool hideThumb;
        private bool enableSelRange;
        private int selectionStart;
        private int selectionEnd;

        public TrackBarEx()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                if (HideThumb)
                    cp.Style |= NativeMethods.TBS_NOTHUMB;
                if (EnableSelRange)
                    cp.Style |= NativeMethods.TBS_ENABLESELRANGE;
                return cp;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (Parent != null)
                BackColor = Parent.BackColor;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (EnableSelRange) {
                UpdateSelectionStart();
                UpdateSelectionEnd();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool HideThumb
        {
            get => hideThumb;
            set
            {
                if (HideThumb != value) {
                    hideThumb = value;
                    if (IsHandleCreated)
                        RecreateHandle();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool EnableSelRange
        {
            get { return enableSelRange; }
            set
            {
                if (enableSelRange != value) {
                    enableSelRange = value;
                    if (IsHandleCreated)
                        RecreateHandle();
                }
            }
        }


        [Category("Behavior")]
        [DefaultValue(0)]
        public int SelectionStart
        {
            get { return selectionStart; }
            set
            {
                if (selectionStart != value) {
                    selectionStart = value;
                    if (IsHandleCreated)
                        UpdateSelectionStart();
                }
            }
        }

        private void UpdateSelectionStart()
        {
            var value = SelectionStart;
            if (Orientation == Orientation.Vertical)
                value = Minimum + Maximum - SelectionEnd;
            NativeMethods.SendMessage(Handle, NativeMethods.TBM_SETSELSTART, (IntPtr)1, (IntPtr)value);
        }

        private void UpdateSelectionEnd()
        {
            var value = SelectionEnd;
            if (Orientation == Orientation.Vertical)
                value = Minimum + Maximum - SelectionStart;
            NativeMethods.SendMessage(Handle, NativeMethods.TBM_SETSELEND, (IntPtr)1, (IntPtr)value);
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        public int SelectionEnd
        {
            get { return selectionEnd; }
            set
            {
                if (selectionEnd != value) {
                    selectionEnd = value;
                    if (IsHandleCreated)
                        UpdateSelectionEnd();
                }
            }
        }
    }
}
