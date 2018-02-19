namespace ThemePreviewer.Controls
{
    using System.ComponentModel;
    using System.Windows.Forms;

    public class TabControlEx : TabControl
    {
        private bool tabs;

        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;

                if (Tabs)
                    createParams.Style |= NativeMethods.TCS_TABS;

                return createParams;
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool Tabs
        {
            get => tabs;
            set
            {
                if (Tabs != value) {
                    tabs = value;
                    if (IsHandleCreated)
                        RecreateHandle();
                }
            }
        }
    }
}
