namespace CFFileSystemManager.Controls
{
    partial class FolderControl
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
            if (disposing && (components != null))
            {
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
            dgvFile = new DataGridView();
            label1 = new Label();
            lblPath = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvFile).BeginInit();
            SuspendLayout();
            // 
            // dgvFile
            // 
            dgvFile.AllowUserToAddRows = false;
            dgvFile.AllowUserToDeleteRows = false;
            dgvFile.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvFile.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFile.Location = new Point(0, 26);
            dgvFile.MultiSelect = false;
            dgvFile.Name = "dgvFile";
            dgvFile.ReadOnly = true;
            dgvFile.RowHeadersVisible = false;
            dgvFile.Size = new Size(875, 492);
            dgvFile.TabIndex = 0;
            dgvFile.CellContentClick += dgvFile_CellContentClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 7);
            label1.Name = "label1";
            label1.Size = new Size(34, 15);
            label1.TabIndex = 1;
            label1.Text = "Path:";
            // 
            // lblPath
            // 
            lblPath.AutoSize = true;
            lblPath.Location = new Point(51, 8);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(0, 15);
            lblPath.TabIndex = 2;
            // 
            // FolderControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblPath);
            Controls.Add(label1);
            Controls.Add(dgvFile);
            Name = "FolderControl";
            Size = new Size(875, 521);
            ((System.ComponentModel.ISupportInitialize)dgvFile).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvFile;
        private Label label1;
        private Label lblPath;
    }
}
