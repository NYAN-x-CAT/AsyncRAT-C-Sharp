using System;
using System.Collections.Generic;
using System.Text;

namespace StreamLibrary.Encoders.GridCoder
{
    public class GridEncoder : IEncoder
    {
        public override ulong CachedSize
        {
            get;
            internal set;
        }

        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        public override int BufferCount
        {
            get { return 1; }
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.AutoDispose | CodecOption.RequireSameSize; }
        }



        public override unsafe void CodeImage(IntPtr Scan0, System.Drawing.Rectangle ScanArea, System.Drawing.Size ImageSize, System.Drawing.Imaging.PixelFormat Format, System.IO.Stream outStream)
        {



        }

        public override unsafe System.Drawing.Bitmap DecodeData(System.IO.Stream inStream)
        {
            return null;
        }

        public override unsafe System.Drawing.Bitmap DecodeData(IntPtr CodecBuffer, uint Length)
        {
            return null;
        }
    }
}