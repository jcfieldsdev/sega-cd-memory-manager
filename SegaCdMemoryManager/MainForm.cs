using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SegaCdMemoryManager
{
    public partial class MainForm : Form
    {
        enum File
        {
            Count = 2,
            Left = 0,
            Right = 1
        }

        private const string AppTitle = "Sega CD Memory Manager";
        private const string FileFilter = "Sega CD memory file|*.brm;*crm";
        private const string EntryFilter = "Sega CD save entry file|*.srm";

        private readonly BramFile[] _bramFiles;
        private readonly bool[] _isModified;

        public MainForm(string[] args)
        {
            InitializeComponent();

            _bramFiles = new BramFile[(int)File.Count];
            _isModified = new bool[(int)File.Count];

            if (args.Length > 1)
            {
                LoadFile((int)File.Right, args[1]);
            } 
            else if (args.Length > 0)
            {
                LoadFile((int)File.Left, args[0]);
            }
            else
            {
                LoadBlankFile((int)File.Left);
                LoadBlankFile((int)File.Right);
            }
        }

        private void OpenFile(int id)
        {
            using (var openFileDialog = new OpenFileDialog
            {
                Filter = FileFilter
            })
            {
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    LoadFile(id, openFileDialog.FileName);
                }
            }
        }

        private void LoadFile(int id, string path)
        {
            try
            {
                int otherId = id == (int)File.Left ? (int)File.Right : (int)File.Left;

                if (_bramFiles[otherId].Path == path)
                {
                    throw new Exception("Cannot open two instances of the same file.");
                }

                _bramFiles[id] = new BramFile(path);

                SetModified(id, false);
                Reload(id);
            }
            catch (Exception error)
            {
                MessageBox.Show(
                    error.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadBlankFile(int id)
        {
            _bramFiles[id] = new BramFile();

            SetModified(id, false);
            Reload(id);
        }

        private void Save(int id)
        {
            var bramFile = _bramFiles[id];

            if (bramFile.Path == "")
            {
                SaveAs(id);
            }
            else
            {
                try
                {
                    bramFile.WriteFile();

                    SetModified(id, false);
                    Reload(id);
                }
                catch (Exception error)
                {
                    MessageBox.Show(
                        error.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void SaveAs(int id)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog
                {
                    Filter = FileFilter
                })
                {
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var bramFile = _bramFiles[id];
                        bramFile.Path = saveFileDialog.FileName;
                        bramFile.WriteFile();

                        SetModified(id, false);
                        Reload(id);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(
                    error.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void CloseFile(int id)
        {
            if (_isModified[id]) // prompts for save if modified
            {
                var bramFile = _bramFiles[id];
                string fileName = bramFile.Path == "" ? "Untitled" : Path.GetFileName(bramFile.Path);
                var dialogResult = MessageBox.Show(
                    $"Do you want to save changes to {fileName}?",
                    AppTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (dialogResult == DialogResult.Yes)
                {
                    Save(id);
                }
            }
        }

        private void SetModified(int id, bool isModified)
        {
            ToolStripButton toolStripButtonSave = null;
            ToolStripStatusLabel toolStripStatusLabelModified = null;

            if (id == (int)File.Left)
            {
                toolStripButtonSave = toolStripButtonSave1;
                toolStripStatusLabelModified = toolStripStatusLabelModified1;

            }
            else if (id == (int)File.Right)
            {
                toolStripButtonSave = toolStripButtonSave2;
                toolStripStatusLabelModified = toolStripStatusLabelModified2;
            }

            toolStripButtonSave.Enabled = isModified;
            toolStripStatusLabelModified.Text = isModified ? "Modified" : "";

            _isModified[id] = isModified;
        }

        private void Reload(int id)
        {
            var bramFile = _bramFiles[id];

            if (bramFile == null)
            {
                return;
            }

            ListView listViewEntries = null;
            ToolStripButton toolStripButtonSave = null;
            TextBox textBoxFileName = null;
            ToolStripStatusLabel toolStripStatusLabelFilesUsed = null;
            ToolStripStatusLabel toolStripStatusLabelBlocksFree = null;
            ToolStripStatusLabel toolStripStatusLabelFileSize = null;

            if (id == (int)File.Left)
            {
                listViewEntries = listViewEntries1;
                toolStripButtonSave = toolStripButtonSave1;
                textBoxFileName = textBoxFileName1;
                toolStripStatusLabelFilesUsed = toolStripStatusLabelFilesUsed1;
                toolStripStatusLabelBlocksFree = toolStripStatusLabelBlocksFree1;
                toolStripStatusLabelFileSize = toolStripStatusLabelFileSize1;
            }
            else if (id == (int)File.Right)
            {
                listViewEntries = listViewEntries2;
                toolStripButtonSave = toolStripButtonSave2;
                textBoxFileName = textBoxFileName2;
                toolStripStatusLabelFilesUsed = toolStripStatusLabelFilesUsed2;
                toolStripStatusLabelBlocksFree = toolStripStatusLabelBlocksFree2;
                toolStripStatusLabelFileSize = toolStripStatusLabelFileSize2;
            }

            listViewEntries.Items.Clear();

            foreach (var entry in bramFile.Entries)
            {
                var row = new string[] { entry.Name, entry.SizeInBlocks.ToString() };
                var listViewItem = new ListViewItem(row)
                {
                    Tag = entry
                };
                listViewEntries.Items.Add(listViewItem);
            }

            toolStripButtonSave.Enabled = _isModified[id];
            textBoxFileName.Text = Path.GetFileName(bramFile.Path);
            toolStripStatusLabelFilesUsed.Text = $"{bramFile.FilesUsed:n0} files";
            toolStripStatusLabelBlocksFree.Text = $"{bramFile.BlocksFree:n0} blocks free";
            toolStripStatusLabelFileSize.Text = $"{bramFile.SizeInBytes:n0} bytes";

            UpdateEntryButtons(id);
        }

        private void UpdateEntryButtons(int id)
        {
            ListView listViewEntries = null;
            ToolStripButton toolStripButtonExport = null;
            ToolStripButton toolStripButtonMove = null;
            ToolStripButton toolStripButtonCopy = null;
            ToolStripButton toolStripButtonDelete = null;
            ToolStripMenuItem toolStripMenuItemExport = null;
            ToolStripMenuItem toolStripMenuItemMove = null;
            ToolStripMenuItem toolStripMenuItemCopy = null;
            ToolStripMenuItem toolStripMenuItemRename = null;
            ToolStripMenuItem toolStripMenuItemDelete = null;

            if (id == (int)File.Left)
            {
                listViewEntries = listViewEntries1;
                toolStripButtonExport = toolStripButtonExport1;
                toolStripButtonMove = toolStripButtonMove1;
                toolStripButtonCopy = toolStripButtonCopy1;
                toolStripButtonDelete = toolStripButtonDelete1;
                toolStripMenuItemExport = toolStripMenuItemExport1;
                toolStripMenuItemMove = toolStripMenuItemMove1;
                toolStripMenuItemCopy = toolStripMenuItemCopy1;
                toolStripMenuItemRename = toolStripMenuItemRename1;
                toolStripMenuItemDelete = toolStripMenuItemDelete1;

            }
            else if (id == (int)File.Right)
            {
                listViewEntries = listViewEntries2;
                toolStripButtonExport = toolStripButtonExport2;
                toolStripButtonMove = toolStripButtonMove2;
                toolStripButtonCopy = toolStripButtonCopy2;
                toolStripButtonDelete = toolStripButtonDelete2;
                toolStripMenuItemExport = toolStripMenuItemExport2;
                toolStripMenuItemMove = toolStripMenuItemMove2;
                toolStripMenuItemCopy = toolStripMenuItemCopy2;
                toolStripMenuItemRename = toolStripMenuItemRename2;
                toolStripMenuItemDelete = toolStripMenuItemDelete2;
            }

            bool state = listViewEntries.SelectedIndices.Count > 0;
            toolStripButtonExport.Enabled = state;
            toolStripButtonMove.Enabled = state;
            toolStripButtonCopy.Enabled = state;
            toolStripButtonDelete.Enabled = state;
            toolStripMenuItemExport.Enabled = state;
            toolStripMenuItemMove.Enabled = state;
            toolStripMenuItemCopy.Enabled = state;
            toolStripMenuItemRename.Enabled = state;
            toolStripMenuItemDelete.Enabled = state;
        }

        private void CopyEntry(int sourceId)
        {
            int destinationId = sourceId == (int)File.Left ? (int)File.Right : (int)File.Left;
            var destinationBramFile = _bramFiles[destinationId];

            ListView listViewEntries = null;

            if (sourceId == (int)File.Left)
            {
                listViewEntries = listViewEntries1;

            }
            else if (sourceId == (int)File.Right)
            {
                listViewEntries = listViewEntries2;
            }

            int modified = 0;

            foreach (ListViewItem listViewItem in listViewEntries.SelectedItems)
            {
                try
                {
                    var entry = listViewItem.Tag as SaveEntry;
                    destinationBramFile.AddEntry(entry);
                    modified++;
                }
                catch (Exception error)
                {
                    MessageBox.Show(
                        error.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            SetModified(destinationId, modified > 0);
            Reload(destinationId);
        }

        private void MoveEntry(int sourceId)
        {
            var sourceBramFile = _bramFiles[sourceId];

            int destinationId = sourceId == (int)File.Left ? (int)File.Right : (int)File.Left;
            var destinationBramFile = _bramFiles[destinationId];

            ListView listViewEntries = null;

            if (sourceId == (int)File.Left)
            {
                listViewEntries = listViewEntries1;

            }
            else if (sourceId == (int)File.Right)
            {
                listViewEntries = listViewEntries2;
            }

            int sourceModified = 0, destinationModified = 0;

            foreach (ListViewItem listViewItem in listViewEntries.SelectedItems)
            {    
                try
                {
                    var entry = listViewItem.Tag as SaveEntry;

                    destinationBramFile.AddEntry(entry);
                    destinationModified++;

                    sourceBramFile.RemoveEntry(entry);
                    sourceModified++;
                }
                catch (Exception error)
                {
                    MessageBox.Show(
                        error.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            SetModified(sourceId, sourceModified > 0);
            Reload(sourceId);

            SetModified(destinationId, destinationModified > 0);
            Reload(destinationId);
        }

        private void RenameEntry(int id)
        {
            ListView listViewEntries = null;

            if (id == (int)File.Left)
            {
                listViewEntries = listViewEntries1;
            }
            else if (id == (int)File.Right)
            {
                listViewEntries = listViewEntries2;
            }

            int modified = 0;

            foreach (ListViewItem listViewItem in listViewEntries.SelectedItems)
            {
                try
                {
                    var entry = listViewItem.Tag as SaveEntry;
                    using (var renameDialog = new RenameDialog
                    {
                        EntryName = entry.Name
                    })
                    {
                        if (renameDialog.ShowDialog() == DialogResult.OK)
                        {
                            entry.Rename(renameDialog.EntryName);
                            modified++;
                        }
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(
                        error.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            SetModified(id, modified > 0);
            Reload(id);
        }

        private void DeleteEntry(int id)
        {
            var bramFile = _bramFiles[id];
            ListView listViewEntries = null;

            if (id == (int)File.Left)
            {
                listViewEntries = listViewEntries1;

            }
            else if (id == (int)File.Right)
            {
                listViewEntries = listViewEntries2;
            }

            int modified = 0;

            foreach (ListViewItem listViewItem in listViewEntries.SelectedItems)
            {
                try
                {
                    var entry = listViewItem.Tag as SaveEntry;
                    bramFile.RemoveEntry(entry);
                    modified++;
                }
                catch (Exception error)
                {
                    MessageBox.Show(
                        error.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            SetModified(id, modified > 0);
            Reload(id);
        }

        private void ImportEntry(int id)
        {
            try
            {
                var bramFile = _bramFiles[id];
                bool isModified = false;
                using (var openFileDialog = new OpenFileDialog
                {
                    Filter = EntryFilter
                })
                {
                    if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        bramFile.ImportEntry(openFileDialog.FileName);
                        isModified = true;
                    }
                }

                SetModified(id, isModified);
                Reload(id);
            }
            catch (Exception error)
            {
                MessageBox.Show(
                    error.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ExportEntry(int id)
        {
            ListView listViewEntries = null;

            if (id == (int)File.Left)
            {
                listViewEntries = listViewEntries1;

            }
            else if (id == (int)File.Right)
            {
                listViewEntries = listViewEntries2;
            }

            foreach (ListViewItem listViewItem in listViewEntries.SelectedItems)
            {
                try
                {
                    var entry = listViewItem.Tag as SaveEntry;
                    using (var saveFileDialog = new SaveFileDialog
                    {
                        Filter = EntryFilter,
                        FileName = entry.Name
                    })
                    {
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            entry.WriteFile(saveFileDialog.FileName);
                        }
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(
                        error.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            Reload(id);
        }

        private void ResizeFile(int id)
        {
            try
            {
                var bramFile = _bramFiles[id];
                bool isModified = false;
                using (var resizeDialog = new ResizeDialog
                {
                    Selection = (int)Math.Floor(Math.Log(bramFile.SizeInBytes / 1024, 2))
                })
                {
                    if (resizeDialog.ShowDialog() == DialogResult.OK)
                    {
                        bramFile.Resize(resizeDialog.Selection);
                        isModified = true;
                    }
                }

                SetModified(id, isModified);
                Reload(id);
            }
            catch (Exception error)
            {
                MessageBox.Show(
                    error.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ChangeListView(object sender, EventArgs e)
        {
            var control = sender as ListView;
            int tag = Convert.ToInt32(control.Tag);

            UpdateEntryButtons(tag);
        }

        private void ClickNewButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            CloseFile(tag);
            LoadBlankFile(tag);
        }

        private void ClickOpenButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            CloseFile(tag);
            OpenFile(tag);
        }

        private void ClickSaveButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            Save(tag);
        }

        private void ClickImportButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            ImportEntry(tag);
        }

        private void ClickExportButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            ExportEntry(tag);
        }

        private void ClickMoveButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            MoveEntry(tag);
        }

        private void ClickCopyButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            CopyEntry(tag);
        }

        private void ClickRenameButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            RenameEntry(tag);
        }

        private void ClickDeleteButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            DeleteEntry(tag);
        }

        private void ClickResizeButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            ResizeFile(tag);
        }

        private void ClickVisitWebSiteButton(object sender, EventArgs e)
        {
            var form = new AboutBox();
            form.OpenWebSite();
        }

        private void ClickAboutButton(object sender, EventArgs e)
        {
            var form = new AboutBox();
            form.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseFile((int)File.Left);
            CloseFile((int)File.Right);
        }
    }
}
