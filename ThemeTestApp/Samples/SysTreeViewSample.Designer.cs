namespace ThemeTestApp.Samples
{
    partial class SysTreeViewSample
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Properties");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("System.Core");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("References", new System.Windows.Forms.TreeNode[] {
            treeNode2});
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Styles.xaml");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Resources", new System.Windows.Forms.TreeNode[] {
            treeNode4});
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Program.cs");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("ThemeTestApp", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode3,
            treeNode5,
            treeNode6});
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("ThemeAppTest.Tests");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Solution \'ThemeTestApp\'", new System.Windows.Forms.TreeNode[] {
            treeNode7,
            treeNode8});
            this.optionPanel = new System.Windows.Forms.TableLayoutPanel();
            this.sysTreeView = new ThemeTestApp.TreeViewEx();
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
            this.sysTreeView.Location = new System.Drawing.Point(0, 0);
            this.sysTreeView.MultiSelect = false;
            this.sysTreeView.Name = "sysTreeView";
            treeNode1.Name = "Node3";
            treeNode1.Text = "Properties";
            treeNode2.Name = "Node5";
            treeNode2.Text = "System.Core";
            treeNode3.Name = "Node4";
            treeNode3.Text = "References";
            treeNode4.Name = "Node7";
            treeNode4.Text = "Styles.xaml";
            treeNode5.Name = "Node8";
            treeNode5.Text = "Resources";
            treeNode6.Name = "Node6";
            treeNode6.Text = "Program.cs";
            treeNode7.Name = "Node1";
            treeNode7.Text = "ThemeTestApp";
            treeNode8.Name = "Node2";
            treeNode8.Text = "ThemeAppTest.Tests";
            treeNode9.Name = "Node0";
            treeNode9.Text = "Solution \'ThemeTestApp\'";
            this.sysTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode9});
            this.sysTreeView.NoIndent = false;
            this.sysTreeView.PartialCheckBoxes = false;
            this.sysTreeView.RichTooltip = false;
            this.sysTreeView.ShowLines = false;
            this.sysTreeView.ShowRootLines = false;
            this.sysTreeView.Size = new System.Drawing.Size(618, 626);
            this.sysTreeView.TabIndex = 0;
            this.sysTreeView.UseDoubleBuffering = false;
            // 
            // SysTreeViewSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.sysTreeView);
            this.Controls.Add(this.optionPanel);
            this.Name = "SysTreeViewSample";
            this.Size = new System.Drawing.Size(618, 626);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel optionPanel;
        private ThemeTestApp.TreeViewEx sysTreeView;
    }
}
