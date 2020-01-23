namespace ThemePreviewer.Samples
{
    partial class DataGridSampleNative
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
            this.components = new System.ComponentModel.Container();
            this.grid = new System.Windows.Forms.DataGridView();
            this.enabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastChangedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.AllowUserToOrderColumns = true;
            this.grid.AutoGenerateColumns = false;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.enabledColumn,
            this.nameColumn,
            this.typeColumn,
            this.sizeColumn,
            this.lastChangedColumn});
            this.grid.DataSource = this.itemsBindingSource;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Name = "grid";
            this.grid.RowHeadersWidth = 25;
            this.grid.Size = new System.Drawing.Size(647, 568);
            this.grid.TabIndex = 0;
            // 
            // enabledColumn
            // 
            this.enabledColumn.DataPropertyName = "Enabled";
            this.enabledColumn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.enabledColumn.HeaderText = "";
            this.enabledColumn.Name = "enabledColumn";
            this.enabledColumn.Width = 20;
            // 
            // nameColumn
            // 
            this.nameColumn.DataPropertyName = "Name";
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.Name = "nameColumn";
            this.nameColumn.Width = 75;
            // 
            // typeColumn
            // 
            this.typeColumn.DataPropertyName = "Type";
            this.typeColumn.HeaderText = "Type";
            this.typeColumn.Name = "typeColumn";
            this.typeColumn.Width = 50;
            // 
            // sizeColumn
            // 
            this.sizeColumn.DataPropertyName = "Size";
            this.sizeColumn.HeaderText = "Size";
            this.sizeColumn.Name = "sizeColumn";
            this.sizeColumn.Width = 75;
            // 
            // lastChangedColumn
            // 
            this.lastChangedColumn.DataPropertyName = "LastChanged";
            this.lastChangedColumn.HeaderText = "LastChanged";
            this.lastChangedColumn.Name = "lastChangedColumn";
            this.lastChangedColumn.Width = 200;
            // 
            // itemsBindingSource
            // 
            this.itemsBindingSource.DataSource = typeof(ThemePreviewer.ItemsCollection);
            // 
            // DataGridSampleNative
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grid);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DataGridSampleNative";
            this.Size = new System.Drawing.Size(647, 568);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemsBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.BindingSource itemsBindingSource;
        private System.Windows.Forms.DataGridViewCheckBoxColumn enabledColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sizeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastChangedColumn;
    }
}
