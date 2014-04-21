namespace ThemeTestApp
{
    using System;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class TreeViewEx : TreeView
    {
        private BitVector32 treeViewState = new BitVector32(0);

        public TreeViewEx()
        {

        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_CREATE) {
                VisualStylesNativeMethods.SetWindowTheme(Handle, "Explorer", null);
            }

            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateExtendedStyles();
        }

        protected virtual void UpdateExtendedStyles()
        {
            if (!IsHandleCreated)
                return;

            int controlExStyles = 0;
            if (MultiSelect)
                controlExStyles |= NativeMethods.TVS_EX_MULTISELECT;
            if (UseDoubleBuffering)
                controlExStyles |= NativeMethods.TVS_EX_DOUBLEBUFFER;
            if (NoIndent)
                controlExStyles |= NativeMethods.TVS_EX_NOINDENTSTATE;
            if (RichTooltip)
                controlExStyles |= NativeMethods.TVS_EX_RICHTOOLTIP;
            if (AutoHorizontalScroll)
                controlExStyles |= NativeMethods.TVS_EX_AUTOHSCROLL;
            if (FadeInOutExpandos)
                controlExStyles |= NativeMethods.TVS_EX_FADEINOUTEXPANDOS;
            if (PartialCheckBoxes)
                controlExStyles |= NativeMethods.TVS_EX_PARTIALCHECKBOXES;
            if (ExclusionCheckBoxes)
                controlExStyles |= NativeMethods.TVS_EX_EXCLUSIONCHECKBOXES;
            if (DimmedCheckBoxes)
                controlExStyles |= NativeMethods.TVS_EX_DIMMEDCHECKBOXES;
            if (DrawImageAsync)
                controlExStyles |= NativeMethods.TVS_EX_DRAWIMAGEASYNC;

            SetControlExtendedStyle(controlExStyles, controlExStyles);
        }

        private int GetControlExtendedStyle()
        {
            return (int)NativeMethods.SendMessage(
                new HandleRef(this, Handle), NativeMethods.TVM_GETEXTENDEDSTYLE, IntPtr.Zero, IntPtr.Zero);
        }

        private void SetControlExtendedStyle(int value, int mask)
        {
            NativeMethods.SendMessage(
                new HandleRef(this, Handle), NativeMethods.TVM_SETEXTENDEDSTYLE, (IntPtr)value, (IntPtr)mask);
        }

        public bool MultiSelect
        {
            get { return treeViewState[NativeMethods.TVS_EX_MULTISELECT]; }
            set
            {
                if (MultiSelect != value) {
                    treeViewState[NativeMethods.TVS_EX_MULTISELECT] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool UseDoubleBuffering
        {
            get { return treeViewState[NativeMethods.TVS_EX_DOUBLEBUFFER]; }
            set
            {
                if (UseDoubleBuffering != value) {
                    treeViewState[NativeMethods.TVS_EX_DOUBLEBUFFER] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool NoIndent
        {
            get { return treeViewState[NativeMethods.TVS_EX_NOINDENTSTATE]; }
            set
            {
                if (NoIndent != value) {
                    treeViewState[NativeMethods.TVS_EX_NOINDENTSTATE] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool RichTooltip
        {
            get { return treeViewState[NativeMethods.TVS_EX_RICHTOOLTIP]; }
            set
            {
                if (RichTooltip != value) {
                    treeViewState[NativeMethods.TVS_EX_RICHTOOLTIP] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool AutoHorizontalScroll
        {
            get { return treeViewState[NativeMethods.TVS_EX_AUTOHSCROLL]; }
            set
            {
                if (AutoHorizontalScroll != value) {
                    treeViewState[NativeMethods.TVS_EX_AUTOHSCROLL] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool FadeInOutExpandos
        {
            get { return treeViewState[NativeMethods.TVS_EX_FADEINOUTEXPANDOS]; }
            set
            {
                if (FadeInOutExpandos != value) {
                    treeViewState[NativeMethods.TVS_EX_FADEINOUTEXPANDOS] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool PartialCheckBoxes
        {
            get { return treeViewState[NativeMethods.TVS_EX_PARTIALCHECKBOXES]; }
            set
            {
                if (PartialCheckBoxes != value) {
                    treeViewState[NativeMethods.TVS_EX_PARTIALCHECKBOXES] = value;
                    if (value)
                        CheckBoxes = true;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool ExclusionCheckBoxes
        {
            get { return treeViewState[NativeMethods.TVS_EX_EXCLUSIONCHECKBOXES]; }
            set
            {
                if (ExclusionCheckBoxes != value) {
                    treeViewState[NativeMethods.TVS_EX_EXCLUSIONCHECKBOXES] = value;
                    if (value)
                        CheckBoxes = true;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool DimmedCheckBoxes
        {
            get { return treeViewState[NativeMethods.TVS_EX_DIMMEDCHECKBOXES]; }
            set
            {
                if (DimmedCheckBoxes != value) {
                    treeViewState[NativeMethods.TVS_EX_DIMMEDCHECKBOXES] = value;
                    if (value)
                        CheckBoxes = true;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool DrawImageAsync
        {
            get { return treeViewState[NativeMethods.TVS_EX_DRAWIMAGEASYNC]; }
            set
            {
                if (DrawImageAsync != value) {
                    treeViewState[NativeMethods.TVS_EX_DRAWIMAGEASYNC] = value;
                    UpdateExtendedStyles();
                }
            }
        }
    }
}