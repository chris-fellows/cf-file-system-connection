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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            tscbConnection = new ToolStripComboBox();
            toolStripLabel2 = new ToolStripLabel();
            tscbDrive = new ToolStripComboBox();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            splitContainer1 = new SplitContainer();
            tvwFolder = new TreeView();
            imageList1 = new ImageList(components);
            cmsFolder = new ContextMenuStrip(components);
            copyToLocalToolStripMenuItem = new ToolStripMenuItem();
            testCopyFileBySectionsToolStripMenuItem = new ToolStripMenuItem();
            copyLocalFileToToolStripMenuItem = new ToolStripMenuItem();
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
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, tscbConnection, toolStripLabel2, tscbDrive });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1245, 25);
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
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(37, 22);
            toolStripLabel2.Text = "Drive:";
            // 
            // tscbDrive
            // 
            tscbDrive.DropDownStyle = ComboBoxStyle.DropDownList;
            tscbDrive.Name = "tscbDrive";
            tscbDrive.Size = new Size(250, 25);
            tscbDrive.SelectedIndexChanged += tscbDrive_SelectedIndexChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 698);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1245, 22);
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
            splitContainer1.Size = new Size(1245, 673);
            splitContainer1.SplitterDistance = 414;
            splitContainer1.TabIndex = 2;
            // 
            // tvwFolder
            // 
            tvwFolder.Dock = DockStyle.Fill;
            tvwFolder.ImageIndex = 0;
            tvwFolder.ImageList = imageList1;
            tvwFolder.Location = new Point(0, 0);
            tvwFolder.Name = "tvwFolder";
            tvwFolder.SelectedImageIndex = 0;
            tvwFolder.Size = new Size(414, 673);
            tvwFolder.TabIndex = 0;
            tvwFolder.BeforeExpand += tvwFolder_BeforeExpand;
            tvwFolder.AfterSelect += tvwFolder_AfterSelect;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "folderdocuments.ico");
            // 
            // cmsFolder
            // 
            cmsFolder.Items.AddRange(new ToolStripItem[] { copyToLocalToolStripMenuItem, testCopyFileBySectionsToolStripMenuItem, copyLocalFileToToolStripMenuItem });
            cmsFolder.Name = "cmsFolder";
            cmsFolder.Size = new Size(205, 70);
            // 
            // copyToLocalToolStripMenuItem
            // 
            copyToLocalToolStripMenuItem.Name = "copyToLocalToolStripMenuItem";
            copyToLocalToolStripMenuItem.Size = new Size(204, 22);
            copyToLocalToolStripMenuItem.Text = "Copy to local";
            copyToLocalToolStripMenuItem.Click += copyToLocalToolStripMenuItem_Click;
            // 
            // testCopyFileBySectionsToolStripMenuItem
            // 
            testCopyFileBySectionsToolStripMenuItem.Name = "testCopyFileBySectionsToolStripMenuItem";
            testCopyFileBySectionsToolStripMenuItem.Size = new Size(204, 22);
            testCopyFileBySectionsToolStripMenuItem.Text = "Test copy file by sections";
            testCopyFileBySectionsToolStripMenuItem.Click += testCopyFileBySectionsToolStripMenuItem_Click;
            // 
            // copyLocalFileToToolStripMenuItem
            // 
            copyLocalFileToToolStripMenuItem.Name = "copyLocalFileToToolStripMenuItem";
            copyLocalFileToToolStripMenuItem.Size = new Size(204, 22);
            copyLocalFileToToolStripMenuItem.Text = "Copy local file to";
            copyLocalFileToToolStripMenuItem.Click += copyLocalFileToToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1245, 720);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Name = "MainForm";
            Text = "CF File System Manager";
            FormClosing += MainForm_FormClosing;
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
        private ToolStripLabel toolStripLabel2;
        private ToolStripComboBox tscbDrive;
        private ToolStripMenuItem testCopyFileBySectionsToolStripMenuItem;
        private ToolStripMenuItem copyLocalFileToToolStripMenuItem;
        private ImageList imageList1;
    }
}
