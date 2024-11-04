namespace CFFileSystemManager
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            tscbConnection = new ToolStripComboBox();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            splitContainer1 = new SplitContainer();
            tvwFolder = new TreeView();
            cmsFolder = new ContextMenuStrip(components);
            copyToLocalToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            cmsFolder.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, tscbConnection });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1156, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(72, 22);
            toolStripLabel1.Text = "Connection:";
            // 
            // tscbConnection
            // 
            tscbConnection.DropDownStyle = ComboBoxStyle.DropDownList;
            tscbConnection.Name = "tscbConnection";
            tscbConnection.Size = new Size(175, 25);
            tscbConnection.SelectedIndexChanged += tscbConnection_SelectedIndexChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 666);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1156, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(118, 17);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 25);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tvwFolder);
            splitContainer1.Size = new Size(1156, 641);
            splitContainer1.SplitterDistance = 385;
            splitContainer1.TabIndex = 2;
            // 
            // tvwFolder
            // 
            tvwFolder.Dock = DockStyle.Fill;
            tvwFolder.Location = new Point(0, 0);
            tvwFolder.Name = "tvwFolder";
            tvwFolder.Size = new Size(385, 641);
            tvwFolder.TabIndex = 0;
            tvwFolder.BeforeExpand += tvwFolder_BeforeExpand;
            tvwFolder.AfterSelect += tvwFolder_AfterSelect;
            // 
            // cmsFolder
            // 
            cmsFolder.Items.AddRange(new ToolStripItem[] { copyToLocalToolStripMenuItem });
            cmsFolder.Name = "cmsFolder";
            cmsFolder.Size = new Size(181, 48);
            // 
            // copyToLocalToolStripMenuItem
            // 
            copyToLocalToolStripMenuItem.Name = "copyToLocalToolStripMenuItem";
            copyToLocalToolStripMenuItem.Size = new Size(180, 22);
            copyToLocalToolStripMenuItem.Text = "Copy to local";
            copyToLocalToolStripMenuItem.Click += copyToLocalToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1156, 688);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Name = "MainForm";
            Text = "CF File System Manager";
            Load += MainForm_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            cmsFolder.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private StatusStrip statusStrip1;
        private SplitContainer splitContainer1;
        private TreeView tvwFolder;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox tscbConnection;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ContextMenuStrip cmsFolder;
        private ToolStripMenuItem copyToLocalToolStripMenuItem;
    }
}
