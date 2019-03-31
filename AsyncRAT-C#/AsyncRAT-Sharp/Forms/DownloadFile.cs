using AsyncRAT_Sharp.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Forms
{
    public partial class DownloadFile : Form
    {
        public DownloadFile()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients C { get; set; }
        public long Size = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
           labelsize.Text = $"{Methods.BytesToString(Size)} \\ {Methods.BytesToString(C.BytesRecevied)}";
            if (C.BytesRecevied > Size)
            {
                labelsize.Text = "Downloaded";
                labelsize.ForeColor = Color.Green;
                timer1.Stop();
            }
        }

        private void SocketDownload_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (C != null)
            {
                C.Disconnected();
            }
        }
    }
}
