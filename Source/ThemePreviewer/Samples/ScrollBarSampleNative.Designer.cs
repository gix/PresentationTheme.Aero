namespace ThemePreviewer.Samples
{
    partial class ScrollBarSampleNative
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
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.hScrollBar2 = new System.Windows.Forms.HScrollBar();
            this.vScrollBar2 = new System.Windows.Forms.VScrollBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.sizeBox2 = new ThemePreviewer.Controls.SizeBox();
            this.sizeBox1 = new ThemePreviewer.Controls.SizeBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hScrollBar1.Location = new System.Drawing.Point(3, 423);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(397, 17);
            this.hScrollBar1.TabIndex = 0;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vScrollBar1.Location = new System.Drawing.Point(400, 3);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 420);
            this.vScrollBar1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.vScrollBar1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.hScrollBar1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(420, 443);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.hScrollBar2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.vScrollBar2, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(397, 420);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // hScrollBar2
            // 
            this.hScrollBar2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hScrollBar2.Enabled = false;
            this.hScrollBar2.LargeChange = 20;
            this.hScrollBar2.Location = new System.Drawing.Point(0, 400);
            this.hScrollBar2.Name = "hScrollBar2";
            this.hScrollBar2.Size = new System.Drawing.Size(377, 17);
            this.hScrollBar2.TabIndex = 0;
            // 
            // vScrollBar2
            // 
            this.vScrollBar2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vScrollBar2.Enabled = false;
            this.vScrollBar2.LargeChange = 20;
            this.vScrollBar2.Location = new System.Drawing.Point(377, 0);
            this.vScrollBar2.Name = "vScrollBar2";
            this.vScrollBar2.Size = new System.Drawing.Size(17, 400);
            this.vScrollBar2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.sizeBox2);
            this.panel1.Controls.Add(this.sizeBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(371, 394);
            this.panel1.TabIndex = 2;
            // 
            // sizeBox2
            // 
            this.sizeBox2.Enabled = false;
            this.sizeBox2.Location = new System.Drawing.Point(3, 31);
            this.sizeBox2.Name = "sizeBox2";
            this.sizeBox2.Size = new System.Drawing.Size(17, 17);
            this.sizeBox2.TabIndex = 1;
            // 
            // sizeBox1
            // 
            this.sizeBox1.Location = new System.Drawing.Point(3, 0);
            this.sizeBox1.Name = "sizeBox1";
            this.sizeBox1.Size = new System.Drawing.Size(17, 17);
            this.sizeBox1.TabIndex = 0;
            // 
            // ScrollBarSampleNative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ScrollBarSampleNative";
            this.Size = new System.Drawing.Size(420, 443);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.HScrollBar hScrollBar2;
        private System.Windows.Forms.VScrollBar vScrollBar2;
        private System.Windows.Forms.Panel panel1;
        private Controls.SizeBox sizeBox2;
        private Controls.SizeBox sizeBox1;
    }
}
