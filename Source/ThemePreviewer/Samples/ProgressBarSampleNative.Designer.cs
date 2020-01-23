namespace ThemePreviewer.Samples
{
    partial class ProgressBarSampleNative
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.progressBarEx3 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBarEx2 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBarEx1 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar10 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar8 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar9 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar7 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar6 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar5 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar4 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar2 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar3 = new ThemePreviewer.Controls.ProgressBarEx();
            this.progressBar1 = new ThemePreviewer.Controls.ProgressBarEx();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Segoe UI", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(258, 174);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(18, 18);
            this.button1.TabIndex = 9;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnValueButtonClicked);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Segoe UI", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(281, 174);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(18, 18);
            this.button2.TabIndex = 12;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnChangeStateButtonClicked);
            // 
            // progressBarEx3
            // 
            this.progressBarEx3.Location = new System.Drawing.Point(3, 365);
            this.progressBarEx3.Name = "progressBarEx3";
            this.progressBarEx3.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.progressBarEx3.Size = new System.Drawing.Size(16, 100);
            this.progressBarEx3.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarEx3.TabIndex = 15;
            // 
            // progressBarEx2
            // 
            this.progressBarEx2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarEx2.Location = new System.Drawing.Point(3, 309);
            this.progressBarEx2.MarqueeAnimationSpeed = 30;
            this.progressBarEx2.Name = "progressBarEx2";
            this.progressBarEx2.Size = new System.Drawing.Size(463, 50);
            this.progressBarEx2.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarEx2.TabIndex = 14;
            // 
            // progressBarEx1
            // 
            this.progressBarEx1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarEx1.Location = new System.Drawing.Point(3, 253);
            this.progressBarEx1.Name = "progressBarEx1";
            this.progressBarEx1.Size = new System.Drawing.Size(463, 50);
            this.progressBarEx1.TabIndex = 13;
            this.progressBarEx1.Value = 80;
            // 
            // progressBar10
            // 
            this.progressBar10.Location = new System.Drawing.Point(3, 225);
            this.progressBar10.Name = "progressBar10";
            this.progressBar10.Size = new System.Drawing.Size(250, 16);
            this.progressBar10.State = ThemePreviewer.ProgressBarState.Paused;
            this.progressBar10.TabIndex = 11;
            this.progressBar10.Value = 80;
            // 
            // progressBar8
            // 
            this.progressBar8.Location = new System.Drawing.Point(3, 203);
            this.progressBar8.Name = "progressBar8";
            this.progressBar8.Size = new System.Drawing.Size(250, 16);
            this.progressBar8.State = ThemePreviewer.ProgressBarState.Error;
            this.progressBar8.TabIndex = 10;
            this.progressBar8.Value = 80;
            // 
            // progressBar9
            // 
            this.progressBar9.Location = new System.Drawing.Point(3, 175);
            this.progressBar9.Name = "progressBar9";
            this.progressBar9.Size = new System.Drawing.Size(250, 16);
            this.progressBar9.TabIndex = 8;
            // 
            // progressBar7
            // 
            this.progressBar7.Location = new System.Drawing.Point(3, 147);
            this.progressBar7.MarqueeAnimationSpeed = 30;
            this.progressBar7.Name = "progressBar7";
            this.progressBar7.Size = new System.Drawing.Size(250, 16);
            this.progressBar7.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar7.TabIndex = 6;
            // 
            // progressBar6
            // 
            this.progressBar6.Enabled = false;
            this.progressBar6.Location = new System.Drawing.Point(3, 119);
            this.progressBar6.Name = "progressBar6";
            this.progressBar6.Size = new System.Drawing.Size(250, 16);
            this.progressBar6.TabIndex = 5;
            this.progressBar6.Value = 100;
            // 
            // progressBar5
            // 
            this.progressBar5.Enabled = false;
            this.progressBar5.Location = new System.Drawing.Point(3, 97);
            this.progressBar5.Name = "progressBar5";
            this.progressBar5.Size = new System.Drawing.Size(250, 16);
            this.progressBar5.TabIndex = 4;
            this.progressBar5.Value = 40;
            // 
            // progressBar4
            // 
            this.progressBar4.Enabled = false;
            this.progressBar4.Location = new System.Drawing.Point(3, 75);
            this.progressBar4.Name = "progressBar4";
            this.progressBar4.Size = new System.Drawing.Size(250, 16);
            this.progressBar4.TabIndex = 3;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(3, 25);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(250, 16);
            this.progressBar2.TabIndex = 1;
            this.progressBar2.Value = 80;
            // 
            // progressBar3
            // 
            this.progressBar3.Location = new System.Drawing.Point(3, 47);
            this.progressBar3.Name = "progressBar3";
            this.progressBar3.Size = new System.Drawing.Size(250, 16);
            this.progressBar3.TabIndex = 2;
            this.progressBar3.Value = 100;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(250, 16);
            this.progressBar1.TabIndex = 0;
            // 
            // ProgressBarSampleNative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBarEx3);
            this.Controls.Add(this.progressBarEx2);
            this.Controls.Add(this.progressBarEx1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.progressBar10);
            this.Controls.Add(this.progressBar8);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressBar9);
            this.Controls.Add(this.progressBar7);
            this.Controls.Add(this.progressBar6);
            this.Controls.Add(this.progressBar5);
            this.Controls.Add(this.progressBar4);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.progressBar3);
            this.Controls.Add(this.progressBar1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ProgressBarSampleNative";
            this.Size = new System.Drawing.Size(469, 679);
            this.ResumeLayout(false);

        }

        #endregion

        private ThemePreviewer.Controls.ProgressBarEx progressBar1;
        private ThemePreviewer.Controls.ProgressBarEx progressBar2;
        private ThemePreviewer.Controls.ProgressBarEx progressBar3;
        private ThemePreviewer.Controls.ProgressBarEx progressBar7;
        private ThemePreviewer.Controls.ProgressBarEx progressBar9;
        private System.Windows.Forms.Button button1;
        private ThemePreviewer.Controls.ProgressBarEx progressBar5;
        private ThemePreviewer.Controls.ProgressBarEx progressBar6;
        private ThemePreviewer.Controls.ProgressBarEx progressBar4;
        private ThemePreviewer.Controls.ProgressBarEx progressBar8;
        private ThemePreviewer.Controls.ProgressBarEx progressBar10;
        private System.Windows.Forms.Button button2;
        private Controls.ProgressBarEx progressBarEx1;
        private Controls.ProgressBarEx progressBarEx2;
        private Controls.ProgressBarEx progressBarEx3;
    }
}
