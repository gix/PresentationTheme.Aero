namespace ThemePreviewer.Samples
{
    using Controls;

    partial class ListViewSampleNative
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
            this.optionPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.explorerListView = new ThemePreviewer.Controls.ListViewEx();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sysListView = new ThemePreviewer.Controls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // optionPanel
            // 
            this.optionPanel.AutoSize = true;
            this.optionPanel.ColumnCount = 1;
            this.optionPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.optionPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.optionPanel.Location = new System.Drawing.Point(0, 0);
            this.optionPanel.Name = "optionPanel";
            this.optionPanel.RowCount = 1;
            this.optionPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.optionPanel.Size = new System.Drawing.Size(0, 722);
            this.optionPanel.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.explorerListView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.sysListView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(721, 722);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // explorerListView
            // 
            this.explorerListView.AutoAutoArrange = false;
            this.explorerListView.AutoCheckSelect = false;
            this.explorerListView.AutoSizeColumns = false;
            this.explorerListView.BorderSelect = false;
            this.explorerListView.ColdTracking = false;
            this.explorerListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.explorerListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.explorerListView.FlatScrollBars = false;
            this.explorerListView.HeaderInAllViews = false;
            this.explorerListView.HideLabels = false;
            this.explorerListView.JustifyColumns = false;
            this.explorerListView.LabelTip = false;
            this.explorerListView.Location = new System.Drawing.Point(3, 364);
            this.explorerListView.Name = "explorerListView";
            this.explorerListView.SimpleSelect = false;
            this.explorerListView.Size = new System.Drawing.Size(715, 355);
            this.explorerListView.TabIndex = 3;
            this.explorerListView.TransparentBackground = false;
            this.explorerListView.TransparentShadowBackground = false;
            this.explorerListView.UseCompatibleStateImageBehavior = false;
            this.explorerListView.UseDoubleBuffering = false;
            this.explorerListView.UseExplorerStyle = true;
            this.explorerListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Name";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Size";
            this.columnHeader5.Width = 90;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Type With Overflow";
            this.columnHeader6.Width = 80;
            // 
            // sysListView
            // 
            this.sysListView.AutoAutoArrange = false;
            this.sysListView.AutoCheckSelect = false;
            this.sysListView.AutoSizeColumns = false;
            this.sysListView.BorderSelect = false;
            this.sysListView.ColdTracking = false;
            this.sysListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.sysListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sysListView.FlatScrollBars = false;
            this.sysListView.HeaderInAllViews = false;
            this.sysListView.HideLabels = false;
            this.sysListView.JustifyColumns = false;
            this.sysListView.LabelTip = false;
            this.sysListView.Location = new System.Drawing.Point(3, 3);
            this.sysListView.Name = "sysListView";
            this.sysListView.SimpleSelect = false;
            this.sysListView.Size = new System.Drawing.Size(715, 355);
            this.sysListView.TabIndex = 2;
            this.sysListView.TransparentBackground = false;
            this.sysListView.TransparentShadowBackground = false;
            this.sysListView.UseCompatibleStateImageBehavior = false;
            this.sysListView.UseDoubleBuffering = false;
            this.sysListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            this.columnHeader2.Width = 90;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Type With Overflow";
            this.columnHeader3.Width = 80;
            // 
            // ListViewSampleNative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.optionPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ListViewSampleNative";
            this.Size = new System.Drawing.Size(721, 722);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel optionPanel;
        private ListViewEx sysListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ListViewEx explorerListView;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
    }
}
