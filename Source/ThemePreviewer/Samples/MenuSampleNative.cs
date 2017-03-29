namespace ThemePreviewer.Samples
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    using StyleCore;
    using StyleCore.Native;

    public partial class MenuSampleNative : Form
    {
        private static int instanceCount;

        public MenuSampleNative()
        {
            instanceCount++;
            InitializeComponent();
            TopLevel = false;
            ContextMenu = contextMenu1;

            if (instanceCount == 1) {
                var b = new Button();
                b.Text = "Open form";
                b.Click += (sender, args) => {
                    var f = new MenuSampleNative();
                    f.TopLevel = true;
                    f.FormBorderStyle = FormBorderStyle.Sizable;
                    f.Show();
                };
                Controls.Add(b);
            }

            //SetOwnerDraw(Menu.MenuItems);
        }

        private void SetOwnerDraw(Menu.MenuItemCollection menuItems)
        {
            foreach (MenuItem item in menuItems) {
                item.OwnerDraw = true;
                item.MeasureItem += OnMeasureItem;
                item.DrawItem += OnDrawItem;
                //SetOwnerDraw(item.MenuItems);
            }
        }

        private void OnMeasureItem(object sender, MeasureItemEventArgs args)
        {
            var item = Menu.MenuItems[args.Index];
            var renderer = GetMenuBarItemRenderer(0);

            using (Font font = renderer.GetFont(args.Graphics, FontProperty.GlyphFont)) {
                SizeF size = args.Graphics.MeasureString(item.Text, font);
                args.ItemWidth = (int)size.Width;
                args.ItemHeight = (int)size.Height;
            }
        }

        private void OnDrawItem(object sender, DrawItemEventArgs args)
        {
            var item = Menu.MenuItems[args.Index];
            var renderer = GetMenuBarItemRenderer(args.State);
            renderer.DrawBackground(args.Graphics, args.Bounds, args.Bounds);
            renderer.DrawText(args.Graphics, args.Bounds, item.Text);
        }

        private VisualStyleRenderer GetMenuBarItemRenderer(DrawItemState stateFlags)
        {
            int state = 1;
            if ((stateFlags & (DrawItemState.HotLight | DrawItemState.Disabled)) == (DrawItemState.HotLight | DrawItemState.Disabled))
                state = (int)BARITEMSTATES.MBI_DISABLEDHOT;
            else if ((stateFlags & DrawItemState.Disabled) != 0)
                state = (int)BARITEMSTATES.MBI_DISABLED;
            else if ((stateFlags & DrawItemState.HotLight) != 0)
                state = (int)BARITEMSTATES.MBI_HOT;
            else if ((stateFlags & DrawItemState.Checked) != 0)
                state = (int)BARITEMSTATES.MBI_PUSHED;

            return new VisualStyleRenderer("MENU", (int)MENUPARTS.MENU_BARITEM, state);
        }
    }
}
