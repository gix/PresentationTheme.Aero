namespace ThemeTestApp.Samples
{
    using ThemeTestApp.Controls;

    partial class SysListViewSample
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
            this.sysListView = new ThemeTestApp.Controls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.optionPanel.Size = new System.Drawing.Size(0, 626);
            this.optionPanel.TabIndex = 1;
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
            this.sysListView.Location = new System.Drawing.Point(0, 0);
            this.sysListView.Name = "sysListView";
            this.sysListView.SimpleSelect = false;
            this.sysListView.Size = new System.Drawing.Size(618, 626);
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
            this.columnHeader2.Text = "Größe";
            this.columnHeader2.Width = 90;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Typ";
            this.columnHeader3.Width = 80;
            // 
            // SysListViewSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.sysListView);
            this.Controls.Add(this.optionPanel);
            this.Name = "SysListViewSample";
            this.Size = new System.Drawing.Size(618, 626);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel optionPanel;
        private ListViewEx sysListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}
