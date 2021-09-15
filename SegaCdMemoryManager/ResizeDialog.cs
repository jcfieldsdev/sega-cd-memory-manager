using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SegaCdMemoryManager
{
    public partial class ResizeDialog : Form
    {
        private int _selection;

        public int Selection { get => _selection; set => _selection = value; }

        public ResizeDialog()
        {
            InitializeComponent();
        }

        private void ChangeRadioButton(object sender, EventArgs e)
        {
            var control = sender as RadioButton;
            int tag = Convert.ToInt32(control.Tag);

            _selection = tag;
        }

        private void ResizeDialog_Load(object sender, EventArgs e)
        {
            foreach (var control in tableLayoutPanel1.Controls.OfType<RadioButton>())
            {
                int tag = Convert.ToInt32(control.Tag);
                control.Checked = _selection == tag;
            }
        }
    }
}
