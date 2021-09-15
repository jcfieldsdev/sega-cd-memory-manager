using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SegaCdMemoryManager
{
    public partial class RenameDialog : Form
    {
        private string _entryName;

        public string EntryName { get => _entryName; set => _entryName = value; }

        public RenameDialog()
        {
            InitializeComponent();
        }

        private void ChangeNameText(object sender, EventArgs e)
        {
            var control = sender as TextBox;
            // forces case and removes invalid characters
            string name = Regex.Replace(control.Text.ToUpper(), @"[^A-Z0-9_]", "");

            int cursorPosition = control.SelectionStart;
            control.Text = name;
            control.SelectionStart = cursorPosition;

            _entryName = textBoxName.Text;
        }

        private void RenameDialog_Load(object sender, EventArgs e)
        {
            textBoxName.Text = _entryName;

            ActiveControl = textBoxName;
            textBoxName.SelectAll();
        }
    }
}
