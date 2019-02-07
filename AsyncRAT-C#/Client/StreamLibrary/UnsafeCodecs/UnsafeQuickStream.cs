using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary.UnsafeCodecs
{
    public class UnsafeQuickStream : IUnsafeCodec
    {
        public override ulong CachedSize { get; internal set; }
        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        public override int BufferCount
        {
            get { return 0; }
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.AutoDispose | CodecOption.RequireSameSize; }
        }
        private PixelFormat EncodedFormat;
        private int EncodedWidth;
        private int EncodedHeight;
        private ulong[] EncodeBuffer;

        private int BlockWidth = 0;
        private int BlockHeight = 0;
        private Bitmap decodedBitmap;

        public List<Rectangle> VerifyPoints = null;

        public Size CheckBlock { get; private set; } 
        public UnsafeQuickStream(int ImageQuality = 100)
            : base(ImageQuality)
        {
            this.CheckBlock = new Size(50, 50);//width must be bigger then 3
        }

        public override unsafe void CodeImage(IntPtr Scan0, Rectangle OutputRect, Size InputSize, PixelFormat Format, Stream outStream)
        {
            byte* pScan0 = (byte*)Scan0.ToInt32();
            if (!outStream.CanWrite)
                throw new Exception("Must have access to Write in the Stream");

            int Stride = 0;
            int RawLength = 0;
            int PixelSize = 0;

            switch (Format)
            {
                case PixelFormat.Format24bppRgb:
                    PixelSize = 3;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    PixelSize = 4;
                    break;
                default:
                    throw new NotSupportedException(Format.ToString());
            }

            Stride = InputSize.Width * PixelSize;
            RawLength = Stride * InputSize.Height;


            if (EncodedWidth == 0 && EncodedHeight == 0)
            {
                this.EncodedFormat = Format;
                this.EncodedWidth = OutputRect.Width;
                this.EncodedHeight = OutputRect.Height;

                byte[] temp = null;
                using (Bitmap TmpBmp = new Bitmap(OutputRect.Width, OutputRect.Height, Stride, Format, Scan0))
                {
                    temp = base.jpgCompression.Compress(TmpBmp);
                }
                outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                outStream.Write(temp, 0, temp.Length);
                return;
            }

            List<Rectangle> Points = ProcessChanges(Scan0, OutputRect, Format, InputSize.Width);

            VerifyPoints = Points;
            long oldPos = outStream.Position;
            outStream.Write(new byte[4], 0, 4);
            int TotalDataLength = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                Rectangle rect = Points[i];
                int blockStride = PixelSize * rect.Width;

                Bitmap TmpBmp = new Bitmap(rect.Width, rect.Height, Format);
                BitmapData TmpData = TmpBmp.LockBits(new Rectangle(0, 0, TmpBmp.Width, TmpBmp.Height), ImageLockMode.ReadWrite, TmpBmp.PixelFormat);
                for (int j = 0, offset = 0; j < rect.Height; j++)
                {
                    int blockOffset = (Stride * (rect.Y + j)) + (PixelSize * rect.X);
                    NativeMethods.memcpy((byte*)TmpData.Scan0.ToPointer() + offset, pScan0 + blockOffset, (uint)blockStride); //copy-changes
                    offset += blockStride;
                }
                TmpBmp.UnlockBits(TmpData);

                outStream.Write(BitConverter.GetBytes(rect.X), 0, 4);
                outStream.Write(BitConverter.GetBytes(rect.Y), 0, 4);
                outStream.Write(BitConverter.GetBytes(rect.Width), 0, 4);
                outStream.Write(BitConverter.GetBytes(rect.Height), 0, 4);
                outStream.Write(new byte[4], 0, 4);

                long length = outStream.Position;
                long OldPos = outStream.Position;
                base.jpgCompression.Compress(TmpBmp, ref outStream);
                length = outStream.Position - length;

                outStream.Position = OldPos - 4;
                outStream.Write(BitConverter.GetBytes((int)length), 0, 4);
                outStream.Position += length;

                TmpBmp.Dispose();
                TotalDataLength += (int)length + (4 * 5);
            }
            outStream.Position = oldPos;
            outStream.Write(BitConverter.GetBytes(TotalDataLength), 0, 4);
        }

        private unsafe List<Rectangle> ProcessChanges(IntPtr Scan0, Rectangle OutputRect, PixelFormat Format, int ImageWidth)
        {
            if (EncodeBuffer == null)
            {
                this.BlockWidth = (int)Math.Floor((float)(OutputRect.Width / CheckBlock.Width));
                this.BlockHeight = (int)Math.Floor((double)(OutputRect.Height / CheckBlock.Height));
                int TotalBlocks = (int)Math.Floor((float)(BlockHeight * BlockWidth));
                this.EncodeBuffer = new ulong[TotalBlocks];
            }

            List<Rectangle> points = new List<Rectangle>();
            int StartScan = Scan0.ToInt32();

            for (int y = OutputRect.Y; y < OutputRect.Height + OutputRect.Y; y += CheckBlock.Height)
            {
                if (y + CheckBlock.Height > OutputRect.Height)
                    break;

                for (int x = OutputRect.X; x < OutputRect.Width + OutputRect.X; x += CheckBlock.Width)
                {
                    if (x + CheckBlock.Width > OutputRect.Width)
                        break;

                    int EncodeOffset = GetOffset(x, y);
                    long offset = FastBitmap.CalcImageOffset(x, y, Format, ImageWidth);
                    ulong* ScanPtr = (ulong*)(StartScan + offset);

                    if (EncodeBuffer[EncodeOffset] != *ScanPtr)
                    {
                        EncodeBuffer[EncodeOffset] = *ScanPtr;

                        Rectangle cBlock = new Rectangle(x, y, CheckBlock.Width, CheckBlock.Height);
                        int index = points.Count - 1;
                        if (points.Count > 0 && (points[index].X + points[index].Width) == cBlock.X)
                        {
                            Rectangle rect = points[index];
                            int newWidth = cBlock.Width + rect.Width;
                            cBlock = new Rectangle(rect.X, rect.Y, newWidth, rect.Height);
                            points[index] = cBlock;
                        }
                        else
                        {
                            points.Add(cBlock);
                        }
                    }
                }
            }
            return points;
        }

        private Point GetOffsetPoint(int x, int y)
        {
            return new Point((int)Math.Floor((float)(y / CheckBlock.Height)) * BlockWidth,
                             (int)Math.Floor((double)(x / CheckBlock.Width)));
        }

        private int GetOffset(int x, int y)
        {
            return (int)Math.Floor((float)(y / CheckBlock.Height)) * BlockWidth +
                   (int)Math.Floor((double)(x / CheckBlock.Width));
        }

        public override unsafe System.Drawing.Bitmap DecodeData(System.IO.Stream inStream)
        {
            byte[] temp = new byte[4];
            inStream.Read(temp, 0, 4);
            int DataSize = BitConverter.ToInt32(temp, 0);

            if (decodedBitmap == null)
            {
                temp = new byte[DataSize];
                inStream.Read(temp, 0, temp.Length);
                this.decodedBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
                return decodedBitmap;
            }

            List<Rectangle> updates = new List<Rectangle>();
            Rectangle rect;
            Graphics g = Graphics.FromImage(decodedBitmap);
            Bitmap tmp;
            byte[] buffer = null;
            MemoryStream m;

            while (DataSize > 0)
            {
                byte[] tempData = new byte[4 * 5];
                inStream.Read(tempData, 0, tempData.Length);

                rect = new Rectangle(BitConverter.ToInt32(tempData, 0), BitConverter.ToInt32(tempData, 4),
                                     BitConverter.ToInt32(tempData, 8), BitConverter.ToInt32(tempData, 12));
                int UpdateLen = BitConverter.ToInt32(tempData, 16);
                buffer = new byte[UpdateLen];
                inStream.Read(buffer, 0, buffer.Length);

                if (onDecodeDebugScan != null)
                    onDecodeDebugScan(rect);

                m = new MemoryStream(buffer);
                tmp = (Bitmap)Image.FromStream(m);
                g.DrawImage(tmp, rect.Location);
                tmp.Dispose();

                m.Close();
                m.Dispose();
                DataSize -= UpdateLen + (4 * 5);
            }
            g.Dispose();
            return decodedBitmap;
        }

        public override unsafe System.Drawing.Bitmap DecodeData(IntPtr CodecBuffer, uint Length)
        {
            if (Length < 4)
                return decodedBitmap;

            int DataSize = *(int*)(CodecBuffer);
            if (decodedBitmap == null)
            {
                byte[] temp = new byte[DataSize];
                fixed (byte* tempPtr = temp)
                {
                    NativeMethods.memcpy(new IntPtr(tempPtr), new IntPtr(CodecBuffer.ToInt32() + 4), (uint)DataSize);
                }

                this.decodedBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
                return decodedBitmap;
            }

            byte* bufferPtr = (byte*)CodecBuffer.ToInt32();
            if (DataSize > 0)
            {
                Graphics g = Graphics.FromImage(decodedBitmap);
                for (int i = 4; DataSize > 0; )
                {
                    Rectangle rect = new Rectangle(*(int*)(bufferPtr + i), *(int*)(bufferPtr + i + 4),
                                                   *(int*)(bufferPtr + i + 8), *(int*)(bufferPtr + i + 12));
                    int UpdateLen = *(int*)(bufferPtr + i + 16);
                    byte[] temp = new byte[UpdateLen];

                    fixed (byte* tempPtr = temp)
                    {
                        NativeMethods.memcpy(new IntPtr(tempPtr), new IntPtr(CodecBuffer.ToInt32() + i + 20), (uint)UpdateLen);
                        using (Bitmap TmpBmp = new Bitmap(rect.Width, rect.Height, rect.Width * 3, decodedBitmap.PixelFormat, new IntPtr(tempPtr)))
                        {
                            g.DrawImage(TmpBmp, new Point(rect.X, rect.Y));
                        }
                    }
                    DataSize -= UpdateLen + (4 * 5);
                    i += UpdateLen + (4 * 5);
                }
                g.Dispose();
            }
            return decodedBitmap;
        }
    }
}
