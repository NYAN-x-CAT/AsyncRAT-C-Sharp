using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormBlockClients : Form
    {
        public FormBlockClients()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                listBlocked.Items.Add(txtBlock.Text);
                txtBlock.Clear();
            }
            catch { }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = listBlocked.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    listBlocked.Items.RemoveAt(listBlocked.SelectedIndices[i]);
                }
            }
            catch { }
        }

        private void FormBlockClients_Load(object sender, EventArgs e)
        {
            try
            {
                listBlocked.Items.Clear();
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.txtBlocked))
                {
                    foreach (string client in Properties.Settings.Default.txtBlocked.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(client))
                        {
                            listBlocked.Items.Add(client);
                        }
                    }
                }
            }
            catch { }
        }

        private void FormBlockClients_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                lock (Settings.Blocked)
                {
                    Settings.Blocked.Clear();
                    List<string> clients = new List<string>();
                    foreach (string client in listBlocked.Items)
                    {
                        clients.Add(client);
                        Settings.Blocked.Add(client);
                    }
                    Properties.Settings.Default.txtBlocked = string.Join(",", clients);
                    Properties.Settings.Default.Save();
                }

            }
            catch { }
        }
    }
}
