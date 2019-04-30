using AsyncRAT_Sharp.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleProcessManager
    {
        public void GetProcess(Clients client, MsgPack unpack_msgpack)
        {
            if (Program.form1.InvokeRequired)
            {
                Program.form1.BeginInvoke((MethodInvoker)(() =>
                {
                    FormProcessManager PM = (FormProcessManager)Application.OpenForms["processManager:" + client.ID];
                    if (PM != null)
                    {
                        PM.listView1.Items.Clear();
                        string processLists = unpack_msgpack.ForcePathObject("Message").AsString;
                        string[] _NextProc = processLists.Split(new[] { "-=>" }, StringSplitOptions.None);
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
    }
}
