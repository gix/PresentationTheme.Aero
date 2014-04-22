namespace ThemePreviewer.Samples
{
    partial class ComboBoxSampleNative
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
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.comboBoxEditable = new System.Windows.Forms.ComboBox();
            this.comboBoxEditableDisabled = new System.Windows.Forms.ComboBox();
            this.comboBoxDisabled = new System.Windows.Forms.ComboBox();
            this.comboBoxDisabledFlag = new System.Windows.Forms.CheckBox();
            this.comboBoxEditableDisabledFlag = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // comboBox
            // 
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Items.AddRange(new object[] {
            "Foo",
            "Documents and Settings",
            "ProgramData",
            "Recovery",
            "System Volume Information",
            "bootmgr",
            "hiberfil.sys",
            "pagefile.sys",
            "swapfile.sys"});
            this.comboBox.Location = new System.Drawing.Point(3, 3);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(140, 23);
            this.comboBox.TabIndex = 0;
            // 
            // comboBoxEditable
            // 
            this.comboBoxEditable.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxEditable.FormattingEnabled = true;
            this.comboBoxEditable.Items.AddRange(new object[] {
            "Documents and Settings",
            "ProgramData",
            "Recovery",
            "System Volume Information",
            "bootmgr",
            "hiberfil.sys",
            "pagefile.sys",
            "swapfile.sys"});
            this.comboBoxEditable.Location = new System.Drawing.Point(3, 61);
            this.comboBoxEditable.Name = "comboBoxEditable";
            this.comboBoxEditable.Size = new System.Drawing.Size(140, 23);
            this.comboBoxEditable.TabIndex = 3;
            this.comboBoxEditable.Text = "Foo";
            // 
            // comboBoxEditableDisabled
            // 
            this.comboBoxEditableDisabled.Enabled = false;
            this.comboBoxEditableDisabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxEditableDisabled.FormattingEnabled = true;
            this.comboBoxEditableDisabled.Items.AddRange(new object[] {
            "Documents and Settings",
            "ProgramData",
            "Recovery",
            "System Volume Information",
            "bootmgr",
            "hiberfil.sys",
            "pagefile.sys",
            "swapfile.sys"});
            this.comboBoxEditableDisabled.Location = new System.Drawing.Point(3, 90);
            this.comboBoxEditableDisabled.Name = "comboBoxEditableDisabled";
            this.comboBoxEditableDisabled.Size = new System.Drawing.Size(140, 23);
            this.comboBoxEditableDisabled.TabIndex = 4;
            this.comboBoxEditableDisabled.Text = "Foo";
            // 
            // comboBoxDisabled
            // 
            this.comboBoxDisabled.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDisabled.Enabled = false;
            this.comboBoxDisabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxDisabled.FormattingEnabled = true;
            this.comboBoxDisabled.Items.AddRange(new object[] {
            "Foo",
            "Documents and Settings",
            "ProgramData",
            "Recovery",
            "System Volume Information",
            "bootmgr",
            "hiberfil.sys",
            "pagefile.sys",
            "swapfile.sys"});
            this.comboBoxDisabled.Location = new System.Drawing.Point(3, 32);
            this.comboBoxDisabled.Name = "comboBoxDisabled";
            this.comboBoxDisabled.Size = new System.Drawing.Size(140, 23);
            this.comboBoxDisabled.TabIndex = 1;
            // 
            // comboBoxDisabledFlag
            // 
            this.comboBoxDisabledFlag.AutoSize = true;
            this.comboBoxDisabledFlag.Location = new System.Drawing.Point(149, 36);
            this.comboBoxDisabledFlag.Name = "comboBoxDisabledFlag";
            this.comboBoxDisabledFlag.Size = new System.Drawing.Size(15, 14);
            this.comboBoxDisabledFlag.TabIndex = 2;
            this.comboBoxDisabledFlag.UseVisualStyleBackColor = true;
            // 
            // comboBoxEditableDisabledFlag
            // 
            this.comboBoxEditableDisabledFlag.AutoSize = true;
            this.comboBoxEditableDisabledFlag.Location = new System.Drawing.Point(149, 94);
            this.comboBoxEditableDisabledFlag.Name = "comboBoxEditableDisabledFlag";
            this.comboBoxEditableDisabledFlag.Size = new System.Drawing.Size(15, 14);
            this.comboBoxEditableDisabledFlag.TabIndex = 5;
            this.comboBoxEditableDisabledFlag.UseVisualStyleBackColor = true;
            // 
            // ComboBoxSampleNative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxEditableDisabledFlag);
            this.Controls.Add(this.comboBoxDisabledFlag);
            this.Controls.Add(this.comboBoxDisabled);
            this.Controls.Add(this.comboBoxEditableDisabled);
            this.Controls.Add(this.comboBoxEditable);
            this.Controls.Add(this.comboBox);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ComboBoxSampleNative";
            this.Size = new System.Drawing.Size(490, 441);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox;
        private System.Windows.Forms.ComboBox comboBoxEditable;
        private System.Windows.Forms.ComboBox comboBoxEditableDisabled;
        private System.Windows.Forms.ComboBox comboBoxDisabled;
        private System.Windows.Forms.CheckBox comboBoxDisabledFlag;
        private System.Windows.Forms.CheckBox comboBoxEditableDisabledFlag;
    }
}
