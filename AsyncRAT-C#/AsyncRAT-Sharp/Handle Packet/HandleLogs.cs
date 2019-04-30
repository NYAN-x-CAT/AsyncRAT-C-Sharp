using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleLogs
    {
        public void Addmsg(string Msg, Color color)
        {
            if (Program.form1.listView2.InvokeRequired)
            {
                Program.form1.listView2.BeginInvoke((MethodInvoker)(() =>
                {
                    ListViewItem LV = new ListViewItem();
                    LV.Text = DateTime.Now.ToLongTimeString();
                    LV.SubItems.Add(Msg);
                    LV.ForeColor = color;
                    Program.form1.listView2.BeginUpdate();
                    Program.form1.listView2.Items.Insert(0, LV);
                    Program.form1.listView2.EndUpdate();
                }));
            }
        }
    }
}