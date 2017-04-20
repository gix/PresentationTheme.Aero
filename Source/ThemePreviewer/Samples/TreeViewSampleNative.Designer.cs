namespace ThemePreviewer.Samples
{
    using Controls;

    partial class TreeViewSampleNative
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
            this.explorerTreeView = new ThemePreviewer.Controls.TreeViewEx();
            this.sysTreeView = new ThemePreviewer.Controls.TreeViewEx();
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
            this.tableLayoutPanel1.Controls.Add(this.explorerTreeView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.sysTreeView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(721, 722);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // explorerTreeView
            // 
            this.explorerTreeView.AutoHorizontalScroll = false;
            this.explorerTreeView.DimmedCheckBoxes = false;
            this.explorerTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.explorerTreeView.DrawImageAsync = false;
            this.explorerTreeView.ExclusionCheckBoxes = false;
            this.explorerTreeView.FadeInOutExpandos = false;
            this.explorerTreeView.FullRowSelect = true;
            this.explorerTreeView.HideSelection = false;
            this.explorerTreeView.HotTracking = true;
            this.explorerTreeView.Location = new System.Drawing.Point(3, 364);
            this.explorerTreeView.MultiSelect = false;
            this.explorerTreeView.Name = "explorerTreeView";
            this.explorerTreeView.NoIndent = false;
            this.explorerTreeView.PartialCheckBoxes = false;
            this.explorerTreeView.RichTooltip = false;
            this.explorerTreeView.ShowLines = false;
            this.explorerTreeView.Size = new System.Drawing.Size(715, 355);
            this.explorerTreeView.TabIndex = 1;
            this.explorerTreeView.UseDoubleBuffering = false;
            this.explorerTreeView.UseExplorerStyle = true;
            // 
            // sysTreeView
            // 
            this.sysTreeView.AutoHorizontalScroll = false;
            this.sysTreeView.DimmedCheckBoxes = false;
            this.sysTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sysTreeView.DrawImageAsync = false;
            this.sysTreeView.ExclusionCheckBoxes = false;
            this.sysTreeView.FadeInOutExpandos = false;
            this.sysTreeView.FullRowSelect = true;
            this.sysTreeView.HideSelection = false;
            this.sysTreeView.Location = new System.Drawing.Point(3, 3);
            this.sysTreeView.MultiSelect = false;
            this.sysTreeView.Name = "sysTreeView";
            this.sysTreeView.NoIndent = false;
            this.sysTreeView.PartialCheckBoxes = false;
            this.sysTreeView.RichTooltip = false;
            this.sysTreeView.ShowLines = false;
            this.sysTreeView.Size = new System.Drawing.Size(715, 355);
            this.sysTreeView.TabIndex = 0;
            this.sysTreeView.UseDoubleBuffering = false;
            // 
            // TreeViewSampleNative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.optionPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TreeViewSampleNative";
            this.Size = new System.Drawing.Size(721, 722);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel optionPanel;
        private TreeViewEx sysTreeView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private TreeViewEx explorerTreeView;
    }
}
