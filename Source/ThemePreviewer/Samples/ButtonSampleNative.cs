namespace ThemePreviewer.Samples
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public partial class ButtonSampleNative : UserControl
    {
        public ButtonSampleNative()
        {
            InitializeComponent();
            button2.Focus();
            button2.NotifyDefault(true);
            button4.Click += OnOpenButtonClicked;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            return;

            var button = VisualStyleElement.Button.PushButton.Normal;

            for (int i = 0; i < 6; ++i) {
                var element = VisualStyleElement.CreateElement(button.ClassName, button.Part, i + 1);
                var r = new VisualStyleRenderer(element);
                r.DrawBackground(e.Graphics, new Rectangle(new Point(100, 3 + i * 29), new Size(75, 23)));
            }
        }

        private void OnOpenButtonClicked(object sender, EventArgs eventArgs)
        {
            int padding = 3;
            int spacing = 6;
            var buttonSize = new Size(75, 23);

            var form = new Form();
            form.SizeGripStyle = SizeGripStyle.Hide;
            form.ClientSize = new Size(
                padding + buttonSize.Width + spacing + buttonSize.Width + padding,
                padding + buttonSize.Height + padding);

            var acceptButton = new Button();
            form.Controls.Add(acceptButton);
            form.AcceptButton = acceptButton;
            acceptButton.Text = "OK";
            acceptButton.Size = buttonSize;
            acceptButton.Location = new Point(padding, padding);
            acceptButton.TabIndex = 1;
            acceptButton.Name = "button1";
            acceptButton.UseVisualStyleBackColor = true;
            acceptButton.FlatStyle = FlatStyle.System;

            var cancelButton = new Button();
            form.Controls.Add(cancelButton);
            cancelButton.Text = "Cancel";
            cancelButton.Size = buttonSize;
            cancelButton.Location = new Point(padding + buttonSize.Width + spacing, padding);
            cancelButton.TabIndex = 2;
            cancelButton.Name = "button2";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.FlatStyle = FlatStyle.System;

            form.Show(this);
        }
    }
}
