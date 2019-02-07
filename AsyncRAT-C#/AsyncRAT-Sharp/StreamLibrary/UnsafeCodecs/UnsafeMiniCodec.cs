using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary.UnsafeCodecs
{
    public class UnsafeMiniCodec : IUnsafeCodec
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

        private PixelFormat EncodedFormat;
        private int EncodedWidth;
        private int EncodedHeight;
        private byte[] EncodeBuffer;
        private Bitmap decodedBitmap;

        private Size CheckBlock { get { return new Size(50, 50); } }

        public UnsafeMiniCodec(int ImageQuality = 100)
            : base(ImageQuality)
        {

        }

        public override unsafe void CodeImage(IntPtr Scan0, Rectangle ScanArea, Size ImageSize, PixelFormat Format, Stream outStream)
        {
            lock (this.ImageProcessLock)
            {
                byte* pScan0 = (byte*)Scan0.ToInt32();
                if (!outStream.CanWrite)
                    throw new Exception("Must have access to Write in the Stream");

                int Stride = 0;
                int RawLength = 0;
                int PixelSize = 0;

                FastBitmap.CalcImageOffset(0, 0, Format, ScanArea.Width); //check for FastBitmap Support
                switch (Format)
                {
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        PixelSize = 3;
                        break;
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        PixelSize = 4;
                        break;
                    default:
                        throw new NotSupportedException(Format + " is not supported.");
                }

                Stride = ImageSize.Width * PixelSize;
                RawLength = Stride * ImageSize.Height;

                //first frame
                if (EncodeBuffer == null)
                {
                    this.EncodedFormat = Format;
                    this.EncodedWidth = ImageSize.Width;
                    this.EncodedHeight = ImageSize.Height;
                    this.EncodeBuffer = new byte[RawLength];
                    fixed (byte* ptr = EncodeBuffer)
                    {
                        byte[] temp = null;
                        using (Bitmap TmpBmp = new Bitmap(ImageSize.Width, ImageSize.Height, Stride, Format, Scan0))
                        {
                            temp = base.jpgCompression.Compress(TmpBmp);
                        }

                        outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                        outStream.Write(temp, 0, temp.Length);
                        NativeMethods.memcpy(new IntPtr(ptr), Scan0, (uint)RawLength);
                    }
                    return;
                }

                if (this.EncodedFormat != Format)
                    throw new Exception("PixelFormat is not equal to previous Bitmap");
                if (this.EncodedWidth != ImageSize.Width || this.EncodedHeight != ImageSize.Height)
                    throw new Exception("Bitmap width/height are not equal to previous bitmap");
                if (ScanArea.Width > ImageSize.Width || ImageSize.Height > this.EncodedHeight)
                    throw new Exception("Scan Area Width/Height cannot be greater then the encoded image");


                List<Rectangle> Blocks = new List<Rectangle>(); //all the changes
                fixed (byte* encBuffer = EncodeBuffer)
                {
                    //1. Check for the changes in height
                    for (int y = ScanArea.Y; y < ScanArea.Height; y++)
                    {
                        Rectangle cBlock = new Rectangle(0, y, ImageSize.Width, 1);

                        if (onCodeDebugScan != null)
                            onCodeDebugScan(cBlock);

                        int Offset = FastBitmap.CalcImageOffset(0, y, Format, ImageSize.Width);
                        if (NativeMethods.memcmp(encBuffer + Offset, pScan0 + Offset, (uint)Stride) != 0)
                        {
                            int index = Blocks.Count - 1;
                            if (Blocks.Count != 0 && (Blocks[index].Y + Blocks[index].Height) == cBlock.Y)
                            {
                                cBlock = new Rectangle(Blocks[index].X, Blocks[index].Y, Blocks[index].Width, Blocks[index].Height + cBlock.Height);
                                Blocks[index] = cBlock;
                            }
                            else
                            {
                                Blocks.Add(cBlock);
                            }
                        }
                    }

                    //2. Capture all the changes using the CheckBlock
                    List<Rectangle> finalUpdates = new List<Rectangle>();
                    for (int i = 0; i < Blocks.Count; i++)
                    {
                        Rectangle scanBlock = Blocks[i];

                        //go through the Blocks
                        for (int y = scanBlock.Y; y < scanBlock.Height; y += CheckBlock.Height)
                        {
                            for (int x = scanBlock.X; x < scanBlock.Width; x += CheckBlock.Width)
                            {
                                int blockWidth = x + CheckBlock.Width < scanBlock.Width ? CheckBlock.Width : scanBlock.Width - x;
                                int blockHeight = y + CheckBlock.Height < scanBlock.Height ? CheckBlock.Height : scanBlock.Height - y;
                                Rectangle cBlock = new Rectangle(x, y, blockWidth, blockHeight);

                                if (onCodeDebugScan != null)
                                    onCodeDebugScan(cBlock);

                                //scan the block from Top To Bottom and check for changes
                                bool FoundChanges = false;
                                for (int blockY = y; blockY < y + blockHeight; blockY++)
                                {
                                    int Offset = FastBitmap.CalcImageOffset(x, blockY, Format, blockWidth);
                                    if (NativeMethods.memcmp(encBuffer + Offset, pScan0 + Offset, (uint)Stride) != 0)
                                    {
                                        FoundChanges = true;
                                        break;
                                    }
                                }

                                if (FoundChanges)
                                {
                                    int index = finalUpdates.Count - 1;
                                    if (finalUpdates.Count > 0 && (finalUpdates[index].X + finalUpdates[index].Width) == cBlock.X)
                                    {
                                        Rectangle rect = finalUpdates[index];
                                        int newWidth = cBlock.Width + rect.Width;
                                        cBlock = new Rectangle(rect.X, rect.Y, newWidth, rect.Height);
                                        finalUpdates[index] = cBlock;
                                    }
                                    else
                                    {
                                        finalUpdates.Add(cBlock);
                                    }
                                }
                            }
                        }
                    }

                    //maybe a too hard algorithm but oh well
                    SortedList<int, SortedList<int, Rectangle>> Array = finalUpdates.ToArray().RectanglesTo2D().Rectangle2DToRows();
                    List<Rectangle> FinalTemp = new List<Rectangle>();

                    for (int i = 0; i < Array.Values.Count; i++)
                    {
                        FinalTemp.AddRange(Array.Values[i].Values);
                    }

                    //fixup the height
                    for (int i = 0; i < FinalTemp.Count; )
                    {
                        if (FinalTemp.Count == 1)
                        {
                            FinalTemp.Add(FinalTemp[i]);
                            break;
                        }

                        if (i + 1 < FinalTemp.Count)
                        {
                            Rectangle curRect = FinalTemp[i];
                            Rectangle nextRect = FinalTemp[i + 1];
                            if ((curRect.Y + curRect.Height) == nextRect.Y && curRect.Width == nextRect.Width)
                            {
                                FinalTemp[i] = new Rectangle(curRect.X, curRect.Y, curRect.Width, curRect.Height + curRect.Height);
                                FinalTemp.RemoveAt(i + 1);
                            }
                            else
                            {
                                i++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    //copy changes to the EncodeBuffer and Process the Output
                    long oldPos = outStream.Position;
                    outStream.Write(new byte[4], 0, 4);
                    int TotalDataLength = 0;

                    for (int i = 0; i < FinalTemp.Count; i++)
                    {
                        Rectangle rect = FinalTemp[i];
                        int blockStride = PixelSize * rect.Width;

                        //copy changes to EncodeBuffer
                        for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                        {
                            int Offset = FastBitmap.CalcImageOffset(rect.X, y, Format, rect.Width);
                            NativeMethods.memcpy(encBuffer + Offset, pScan0 + Offset, (uint)blockStride); //copy-changes
                        }

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
                    Blocks.Clear();
                }
            }
        }

        public override unsafe Bitmap DecodeData(IntPtr CodecBuffer, uint Length)
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
    }
}