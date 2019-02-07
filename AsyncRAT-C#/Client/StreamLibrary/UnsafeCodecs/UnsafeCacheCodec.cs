using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary.UnsafeCodecs
{
    public class UnsafeCacheCodec : IUnsafeCodec
    {
        private const int BlockCount = 5;
        private const int HashBlockCount = 8192;

        public override ulong CachedSize
        {
            get { return 0; }
            internal set { }
        }

        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        public override int BufferCount
        {
            get { return 0; }
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.HasBuffers | CodecOption.RequireSameSize; }
        }

        private Bitmap decodedBitmap;
        private int EncodedWidth;
        private int EncodedHeight;
        private PixelFormat EncodedFormat;
        private MurmurHash2Unsafe hasher;
        private SortedList<int, SortedList<int, uint>> EncodeBuffer;
        private Rectangle[] Offsets;
        private BlockInfo[] EncodeHashBlocks;
        private BlockInfo[] DecodeHashBlocks;

        public ulong EncodedFrames { get; private set; }
        public ulong DecodedFrames { get; private set; }

        public UnsafeCacheCodec(int ImageQuality = 80)
            : base(ImageQuality)
        {
            this.hasher = new MurmurHash2Unsafe();
            this.Offsets = new Rectangle[BlockCount];
            this.EncodeHashBlocks = new BlockInfo[HashBlockCount];
            this.DecodeHashBlocks = new BlockInfo[HashBlockCount];
            for (int i = 0; i < HashBlockCount; i++)
            {
                EncodeHashBlocks[i] = new BlockInfo();
                DecodeHashBlocks[i] = new BlockInfo();
            }
        }

        public override unsafe void CodeImage(IntPtr Scan0, Rectangle ScanArea, Size ImageSize, PixelFormat Format, Stream outStream)
        {
            if (ImageSize.Width == 0 || ImageSize.Height == 0)
                throw new ArgumentException("The width and height must be 1 or higher");
            if (ImageSize.Width < BlockCount || ImageSize.Height < BlockCount)
                throw new Exception("The Image size Width/Height must be bigger then the Block Count " + BlockCount + "x" + BlockCount);

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

            int Stride = ImageSize.Width * PixelSize;
            int RawLength = Stride * ImageSize.Height;

            if (EncodedFrames == 0)
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

                double size = (double)ImageSize.Width / (double)BlockCount;

                //fix floating point here
                for (int i = 0, j = 0; i < BlockCount; i++, j += (int)size)
                {
                    Offsets[i] = new Rectangle(j, 0, (int)size, 1);
                }

                EncodeBuffer = new SortedList<int, SortedList<int, uint>>();
                for (int y = 0; y < ImageSize.Height; y++)
                {
                    for(int i = 0; i < Offsets.Length; i++)
                    {
                        if (!EncodeBuffer.ContainsKey(y))
                            EncodeBuffer.Add(y, new SortedList<int, uint>());
                        if (!EncodeBuffer[y].ContainsKey(Offsets[i].X))
                            EncodeBuffer[y].Add(Offsets[i].X, 0); //0=hash
                    }
                }
                EncodedFrames++;
                return;
            }

            long oldPos = outStream.Position;
            outStream.Write(new byte[4], 0, 4);
            int TotalDataLength = 0;

            List<HashBlock> ImageOffsets = new List<HashBlock>();
            List<uint> ImageHashes = new List<uint>();

            byte* pScan0 = (byte*)Scan0.ToInt32();
            for (int i = 0; i < Offsets.Length; i++)
            {
                for (int y = 0; y < ImageSize.Height; y++)
                {
                    Rectangle ScanRect = Offsets[i];
                    ScanRect.Y = y;

                    int offset = (y * Stride) + (ScanRect.X * PixelSize);

                    if (offset+Stride > Stride * ImageSize.Height)
                        break;

                    uint Hash = hasher.Hash(pScan0 + offset, (int)Stride);
                    uint BlockOffset = Hash % HashBlockCount;

                    BlockInfo blockInfo = EncodeHashBlocks[BlockOffset];

                    if (EncodeBuffer[y][ScanRect.X] != Hash)
                    {
                        if (!blockInfo.Hashes.ContainsKey(Hash))
                        {
                            int index = ImageOffsets.Count - 1;
                            if (ImageOffsets.Count > 0 && ImageOffsets[index].Location.Y + 1 == ScanRect.Y)
                            {
                                Rectangle rect = ImageOffsets[index].Location;
                                ImageOffsets[index].Location = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + 1);
                            }
                            else
                            {
                                ImageOffsets.Add(new HashBlock(Hash, false, new Rectangle(ScanRect.X, y, ScanRect.Width, 1)));
                            }

                            blockInfo.Hashes.Add(Hash, new HashBlock(Hash, false, new Rectangle(ScanRect.X, y, ScanRect.Width, 1)));
                            ImageHashes.Add(Hash);
                        }
                        EncodeBuffer[y][ScanRect.X] = Hash;
                    }
                }
            }

            if (ImageOffsets.Count > 0)
            {
                for (int i = 0; i < Offsets.Length; i++)
                {
                    Rectangle TargetOffset = Offsets[i];
                    int Height = GetOffsetHeight(ImageOffsets, TargetOffset);
                    Bitmap TmpBmp = new Bitmap(TargetOffset.Width, Height, Format);
                    BitmapData TmpData = TmpBmp.LockBits(new Rectangle(0, 0, TmpBmp.Width, TmpBmp.Height), ImageLockMode.ReadWrite, TmpBmp.PixelFormat);
                    int blockStride = PixelSize * TargetOffset.Width;
                    List<HashBlock> UsedOffsets = new List<HashBlock>();

                    for (int j = 0; j < ImageOffsets.Count; j++)
                    {
                        Rectangle rect = ImageOffsets[j].Location;

                        if (rect.Width != TargetOffset.Width || rect.X != TargetOffset.X)
                            continue; //error in 1440p, did not tested futher

                        for (int o = 0, offset = 0; o < rect.Height; o++)
                        {
                            int blockOffset = (Stride * (rect.Y + o)) + (PixelSize * rect.X);
                            NativeMethods.memcpy((byte*)TmpData.Scan0.ToPointer() + offset, pScan0 + blockOffset, (uint)blockStride); //copy-changes
                            offset += blockStride;
                            UsedOffsets.Add(ImageOffsets[j]);
                        }
                    }
                    TmpBmp.UnlockBits(TmpData);
                    TmpBmp.Dispose();

                    outStream.Write(BitConverter.GetBytes((short)UsedOffsets.Count), 0, 2);
                    if (UsedOffsets.Count > 0)
                    {
                        outStream.Write(BitConverter.GetBytes((short)UsedOffsets[0].Location.X), 0, 2);
                        for (int j = 0; j < UsedOffsets.Count; j++)
                        {
                            outStream.Write(BitConverter.GetBytes((short)UsedOffsets[j].Location.Y), 0, 2);
                            outStream.Write(BitConverter.GetBytes((short)UsedOffsets[j].Location.Height), 0, 2);
                            outStream.Write(BitConverter.GetBytes(UsedOffsets[j].Hash), 0, 4);
                        }
                        byte[] CompressedImg = base.jpgCompression.Compress(TmpBmp);
                        outStream.Write(BitConverter.GetBytes(CompressedImg.Length), 0, 4);
                        outStream.Write(CompressedImg, 0, CompressedImg.Length);
                    }


                    //TotalDataLength += (int)length + (4 * 5);
                }
            }
            EncodedFrames++;
        }

        private int GetOffsetHeight(List<HashBlock> ImageOffsets, Rectangle rect)
        {
            int height = 0;
            for (int i = 0; i < ImageOffsets.Count; i++)
            {
                if (ImageOffsets[i].Location.Width == rect.Width && ImageOffsets[i].Location.X == rect.X)
                    height += ImageOffsets[i].Location.Height;
            }
            return height;
        }

        public override Bitmap DecodeData(Stream inStream)
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
            //return decodedBitmap;

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
            return null;
        }

        private class BlockInfo
        {
            public SortedList<uint, HashBlock> Hashes { get; private set; }
            public BlockInfo()
            {
                Hashes = new SortedList<uint, HashBlock>();
            }
        }
        private class HashBlock
        {
            public bool Cached { get; set; }
            public uint Hash { get; set; }
            public Rectangle Location { get; set; }

            public HashBlock(uint Hash, bool Cached, Rectangle Location)
            {
                this.Hash = Hash;
                this.Cached = Cached;
                this.Location = Location;
            }
            public HashBlock()
            {

            }
        }
        private class ImageOffset
        {
            public Rectangle Location { get; set; }
            public List<uint> Hashes { get; set; }

            public ImageOffset()
            {
                this.Hashes = new List<uint>();
            }
        }
    }
}