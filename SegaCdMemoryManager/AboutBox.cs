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
    public partial class AboutBox : Form
    {
        private bool _switch;

        private const string AppTitle = "Sega CD Memory Manager";
        private const string Copyright = "Written by J.C. Fields";
        private const string WebSiteUrl = "https://gitlab.com/jcfields/sega-cd-memory-manager";

        public AboutBox()
        {
            InitializeComponent();

            Text = $"About {AppTitle}";
            labelProductName.Text = AppTitle;
            labelCopyright.Text = Copyright;
            labelWebSiteUrl.Text = WebSiteUrl;
        }

        public void OpenWebSite()
        {
            System.Diagnostics.Process.Start(WebSiteUrl);
        }

        private void ClickVisitWebSiteButton(object sender, EventArgs e)
        {
            OpenWebSite();
        }

        private void ClickConsolePicture(object sender, EventArgs e)
        {
            logoPictureBox.Image = _switch ? Properties.Resources.image_model1 : Properties.Resources.image_model2;
            _switch = !_switch;
        }
    }
}
