namespace WindowsFormsApplication1
{
    using ThemeTestApp.Samples;

    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.progressBar3 = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.listView1 = new System.Windows.Forms.ListView();
            this.componentTabControl = new System.Windows.Forms.TabControl();
            this.listViewPage = new System.Windows.Forms.TabPage();
            this.sysListViewSample1 = new SysListViewSample();
            this.treeViewPage = new System.Windows.Forms.TabPage();
            this.sysTreeViewSample1 = new SysTreeViewSample();
            this.progressBarPage = new System.Windows.Forms.TabPage();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.groupBox1.SuspendLayout();
            this.componentTabControl.SuspendLayout();
            this.listViewPage.SuspendLayout();
            this.treeViewPage.SuspendLayout();
            this.progressBarPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.progressBar3);
            this.groupBox1.Controls.Add(this.progressBar2);
            this.groupBox1.Controls.Add(this.progressBar1);
            this.groupBox1.Controls.Add(this.listView1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(186, 68);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // progressBar3
            // 
            this.progressBar3.Location = new System.Drawing.Point(19, 102);
            this.progressBar3.Name = "progressBar3";
            this.progressBar3.Size = new System.Drawing.Size(267, 27);
            this.progressBar3.TabIndex = 5;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(19, 67);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(267, 27);
            this.progressBar2.TabIndex = 4;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(19, 33);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(267, 27);
            this.progressBar1.TabIndex = 3;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Location = new System.Drawing.Point(7, 22);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(2338, 1579);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // componentTabControl
            // 
            this.componentTabControl.Controls.Add(this.listViewPage);
            this.componentTabControl.Controls.Add(this.treeViewPage);
            this.componentTabControl.Controls.Add(this.progressBarPage);
            this.componentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.componentTabControl.Location = new System.Drawing.Point(0, 24);
            this.componentTabControl.Name = "componentTabControl";
            this.componentTabControl.SelectedIndex = 0;
            this.componentTabControl.Size = new System.Drawing.Size(922, 631);
            this.componentTabControl.TabIndex = 6;
            // 
            // listViewPage
            // 
            this.listViewPage.Controls.Add(this.sysListViewSample1);
            this.listViewPage.Location = new System.Drawing.Point(4, 24);
            this.listViewPage.Name = "listViewPage";
            this.listViewPage.Size = new System.Drawing.Size(914, 603);
            this.listViewPage.TabIndex = 2;
            this.listViewPage.Text = "ListView";
            this.listViewPage.UseVisualStyleBackColor = true;
            // 
            // sysListViewSample1
            // 
            this.sysListViewSample1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sysListViewSample1.Location = new System.Drawing.Point(0, 0);
            this.sysListViewSample1.Name = "sysListViewSample1";
            this.sysListViewSample1.Size = new System.Drawing.Size(914, 603);
            this.sysListViewSample1.TabIndex = 1;
            // 
            // treeViewPage
            // 
            this.treeViewPage.Controls.Add(this.sysTreeViewSample1);
            this.treeViewPage.Location = new System.Drawing.Point(4, 24);
            this.treeViewPage.Name = "treeViewPage";
            this.treeViewPage.Padding = new System.Windows.Forms.Padding(3);
            this.treeViewPage.Size = new System.Drawing.Size(914, 600);
            this.treeViewPage.TabIndex = 1;
            this.treeViewPage.Text = "TreeView";
            this.treeViewPage.UseVisualStyleBackColor = true;
            // 
            // sysTreeViewSample1
            // 
            this.sysTreeViewSample1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sysTreeViewSample1.Location = new System.Drawing.Point(3, 3);
            this.sysTreeViewSample1.Name = "sysTreeViewSample1";
            this.sysTreeViewSample1.Size = new System.Drawing.Size(186, 68);
            this.sysTreeViewSample1.TabIndex = 1;
            // 
            // progressBarPage
            // 
            this.progressBarPage.Controls.Add(this.groupBox1);
            this.progressBarPage.Location = new System.Drawing.Point(4, 24);
            this.progressBarPage.Name = "progressBarPage";
            this.progressBarPage.Padding = new System.Windows.Forms.Padding(3);
            this.progressBarPage.Size = new System.Drawing.Size(914, 600);
            this.progressBarPage.TabIndex = 0;
            this.progressBarPage.Text = "ProgressBar";
            this.progressBarPage.UseVisualStyleBackColor = true;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 23);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "&Open";
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem1.Text = "E&xit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 23);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(922, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 23);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 655);
            this.Controls.Add(this.componentTabControl);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.componentTabControl.ResumeLayout(false);
            this.listViewPage.ResumeLayout(false);
            this.treeViewPage.ResumeLayout(false);
            this.progressBarPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ProgressBar progressBar3;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TabControl componentTabControl;
        private System.Windows.Forms.TabPage progressBarPage;
        private System.Windows.Forms.TabPage treeViewPage;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private SysListViewSample sysListViewSample1;
        private SysTreeViewSample sysTreeViewSample1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.TabPage listViewPage;
    }
}

