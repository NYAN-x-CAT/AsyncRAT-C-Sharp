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
using System.Threading;

namespace AsyncRAT_Sharp.Forms
{
    public partial class FormDownloadFile : Form
    {
        public FormDownloadFile()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients C { get; set; }
        public long dSize = 0;
        private async void timer1_Tick(object sender, EventArgs e)
        {
            labelsize.Text = $"{Methods.BytesToString(dSize)} \\ {Methods.BytesToString(C.BytesRecevied)}";
            if (C.BytesRecevied > dSize)
            {
                labelsize.Text = "Downloaded";
                labelsize.ForeColor = Color.Green;
                timer1.Stop();
                await Task.Delay(1500);
                this.Close();

            }
        }

        private void SocketDownload_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (C != null) C.Disconnected();
        }
    }
}
