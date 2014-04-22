namespace ThemePreviewer.Samples
{
    using Controls;

    partial class ListBoxSampleNative
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sysListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // sysListBox
            // 
            this.sysListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sysListBox.IntegralHeight = false;
            this.sysListBox.ItemHeight = 15;
            this.sysListBox.Location = new System.Drawing.Point(3, 3);
            this.sysListBox.Name = "sysListBox";
            this.sysListBox.Size = new System.Drawing.Size(715, 716);
            this.sysListBox.TabIndex = 2;
            // 
            // ListBoxSampleNative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.sysListBox);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ListBoxSampleNative";
            this.Size = new System.Drawing.Size(721, 722);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListBox sysListBox;
    }
}
