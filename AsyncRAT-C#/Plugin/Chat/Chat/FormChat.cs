using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Plugin
{
    public partial class FormChat : Form
    {
        public FormChat()
        {
            InitializeComponent();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                richTextBox1.AppendText("Me: " + textBox1.Text + Environment.NewLine);
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "chat";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("WriteInput").AsString = Environment.UserName + ": " + textBox1.Text + Environment.NewLine;
                Connection.Send(msgpack.Encode2Bytes());
                textBox1.Clear();
            }
        }

        private void FormChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!Connection.IsConnected)
            {
                Packet.GetFormChat.Invoke((MethodInvoker)(() =>
                {
                    Packet.GetFormChat?.Close();
                    Packet.GetFormChat?.Dispose();
                }));
                Connection.Disconnected();
                GC.Collect();
            }
        }
    }
}
