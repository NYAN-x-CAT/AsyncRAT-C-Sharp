using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary.Codecs
{
    public class QuickStreamCodec : IVideoCodec
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

        /// <summary>
        /// Initialize a new object of QuickStreamCodec
        /// </summary>
        /// <param name="ImageQuality">The image quality 0-100%</param>
        public QuickStreamCodec(int ImageQuality = 100)
            : base(ImageQuality)
        {

        }

        public override int BufferCount
        {
            get { return 1; }
        }
        public override CodecOption CodecOptions
        {
            get { return CodecOption.RequireSameSize | CodecOption.AutoDispose; }
        }

        public override void CodeImage(Bitmap bitmap, Stream outStream)
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
                if(onCodeDebugScan != null)
                    onCodeDebugScan(new Rectangle(0, y, bitmap.Width, 1));

                Rectangle ScanBlock = new Rectangle(0, y, bitmap.Width, 1);
                if (NativeMethods.memcmp(new IntPtr(bmpData.Scan0.ToInt32() + i), new IntPtr(CodeBmpData.Scan0.ToInt32() + i), (uint)Stride) != 0)
                {
                    int index = Blocks.Count - 1;
                    if (Blocks.Count != 0 && (Blocks[index].Y + Blocks[index].Height) == ScanBlock.Y)
                    {
                        ScanBlock = new Rectangle(Blocks[index].X, Blocks[index].Y, Blocks[index].Width, Blocks[index].Height + ScanBlock.Height);
                        Blocks[index] = ScanBlock;
                    }
                    else
                    {
                        Blocks.Add(ScanBlock);
                    }
                }
            }

            long oldPos = outStream.Position;
            outStream.Write(new byte[4], 0, 4);
            int TotalDataLength = 0;

            for (int i = 0; i < Blocks.Count; i++)
            {
                Bitmap cloned = (Bitmap)bitmap.Clone(Blocks[i], bitmap.PixelFormat);
                byte[] temp = base.jpgCompression.Compress(cloned);
                outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                outStream.Write(BitConverter.GetBytes((ushort)Blocks[i].Y), 0, 2);
                outStream.Write(temp, 0, temp.Length);
                cloned.Dispose();
                TotalDataLength += 6 + temp.Length;
            }

            outStream.Position = oldPos;
            outStream.Write(BitConverter.GetBytes(TotalDataLength), 0, 4);

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
                int DataSize = BitConverter.ToInt32(temp, 4);
                temp = new byte[DataSize];
                inStream.Read(temp, 0, temp.Length);
                DecodeTempBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
                return DecodeTempBitmap;
            }


            byte[] LenTemp = new byte[4];
            inStream.Read(LenTemp, 0, 4);
            int DataLength = BitConverter.ToInt32(LenTemp, 0);

            //BitmapData BmpData = DecodeTempBitmap.LockBits(new Rectangle(0, 0, DecodeTempBitmap.Width, DecodeTempBitmap.Height), ImageLockMode.WriteOnly, DecodeTempBitmap.PixelFormat);
            //int Stride = Math.Abs(BmpData.Stride);

            while (DataLength > 0)
            {
                byte[] temp = new byte[6];
                if (inStream.Read(temp, 0, temp.Length) != temp.Length)
                    break;

                int DataSize = BitConverter.ToInt32(temp, 0);
                ushort Y = BitConverter.ToUInt16(temp, 4);
                temp = new byte[DataSize];
                if (inStream.Read(temp, 0, temp.Length) != temp.Length)
                    break;

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
                DataLength -= 6 + temp.Length;
            }
            //DecodeTempBitmap.UnlockBits(BmpData);

            return DecodeTempBitmap;
        }
    }
}