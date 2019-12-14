using Server.Connection;
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
using System.IO;
using System.Net.Sockets;
using Timer = System.Threading.Timer;
using Server.Helper;
using Server.Algorithm;
using System.Diagnostics;

namespace Server.Forms
{
    public partial class FormDownloadFile : Form
    {
        public Form1 F { get; set; }
        internal Clients Client { get; set; }
        public long FileSize = 0;
        private long BytesSent = 0;
        public string FullFileName;
        public string ClientFullFileName;
        private bool IsUpload = false;
        public string DirPath;
        public FormDownloadFile()
        {
            InitializeComponent();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!IsUpload)
            {
                labelsize.Text = $"{Methods.BytesToString(FileSize)} \\ {Methods.BytesToString(Client.BytesRecevied)}";
                if (Client.BytesRecevied >= FileSize)
                {
                    labelsize.Text = "Downloaded";
                    labelsize.ForeColor = Color.Green;
                    timer1.Stop();
                }
            }
            else
            {
                labelsize.Text = $"{Methods.BytesToString(FileSize)} \\ {Methods.BytesToString(BytesSent)}";
                if (BytesSent >= FileSize)
                {
                    labelsize.Text = "Uploaded";
                    labelsize.ForeColor = Color.Green;
                    timer1.Stop();
                }
            }
        }

        private void SocketDownload_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Client?.Disconnected();
                timer1?.Dispose();
            }
            catch { }
        }

        public void Send(object obj)
        {
            lock (Client.SendSync)
            {
                try
                {
                    IsUpload = true;
                    byte[] msg = (byte[])obj;
                    byte[] buffersize = BitConverter.GetBytes(msg.Length);
                    Client.TcpClient.Poll(-1, SelectMode.SelectWrite);
                    Client.SslClient.Write(buffersize, 0, buffersize.Length);

                    Debug.WriteLine("send chunks");
                    using (MemoryStream memoryStream = new MemoryStream(msg))
                    {
                        int read = 0;
                        memoryStream.Position = 0;
                        byte[] chunk = new byte[50 * 1000];
                        while ((read = memoryStream.Read(chunk, 0, chunk.Length)) > 0)
                        {
                            Client.TcpClient.Poll(-1, SelectMode.SelectWrite);
                            Client.SslClient.Write(chunk, 0, read);
                            BytesSent += read;
                        }
                    }

                    Program.form1.BeginInvoke((MethodInvoker)(() =>
                    {
                        this.Close();
                    }));
                }
                catch
                {
                    Client?.Disconnected();
                    Program.form1.BeginInvoke((MethodInvoker)(() =>
                    {
                        labelsize.Text = "Error";
                        labelsize.ForeColor = Color.Red;
                    }));
                }
            }
        }
    }
}
