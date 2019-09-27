using System;
using System.Windows.Forms;
using System.Drawing;

namespace Server.Handle_Packet {
    public class HandleLogs
    {
        public static HandleLogs Instance => new HandleLogs();

        public void Addmsg(string Msg, Color color)
        {
            try
            {
                ListViewItem LV = new ListViewItem();
                LV.Text = DateTime.Now.ToLongTimeString();
                LV.SubItems.Add(Msg);
                LV.ForeColor = color;
                lock (Settings.LockListviewLogs)
                {
                    Program.form1.listView2.Items.Insert(0, LV);
                }
            }
            catch { }
        }
    }
}