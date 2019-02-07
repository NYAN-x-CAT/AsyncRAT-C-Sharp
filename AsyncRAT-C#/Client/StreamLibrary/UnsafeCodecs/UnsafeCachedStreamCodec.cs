using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary.UnsafeCodecs
{
    public class UnsafeCachedStreamCodec : IUnsafeCodec
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
            get { return 0; }
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.HasBuffers; }
        }

        private PixelFormat EncodedFormat;
        private int EncodedWidth;
        private int EncodedHeight;
        private static Size CheckBlock = new Size(50, 50);
        private MurmurHash2Unsafe Hasher = new MurmurHash2Unsafe();
        private List<Frame> Frames = new List<Frame>();
        public const int MAX_FRAMES = 120;

        private ulong[] DecodeBuffer;
        private Bitmap DecodedBitmap;
        private List<Bitmap> DecodedFrames = new List<Bitmap>();

        public UnsafeCachedStreamCodec(int ImageQuality)
            : base(ImageQuality)
        {

        }

        public override unsafe void CodeImage(IntPtr Scan0, Rectangle ScanArea, Size ImageSize, PixelFormat Format, Stream outStream)
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

            Stride = ImageSize.Width * PixelSize;
            RawLength = Stride * ImageSize.Height;

            if (EncodedWidth == 0 && EncodedHeight == 0)
            {
                this.EncodedFormat = Format;
                this.EncodedWidth = ImageSize.Width;
                this.EncodedHeight = ImageSize.Height;

                byte[] temp = null;
                using (Bitmap TmpBmp = new Bitmap(ImageSize.Width, ImageSize.Height, Stride, Format, Scan0))
                {
                    temp = base.jpgCompression.Compress(TmpBmp);
                }
                outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                outStream.Write(temp, 0, temp.Length);
                return;
            }

            Frame frame = new Frame(ImageSize.Width, ImageSize.Height);

            int Blocks = (ScanArea.Width / CheckBlock.Width) * (ScanArea.Height / CheckBlock.Height);
            int RawSizeUsed = 0;

            long oldPos = outStream.Position;
            outStream.Write(new byte[4], 0, 4);
            int TotalDataLength = 0;
            List<Rectangle> ChangedBlocks = new List<Rectangle>();

            for(int y = ScanArea.Y; y < ScanArea.Height; )
            {
                int height = y + CheckBlock.Height < ScanArea.Height ? CheckBlock.Height : ScanArea.Height - y;
                for (int x = ScanArea.X; x < ScanArea.Width; )
                {
                    int width = x + CheckBlock.Width < ScanArea.Width ? CheckBlock.Width : ScanArea.Width - x;
                    int BlockStride = Format == PixelFormat.Format24bppRgb ? width * 3 : width * 4;
                    decimal FinalHash = 0;

                    for (int h = 0; h < height; h++)
                    {
                        int Offset = FastBitmap.CalcImageOffset(x, y+h, Format, ImageSize.Width);
                        FinalHash += Hasher.Hash(pScan0 + Offset, BlockStride);
                    }

                    if(onCodeDebugScan != null)
                        onCodeDebugScan(new Rectangle(x, y, width, height));

                    bool FoundBlock = false;
                    decimal FoundHash = 0;
                    int FrameIndex = 0;
                    for (int i = 0; i < Frames.Count; i++)
                    {
                        decimal hash = Frames[i].GetHashBlock(x, y);
                        if (hash == FinalHash)
                        {
                            FrameIndex = i;
                            FoundBlock = true;
                            FoundHash = hash;
                            break;
                        }
                    }

                    frame.AddHashBlock(x, y, FinalHash);

                    if (!FoundBlock)
                    {
                        int index = ChangedBlocks.Count - 1;
                        Rectangle cBlock = new Rectangle(x, y, width, height);

                        if (ChangedBlocks.Count > 0 && (ChangedBlocks[index].X + ChangedBlocks[index].Width) == cBlock.X)
                        {
                            Rectangle rect = ChangedBlocks[index];
                            int newWidth = cBlock.Width + rect.Width;
                            cBlock = new Rectangle(rect.X, rect.Y, newWidth, rect.Height);
                            ChangedBlocks[index] = cBlock;
                        }
                        /*else if (ChangedBlocks.Count > 0 && (ChangedBlocks[index].Y + ChangedBlocks[index].Height) == cBlock.Y)
                        {
                            Rectangle rect = ChangedBlocks[index];
                            int newHeight = cBlock.Height + rect.Height;
                            cBlock = new Rectangle(rect.X, rect.Y, rect.Width, newHeight);
                            ChangedBlocks[index] = cBlock;
                        }*/
                        else
                        {
                            ChangedBlocks.Add(cBlock);
                        }
                        RawSizeUsed += BlockStride * height;
                    }
                    x += width;
                }
                y += height;
            }

            //write all the blocks
            for (int i = 0; i < ChangedBlocks.Count; i++)
            {
                Rectangle rect = ChangedBlocks[i];
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
            ChangedBlocks.Clear();

            Frames.Add(frame);
            if (Frames.Count > MAX_FRAMES)
                Frames.RemoveAt(0);
        }

        public override unsafe System.Drawing.Bitmap DecodeData(System.IO.Stream inStream)
        {
            byte[] temp = new byte[4];
            inStream.Read(temp, 0, 4);
            int DataSize = BitConverter.ToInt32(temp, 0);

            if (DecodedBitmap == null)
            {
                temp = new byte[DataSize];
                inStream.Read(temp, 0, temp.Length);
                this.DecodedBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(temp));
                DecodedFrames.Add(DecodedBitmap);
                return DecodedBitmap;
            }

            Rectangle rect;
            Graphics g = Graphics.FromImage(DecodedBitmap);
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
            return DecodedBitmap;
        }

        public override unsafe System.Drawing.Bitmap DecodeData(IntPtr CodecBuffer, uint Length)
        {
            return new Bitmap(10, 10);
        }

        private class HashedBlock
        {
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int FrameIndex { get; private set; }
            public HashedBlock(int x, int y, int width, int height, int FrameIndex)
            {
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
                this.FrameIndex = FrameIndex;
            }

            public override string ToString()
            {
                return "X:" + X + ", Y:" + Y + ", Width:" + Width + ", Height:" + Height + ", FrameIndex:" + FrameIndex;
            }
        }

        private class Frame
        {
            public decimal[,] HashBlocks { get; private set; }

            public Frame(int ImageWidth, int ImageHeight)
            {
                this.HashBlocks = new decimal[(ImageWidth / UnsafeCachedStreamCodec.CheckBlock.Width) + 2, (ImageHeight / UnsafeCachedStreamCodec.CheckBlock.Height) + 2];
            }

            public void AddHashBlock(int x, int y, decimal Hash)
            {
                x = x / UnsafeCachedStreamCodec.CheckBlock.Width;
                y = y / UnsafeCachedStreamCodec.CheckBlock.Height;
                this.HashBlocks[x, y] = Hash;
            }
            public decimal GetHashBlock(int x, int y)
            {
                x = x / UnsafeCachedStreamCodec.CheckBlock.Width;
                y = y / UnsafeCachedStreamCodec.CheckBlock.Height;
                return this.HashBlocks[x, y];
            }
        }
    }
}