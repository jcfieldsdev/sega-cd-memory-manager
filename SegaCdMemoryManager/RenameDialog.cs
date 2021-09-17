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
