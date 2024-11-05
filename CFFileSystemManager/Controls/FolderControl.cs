using CFFileSystemConnection.Interfaces;
using CFFileSystemConnection.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CFFileSystemManager.Controls
{
    public partial class FolderControl : UserControl
    {
        private readonly IFileSystem _fileSystem;
        private readonly FolderObject _folderObject;

        public FolderControl()
        {
            InitializeComponent();
        }

        public FolderControl(IFileSystem fileSystem, FolderObject folderObject)
        {
            InitializeComponent();

            _fileSystem = fileSystem;

            _folderObject = folderObject;
            ModelToView(folderObject);
        }

        private void ModelToView(FolderObject folderObject)
        {
            lblPath.Text = folderObject.Path;

            dgvFile.Rows.Clear();
            dgvFile.Columns.Clear();

            int columnIndex = dgvFile.Columns.Add("Name", "Name");
            dgvFile.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFile.Columns.Add("Size", "Size");
            dgvFile.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFile.Columns.Add("Created", "Created");
            dgvFile.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFile.Columns.Add("Updated", "Updated");
            dgvFile.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFile.Columns.Add("Read Only", "Read Only");
            dgvFile.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFile.Columns.Add("Download", "Download");
            dgvFile.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            if (folderObject.Files != null)
            {
                foreach (var fileObject in folderObject.Files)
                {
                    using (DataGridViewRow row = new DataGridViewRow())
                    {
                        using (var cell = new DataGridViewTextBoxCell())
                        {
                            cell.Value = fileObject.Name;
                            cell.Tag = fileObject;
                            row.Cells.Add(cell);
                        }
                        using (var cell = new DataGridViewTextBoxCell())
                        {
                            cell.Value = fileObject.Length;
                            row.Cells.Add(cell);
                        }
                        using (var cell = new DataGridViewTextBoxCell())
                        {
                            cell.Value = fileObject.CreatedTimeUtc.ToString();
                            row.Cells.Add(cell);
                        }
                        using (var cell = new DataGridViewTextBoxCell())
                        {
                            cell.Value = fileObject.UpdatedTimeUtc == null ? "" : fileObject.UpdatedTimeUtc.ToString();
                            row.Cells.Add(cell);
                        }
                        using (var cell = new DataGridViewCheckBoxCell())
                        {
                            cell.Value = fileObject.ReadOnly;
                            row.Cells.Add(cell);
                        }
                        using (var cell = new DataGridViewButtonCell())
                        {
                            cell.Value = "Download";
                            row.Cells.Add(cell);
                        }
                        dgvFile.Rows.Add(row);
                    }
                }
            }

            dgvFile.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void dgvFile_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell)
            {
                FileObject fileObject = (FileObject)dgvFile.Rows[e.RowIndex].Cells[0].Tag;
                var localFile = Path.Combine(Path.GetTempPath(), "Downloads", Guid.NewGuid().ToString(), fileObject.Name);
                DownloadFile(fileObject, localFile);

                MessageBox.Show("File downloaded", "Download File");
            }
        }

        private void DownloadFile(FileObject fileObject, string localFile)
        {            
            try
            {


                using (var writer = new BinaryWriter(File.OpenWrite(localFile)))
                {
                    _fileSystem.GetFileContentBySection(fileObject.Path, 1024 * 500, (section, isMore) =>
                    {
                        writer.Write(section);
                        //lastIsMore = isMore;
                    });
                    writer.Flush();
                }
            }
            catch(Exception exception)
            {
                // Clean up partial file
                if (File.Exists(localFile)) File.Delete(localFile);
                throw;
            }

            //var content = _fileSystem.GetFileContent(fileObject.Path);
            //if (content != null)
            //{
            //    File.WriteAllBytes(localFile, content);
            //}
        }
    }
}
