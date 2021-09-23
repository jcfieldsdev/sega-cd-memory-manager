/******************************************************************************
 * Sega CD Memory Manager                                                     *
 *                                                                            *
 * Copyright (C) 2021 J.C. Fields (jcfields@jcfields.dev).                    *
 *                                                                            *
 * This program is free software: you can redistribute it and/or modify it    *
 * under the terms of the GNU General Public License as published by the Free *
 * Software Foundation, either version 3 of the License, or (at your option)  *
 * any later version.                                                         *
 *                                                                            *
 * This program is distributed in the hope that it will be useful, but        *
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY *
 * or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License   *
 * for more details.                                                          *
 *                                                                            *
 * You should have received a copy of the GNU General Public License along    *
 * with this program.  If not, see <http://www.gnu.org/licenses/>.            *
 ******************************************************************************/

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

        private struct Editor
        {
            public ListView ListViewEntries { get; set; }
            public ToolStripButton ToolStripButtonNew { get; set; }
            public ToolStripButton ToolStripButtonOpen { get; set; }
            public ToolStripButton ToolStripButtonSave { get; set; }
            public ToolStripButton ToolStripButtonImport { get; set; }
            public ToolStripButton ToolStripButtonExport { get; set; }
            public ToolStripButton ToolStripButtonMove { get; set; }
            public ToolStripButton ToolStripButtonCopy { get; set; }
            public ToolStripButton ToolStripButtonDelete { get; set; }
            public TextBox TextBoxFileName { get; set; }
            public ToolStripStatusLabel ToolStripStatusLabelFilesUsed { get; set; }
            public ToolStripStatusLabel ToolStripStatusLabelBlocksFree { get; set; }
            public ToolStripStatusLabel ToolStripStatusLabelFileSize { get; set; }
            public ToolStripStatusLabel ToolStripStatusLabelModified { get; set; }
            public ToolStripMenuItem ToolStripMenuItemImport { get; set; }
            public ToolStripMenuItem ToolStripMenuItemExport { get; set; }
            public ToolStripMenuItem ToolStripMenuItemMoveUp { get; set; }
            public ToolStripMenuItem ToolStripMenuItemMoveDown { get; set; }
            public ToolStripMenuItem ToolStripMenuItemProtect { get; set; }
            public ToolStripMenuItem ToolStripMenuItemMove { get; set; }
            public ToolStripMenuItem ToolStripMenuItemCopy { get; set; }
            public ToolStripMenuItem ToolStripMenuItemRename { get; set; }
            public ToolStripMenuItem ToolStripMenuItemDelete { get; set; }
            public ToolStripMenuItem ToolStripMenuItemResize { get; set; }
        }

        private const string AppTitle = "Sega CD Memory Manager";
        private const string FileFilter = "Sega CD memory file|*.brm;*crm";
        private const string EntryFilter = "Sega CD save entry file|*.srm";

        private readonly BramFile[] _bramFiles;
        private readonly Editor[] _editors;
        private readonly bool[] _isModified;

        public MainForm(string[] args)
        {
            InitializeComponent();

            _bramFiles = new BramFile[(int)File.Count];
            _editors = new Editor[(int)File.Count];
            _isModified = new bool[(int)File.Count];

            InitializeEditors();

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

        private void InitializeEditors()
        {
            _editors[(int)File.Left] = new Editor
            {
                ListViewEntries = listViewEntries1,
                ToolStripButtonNew = toolStripButtonNew1,
                ToolStripButtonOpen = toolStripButtonOpen1,
                ToolStripButtonSave = toolStripButtonSave1,
                ToolStripButtonImport = toolStripButtonImport1,
                ToolStripButtonExport = toolStripButtonExport1,
                ToolStripButtonMove = toolStripButtonMove1,
                ToolStripButtonCopy = toolStripButtonCopy1,
                ToolStripButtonDelete = toolStripButtonDelete1,
                TextBoxFileName = textBoxFileName1,
                ToolStripStatusLabelFilesUsed = toolStripStatusLabelFilesUsed1,
                ToolStripStatusLabelBlocksFree = toolStripStatusLabelBlocksFree1,
                ToolStripStatusLabelFileSize = toolStripStatusLabelFileSize1,
                ToolStripStatusLabelModified = toolStripStatusLabelModified1,
                ToolStripMenuItemImport = toolStripMenuItemImport1,
                ToolStripMenuItemExport = toolStripMenuItemExport1,
                ToolStripMenuItemMoveUp = toolStripMenuItemMoveUp1,
                ToolStripMenuItemMoveDown = toolStripMenuItemMoveDown1,
                ToolStripMenuItemProtect = toolStripMenuItemProtect1,
                ToolStripMenuItemMove = toolStripMenuItemMove1,
                ToolStripMenuItemCopy = toolStripMenuItemCopy1,
                ToolStripMenuItemRename = toolStripMenuItemRename1,
                ToolStripMenuItemDelete = toolStripMenuItemDelete1,
                ToolStripMenuItemResize = toolStripMenuItemResize1
            };

            _editors[(int)File.Right] = new Editor
            {
                ListViewEntries = listViewEntries2,
                ToolStripButtonNew = toolStripButtonNew2,
                ToolStripButtonOpen = toolStripButtonOpen2,
                ToolStripButtonSave = toolStripButtonSave2,
                ToolStripButtonImport = toolStripButtonImport2,
                ToolStripButtonExport = toolStripButtonExport2,
                ToolStripButtonMove = toolStripButtonMove2,
                ToolStripButtonCopy = toolStripButtonCopy2,
                ToolStripButtonDelete = toolStripButtonDelete2,
                TextBoxFileName = textBoxFileName2,
                ToolStripStatusLabelFilesUsed = toolStripStatusLabelFilesUsed2,
                ToolStripStatusLabelBlocksFree = toolStripStatusLabelBlocksFree2,
                ToolStripStatusLabelFileSize = toolStripStatusLabelFileSize2,
                ToolStripStatusLabelModified = toolStripStatusLabelModified2,
                ToolStripMenuItemImport = toolStripMenuItemImport2,
                ToolStripMenuItemExport = toolStripMenuItemExport2,
                ToolStripMenuItemMoveUp = toolStripMenuItemMoveUp2,
                ToolStripMenuItemMoveDown = toolStripMenuItemMoveDown2,
                ToolStripMenuItemProtect = toolStripMenuItemProtect2,
                ToolStripMenuItemMove = toolStripMenuItemMove2,
                ToolStripMenuItemCopy = toolStripMenuItemCopy2,
                ToolStripMenuItemRename = toolStripMenuItemRename2,
                ToolStripMenuItemDelete = toolStripMenuItemDelete2,
                ToolStripMenuItemResize = toolStripMenuItemResize2
            };
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
            var editor = _editors[id];
            editor.ToolStripButtonSave.Enabled = isModified;
            editor.ToolStripStatusLabelModified.Text = isModified ? "Modified" : "";

            _isModified[id] = isModified;
        }

        private void Reload(int id)
        {
            var bramFile = _bramFiles[id];

            if (bramFile == null)
            {
                return;
            }

            var editor = _editors[id];
            editor.ListViewEntries.Items.Clear();

            foreach (var entry in bramFile.Entries)
            {
                var row = new string[] { entry.Name, entry.SizeInBlocks.ToString() };
                var listViewItem = new ListViewItem(row)
                {
                    Tag = entry
                };
                editor.ListViewEntries.Items.Add(listViewItem);
            }

            editor.ToolStripButtonSave.Enabled = _isModified[id];
            editor.TextBoxFileName.Text = Path.GetFileName(bramFile.Path);

            var stringFormat = "{0:n0} {1}";
            editor.ToolStripStatusLabelFilesUsed.Text = String.Format(
                stringFormat,
                bramFile.FilesUsed,
                bramFile.FilesUsed == 1 ? "file" : "files"
            );
            editor.ToolStripStatusLabelBlocksFree.Text = String.Format(
                stringFormat,
                bramFile.BlocksFree,
                bramFile.BlocksFree == 1 ? "block free" : "blocks free"
            );
            editor.ToolStripStatusLabelFileSize.Text = String.Format(
                stringFormat,
                bramFile.SizeInBytes,
                bramFile.BlocksFree == 1 ? "bytes" : "bytes"
            );

            UpdateEntryButtons(id);
        }

        private void UpdateEntryButtons(int id)
        {
            var editor = _editors[id];

            // enables items if entry selected
            bool state = editor.ListViewEntries.SelectedIndices.Count > 0;
            editor.ToolStripButtonExport.Enabled = state;
            editor.ToolStripButtonMove.Enabled = state;
            editor.ToolStripButtonCopy.Enabled = state;
            editor.ToolStripButtonDelete.Enabled = state;
            editor.ToolStripMenuItemExport.Enabled = state;
            editor.ToolStripMenuItemProtect.Enabled = state;
            editor.ToolStripMenuItemMove.Enabled = state;
            editor.ToolStripMenuItemCopy.Enabled = state;
            editor.ToolStripMenuItemRename.Enabled = state;
            editor.ToolStripMenuItemDelete.Enabled = state;
            
            // enables items if not at end of list
            var selectedIndices = editor.ListViewEntries.SelectedIndices;
            int totalItems = editor.ListViewEntries.Items.Count - 1;
            editor.ToolStripMenuItemMoveUp.Enabled = state && selectedIndices[0] > 0;
            editor.ToolStripMenuItemMoveDown.Enabled = state && selectedIndices[selectedIndices.Count - 1] < totalItems;

            // checks item if entry is protected
            var selectedItems = editor.ListViewEntries.SelectedItems;
            var entries = selectedItems.Cast<ListViewItem>().Select(listViewItem => listViewItem.Tag as SaveEntry).ToList();
            var allProtected = entries.All(entry => entry.Protect);
            var anyProtected = entries.Any(entry => entry.Protect);
            editor.ToolStripMenuItemProtect.Checked = anyProtected;
            editor.ToolStripMenuItemProtect.CheckState = !anyProtected ? CheckState.Unchecked :
                                                          allProtected ? CheckState.Checked :
                                                          CheckState.Indeterminate;
        }
    
        private void CopyEntry(int sourceId)
        {
            int destinationId = sourceId == (int)File.Left ? (int)File.Right : (int)File.Left;
            var destinationBramFile = _bramFiles[destinationId];

            int modified = 0;

            foreach (ListViewItem listViewItem in _editors[sourceId].ListViewEntries.SelectedItems)
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

            int sourceModified = 0, destinationModified = 0;

            foreach (ListViewItem listViewItem in _editors[sourceId].ListViewEntries.SelectedItems)
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
            var bramFile = _bramFiles[id];
            int modified = 0;

            foreach (ListViewItem listViewItem in _editors[id].ListViewEntries.SelectedItems)
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
                            bramFile.RenameEntry(entry, renameDialog.EntryName);
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

        private void MoveUpEntry(int id)
        {
            var bramFile = _bramFiles[id];
            var selectedIndices = new List<int>();
            int modified = 0;

            foreach (int index in _editors[id].ListViewEntries.SelectedIndices)
            {
                try
                {
                    bramFile.MoveUpEntry(index);
                    selectedIndices.Add(Math.Max(0, index - 1));
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

            // re-selects rows after reloading listview
            foreach (int index in selectedIndices)
            {
                _editors[id].ListViewEntries.Items[index].Selected = true;
            }
        }

        private void MoveDownEntry(int id)
        {
            var bramFile = _bramFiles[id];
            var selectedIndices = new List<int>();
            int modified = 0;

            int count = _editors[id].ListViewEntries.SelectedIndices.Count;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    int index = count - i - 1;
                    bramFile.MoveDownEntry(index);
                    selectedIndices.Add(Math.Min(bramFile.FilesUsed - 1, index + 1));
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

            // re-selects rows after reloading listview
            foreach (int index in selectedIndices)
            {
                _editors[id].ListViewEntries.Items[index].Selected = true;
            }
        }

        private void DeleteEntry(int id)
        {
            var bramFile = _bramFiles[id];
            int modified = 0;

            foreach (ListViewItem listViewItem in _editors[id].ListViewEntries.SelectedItems)
            {
                try
                {
                    var entry = listViewItem.Tag as SaveEntry;
                    var dialogResult = MessageBox.Show(
                        $"Are you sure you want to delete the entry {entry.Name}?",
                        AppTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (dialogResult == DialogResult.Yes)
                    {
                        bramFile.RemoveEntry(entry);
                        modified++;
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
            foreach (ListViewItem listViewItem in _editors[id].ListViewEntries.SelectedItems)
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

        private void ToggleProtect(int id)
        {
            var bramFile = _bramFiles[id];
            int modified = 0;

            foreach (ListViewItem listViewItem in _editors[id].ListViewEntries.SelectedItems)
            {
                try
                {
                    var entry = listViewItem.Tag as SaveEntry;
                    bramFile.ProtectEntry(entry);
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

        private void ClickProtectButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            ToggleProtect(tag);
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

        private void ClickMoveUpButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            MoveUpEntry(tag);
        }

        private void ClickMoveDownButton(object sender, EventArgs e)
        {
            var control = sender as ToolStripItem;
            int tag = Convert.ToInt32(control.Tag);

            MoveDownEntry(tag);
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
