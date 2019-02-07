using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace StreamLibrary.Codecs
{
    /// <summary>
    /// The M-JPG codec is not very efficient for networking as it is just a very simple codec
    /// </summary>
    public class MJPGCodec : IVideoCodec
    {
        public override event IVideoCodec.VideoCodeProgress onVideoStreamCoding;
        public override event IVideoCodec.VideoDecodeProgress onVideoStreamDecoding;
        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        public override ulong CachedSize
        {
            get { return 0; }
            internal set { }
        }

        public override int BufferCount
        {
            get { return 0; }
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.None; }
        }

        public MJPGCodec(int ImageQuality = 100)
            : base(ImageQuality)
        {

        }

        public override void CodeImage(Bitmap bitmap, Stream outStream)
        {
            lock (base.jpgCompression)
            {
                byte[] data = base.jpgCompression.Compress(bitmap);
                outStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                outStream.Write(data, 0, data.Length);
            }
        }

        public override Bitmap DecodeData(Stream inStream)
        {
            lock (base.jpgCompression)
            {
                if (!inStream.CanRead)
                    throw new Exception("Must have access to Read in the Stream");

                byte[] temp = new byte[4];
                inStream.Read(temp, 0, temp.Length);
                int DataLength = BitConverter.ToInt32(temp, 0);
                temp = new byte[DataLength];
                inStream.Read(temp, 0, temp.Length);
                return (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
            }
        }
    }
}