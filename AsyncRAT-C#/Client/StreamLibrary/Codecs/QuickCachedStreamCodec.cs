using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary.Codecs
{
    public class QuickCachedStreamCodec : IVideoCodec
    {
        private Bitmap CodeTempBitmap;
        private Bitmap DecodeTempBitmap;

        public override event IVideoCodec.VideoCodeProgress onVideoStreamCoding;
        public override event IVideoCodec.VideoDecodeProgress onVideoStreamDecoding;
        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;
        public override ulong CachedSize
        {
            get;
            internal set;
        }

        public int MaxBuffers { get; private set; }
        private SortedList<int, byte[]> EncodeCache;
        private SortedList<int, byte[]> DecodeCache;


        /// <summary>
        /// Initialize a new object of QuickStreamCodec
        /// </summary>
        /// <param name="ImageQuality">The image quality 0-100%</param>
        public QuickCachedStreamCodec(int MaxBuffers = 5000, int ImageQuality = 100)
            : base(ImageQuality)
        {
            this.MaxBuffers = MaxBuffers;
            this.EncodeCache = new SortedList<int, byte[]>();
            this.DecodeCache = new SortedList<int, byte[]>();
        }

        public override int BufferCount
        {
            get { return 1; }
        }
        public override CodecOption CodecOptions
        {
            get { return CodecOption.RequireSameSize | CodecOption.AutoDispose; }
        }

        public override unsafe void CodeImage(Bitmap bitmap, Stream outStream)
        {
            if (!outStream.CanWrite)
                throw new Exception("Must have access to Write in the Stream");

            if (CodeTempBitmap != null)
            {
                if (CodeTempBitmap.Width != bitmap.Width || CodeTempBitmap.Height != bitmap.Height)
                    throw new Exception("Bitmap width/height are not equal to previous bitmap");
                if (bitmap.PixelFormat != CodeTempBitmap.PixelFormat)
                    throw new Exception("PixelFormat is not equal to previous Bitmap");
            }

            if (CodeTempBitmap == null)
            {
                byte[] temp = base.jpgCompression.Compress(bitmap);
                outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                outStream.Write(temp, 0, temp.Length);
                CodeTempBitmap = bitmap;
                return;
            }

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            BitmapData CodeBmpData = CodeTempBitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int Stride = Math.Abs(bmpData.Stride);

            List<Rectangle> Blocks = new List<Rectangle>();

            for (int y = 0, i = 0; y < bitmap.Height; y++, i += Stride)
            {
                if (onCodeDebugScan != null)
                    onCodeDebugScan(new Rectangle(0, y, bitmap.Width, 1));

                Rectangle ScanBlock = new Rectangle(0, y, bitmap.Width, 1);
                if (NativeMethods.memcmp(new IntPtr(bmpData.Scan0.ToInt32() + i), new IntPtr(CodeBmpData.Scan0.ToInt32() + i), (uint)Stride) != 0)
                {
                    byte[] temp = new byte[Stride];
                    fixed(byte* ptr = temp)
                    {
                        NativeMethods.memcpy(ptr, (void*)(bmpData.Scan0.ToInt32() + i), (uint)temp.Length);
                    }

                    CRC32 hasher = new CRC32();
                    int hash = BitConverter.ToInt32(hasher.ComputeHash(temp), 0);

                    if (EncodeCache.Count >= MaxBuffers)
                        EncodeCache.RemoveAt(0);

                    if (EncodeCache.ContainsKey(hash))
                    {
                        outStream.WriteByte(1);
                        outStream.Write(new byte[4], 0, 4);
                        outStream.Write(BitConverter.GetBytes(hash), 0, 4);
                        outStream.Write(BitConverter.GetBytes((ushort)y), 0, 2);
                    }
                    else
                    {
                        outStream.WriteByte(0);
                        outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                        outStream.Write(BitConverter.GetBytes(hash), 0, 4);
                        outStream.Write(BitConverter.GetBytes((ushort)y), 0, 2);
                        outStream.Write(temp, 0, temp.Length);
                        EncodeCache.Add(hash, temp);
                    }
                    Blocks.Add(ScanBlock);
                }
            }

            for (int i = 0; i < Blocks.Count; i++)
            {
                Bitmap cloned = (Bitmap)bitmap.Clone(Blocks[i], bitmap.PixelFormat);
                byte[] temp = base.jpgCompression.Compress(cloned);
                
                cloned.Dispose();
            }

            bitmap.UnlockBits(bmpData);
            CodeTempBitmap.UnlockBits(CodeBmpData);

            if (onVideoStreamCoding != null)
                onVideoStreamCoding(outStream, Blocks.ToArray());

            if (CodeTempBitmap != null)
                CodeTempBitmap.Dispose();
            this.CodeTempBitmap = bitmap;
        }

        public override Bitmap DecodeData(Stream inStream)
        {
            if (!inStream.CanRead)
                throw new Exception("Must have access to Read in the Stream");

            if (DecodeTempBitmap == null)
            {
                byte[] temp = new byte[4];
                inStream.Read(temp, 0, 4);
                int DataSize = BitConverter.ToInt32(temp, 0);
                temp = new byte[DataSize];
                inStream.Read(temp, 0, temp.Length);
                DecodeTempBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
                return DecodeTempBitmap;
            }

            //BitmapData BmpData = DecodeTempBitmap.LockBits(new Rectangle(0, 0, DecodeTempBitmap.Width, DecodeTempBitmap.Height), ImageLockMode.WriteOnly, DecodeTempBitmap.PixelFormat);
            //int Stride = Math.Abs(BmpData.Stride);

            while (inStream.Position < inStream.Length)
            {
                byte[] temp = new byte[11];
                if (inStream.Read(temp, 0, temp.Length) != temp.Length)
                    break;

                bool inCache = temp[0] == 1;
                int DataSize = BitConverter.ToInt32(temp, 1);
                int Hash = BitConverter.ToInt32(temp, 5);
                ushort Y = BitConverter.ToUInt16(temp, 9);

                temp = new byte[DataSize];
                if (inStream.Read(temp, 0, temp.Length) != temp.Length)
                    break;

                if (inCache)
                {
                    if (DecodeCache.ContainsKey(Hash))
                    {
                        temp = DecodeCache[Hash];
                    }
                    else
                    {

                    }
                }

                //copy new data to cached bitmap
                Bitmap tmpBmp = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));

                using (Graphics g = Graphics.FromImage(DecodeTempBitmap))
                {
                    g.DrawImage(tmpBmp, new Point(0, Y));
                }

                /*BitmapData tmpData = tmpBmp.LockBits(new Rectangle(0, 0, tmpBmp.Width, tmpBmp.Height), ImageLockMode.WriteOnly, tmpBmp.PixelFormat);
                int Offset = Y * Stride;
                NativeMethods.memcpy(new IntPtr(BmpData.Scan0.ToInt32() + Offset), tmpData.Scan0, (uint)(tmpBmp.Height * Stride));*/
                //tmpBmp.UnlockBits(tmpData);
                tmpBmp.Dispose();
            }
            //DecodeTempBitmap.UnlockBits(BmpData);

            return DecodeTempBitmap;
        }
    }
}
