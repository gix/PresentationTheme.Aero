
namespace ThemePreviewer.Samples
{
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public partial class RadioCheckSampleNative : UserControl
    {
        public RadioCheckSampleNative()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var button = VisualStyleElement.Button.RadioButton.UncheckedNormal;

            for (int i = 0; i < 8; ++i) {
                var element = VisualStyleElement.CreateElement(button.ClassName, button.Part, i + 1);
                var r = new VisualStyleRenderer(element);
                r.DrawBackground(e.Graphics, new Rectangle(new Point(200, 3 + i * 20), new Size(75, 16)));
            }

            button = VisualStyleElement.Button.CheckBox.UncheckedNormal;

            for (int i = 0; i < 20; ++i) {
                var element = VisualStyleElement.CreateElement(button.ClassName, button.Part, i + 1);
                var r = new VisualStyleRenderer(element);
                r.DrawBackground(e.Graphics, new Rectangle(new Point(250, 3 + i * 20), new Size(75, 16)));
            }
        }
    }
}
