namespace ThemePreviewer.Controls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using ThemeCore.Native;

    public sealed class ListViewEx : ListView
    {
        private BitVector32 listViewState = new BitVector32(0);

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg) {
                case NativeMethods.WM_CREATE:
                    if (UseExplorerStyle)
                        StyleNativeMethods.SetWindowTheme(Handle, "Explorer", null);
                    break;
            }

            base.WndProc(ref m);
        }

        [DefaultValue(false)]
        [Category("Appearance")]
        public bool UseExplorerStyle { get; set; }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateExtendedStyles();
        }

        private new void UpdateExtendedStyles()
        {
            if (!IsHandleCreated)
                return;

            int controlExStyles = 0;
            if (GridLines)
                controlExStyles |= NativeMethods.LVS_EX_GRIDLINES;
            //if (ShowPlusMinus)
            //    controlExStyles |= NativeMethods.LVS_EX_SUBITEMIMAGES;
            if (CheckBoxes)
                controlExStyles |= NativeMethods.LVS_EX_CHECKBOXES;
            if (HoverSelection)
                controlExStyles |= NativeMethods.LVS_EX_TRACKSELECT;
            if (AllowColumnReorder)
                controlExStyles |= NativeMethods.LVS_EX_HEADERDRAGDROP;
            if (FullRowSelect)
                controlExStyles |= NativeMethods.LVS_EX_FULLROWSELECT;
            if (Activation == ItemActivation.OneClick)
                controlExStyles |= NativeMethods.LVS_EX_ONECLICKACTIVATE;
            if (Activation == ItemActivation.TwoClick)
                controlExStyles |= NativeMethods.LVS_EX_TWOCLICKACTIVATE;
            if (FlatScrollBars)
                controlExStyles |= NativeMethods.LVS_EX_FLATSB;
            //if (ShowPlusMinus)
            //    controlExStyles |= NativeMethods.LVS_EX_REGIONAL;
            if (ShowItemToolTips)
                controlExStyles |= NativeMethods.LVS_EX_INFOTIP;
            if (HotTracking)
                controlExStyles |= NativeMethods.LVS_EX_UNDERLINEHOT;
            if (ColdTracking)
                controlExStyles |= NativeMethods.LVS_EX_UNDERLINECOLD;
            //if (ShowPlusMinus)
            //    controlExStyles |= NativeMethods.LVS_EX_MULTIWORKAREAS;
            if (LabelTip)
                controlExStyles |= NativeMethods.LVS_EX_LABELTIP;
            if (BorderSelect)
                controlExStyles |= NativeMethods.LVS_EX_BORDERSELECT;
            if (UseDoubleBuffering)
                controlExStyles |= NativeMethods.LVS_EX_DOUBLEBUFFER;
            if (HideLabels)
                controlExStyles |= NativeMethods.LVS_EX_HIDELABELS;
            //if (ShowPlusMinus)
            //    controlExStyles |= NativeMethods.LVS_EX_SINGLEROW;
            //if (ShowPlusMinus)
            //    controlExStyles |= NativeMethods.LVS_EX_SNAPTOGRID;
            if (SimpleSelect)
                controlExStyles |= NativeMethods.LVS_EX_SIMPLESELECT;
            if (JustifyColumns)
                controlExStyles |= NativeMethods.LVS_EX_JUSTIFYCOLUMNS;
            if (TransparentBackground)
                controlExStyles |= NativeMethods.LVS_EX_TRANSPARENTBKGND;
            if (TransparentShadowBackground)
                controlExStyles |= NativeMethods.LVS_EX_TRANSPARENTSHADOWTEXT;
            if (AutoAutoArrange)
                controlExStyles |= NativeMethods.LVS_EX_AUTOAUTOARRANGE;
            if (HeaderInAllViews)
                controlExStyles |= NativeMethods.LVS_EX_HEADERINALLVIEWS;
            if (AutoCheckSelect)
                controlExStyles |= NativeMethods.LVS_EX_AUTOCHECKSELECT;
            if (AutoSizeColumns)
                controlExStyles |= NativeMethods.LVS_EX_AUTOSIZECOLUMNS;
            //if (ShowPlusMinus)
            //    controlExStyles |= NativeMethods.LVS_EX_COLUMNSNAPPOINTS;
            //if (ShowPlusMinus)
            //    controlExStyles |= NativeMethods.LVS_EX_COLUMNOVERFLOW;

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

        public bool FlatScrollBars
        {
            get { return listViewState[NativeMethods.LVS_EX_FLATSB]; }
            set
            {
                if (FlatScrollBars != value) {
                    listViewState[NativeMethods.LVS_EX_FLATSB] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool ColdTracking
        {
            get { return listViewState[NativeMethods.LVS_EX_UNDERLINECOLD]; }
            set
            {
                if (ColdTracking != value) {
                    listViewState[NativeMethods.LVS_EX_UNDERLINECOLD] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool LabelTip
        {
            get { return listViewState[NativeMethods.LVS_EX_LABELTIP]; }
            set
            {
                if (LabelTip != value) {
                    listViewState[NativeMethods.LVS_EX_LABELTIP] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool BorderSelect
        {
            get { return listViewState[NativeMethods.LVS_EX_BORDERSELECT]; }
            set
            {
                if (BorderSelect != value) {
                    listViewState[NativeMethods.LVS_EX_BORDERSELECT] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool UseDoubleBuffering
        {
            get { return listViewState[NativeMethods.LVS_EX_DOUBLEBUFFER]; }
            set
            {
                if (UseDoubleBuffering != value) {
                    listViewState[NativeMethods.LVS_EX_DOUBLEBUFFER] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool HideLabels
        {
            get { return listViewState[NativeMethods.LVS_EX_HIDELABELS]; }
            set
            {
                if (HideLabels != value) {
                    listViewState[NativeMethods.LVS_EX_HIDELABELS] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool SimpleSelect
        {
            get { return listViewState[NativeMethods.LVS_EX_SIMPLESELECT]; }
            set
            {
                if (SimpleSelect != value) {
                    listViewState[NativeMethods.LVS_EX_SIMPLESELECT] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool JustifyColumns
        {
            get { return listViewState[NativeMethods.LVS_EX_JUSTIFYCOLUMNS]; }
            set
            {
                if (JustifyColumns != value) {
                    listViewState[NativeMethods.LVS_EX_JUSTIFYCOLUMNS] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool TransparentBackground
        {
            get { return listViewState[NativeMethods.LVS_EX_TRANSPARENTBKGND]; }
            set
            {
                if (TransparentBackground != value) {
                    listViewState[NativeMethods.LVS_EX_TRANSPARENTBKGND] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool TransparentShadowBackground
        {
            get { return listViewState[NativeMethods.LVS_EX_TRANSPARENTSHADOWTEXT]; }
            set
            {
                if (TransparentShadowBackground != value) {
                    listViewState[NativeMethods.LVS_EX_TRANSPARENTSHADOWTEXT] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool AutoAutoArrange
        {
            get { return listViewState[NativeMethods.LVS_EX_AUTOAUTOARRANGE]; }
            set
            {
                if (AutoAutoArrange != value) {
                    listViewState[NativeMethods.LVS_EX_AUTOAUTOARRANGE] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool HeaderInAllViews
        {
            get { return listViewState[NativeMethods.LVS_EX_HEADERINALLVIEWS]; }
            set
            {
                if (HeaderInAllViews != value) {
                    listViewState[NativeMethods.LVS_EX_HEADERINALLVIEWS] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool AutoCheckSelect
        {
            get { return listViewState[NativeMethods.LVS_EX_AUTOCHECKSELECT]; }
            set
            {
                if (AutoCheckSelect != value) {
                    listViewState[NativeMethods.LVS_EX_AUTOCHECKSELECT] = value;
                    UpdateExtendedStyles();
                }
            }
        }

        public bool AutoSizeColumns
        {
            get { return listViewState[NativeMethods.LVS_EX_AUTOSIZECOLUMNS]; }
            set
            {
                if (AutoSizeColumns != value) {
                    listViewState[NativeMethods.LVS_EX_AUTOSIZECOLUMNS] = value;
                    UpdateExtendedStyles();
                }
            }
        }
    }
}
