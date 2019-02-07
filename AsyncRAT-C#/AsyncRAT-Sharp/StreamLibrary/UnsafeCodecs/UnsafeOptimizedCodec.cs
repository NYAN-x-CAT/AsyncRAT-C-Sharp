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
    public class UnsafeOptimizedCodec : IUnsafeCodec
    {
        private class PopulairPoint
        {
            public Rectangle Rect;
            public int Score;
            public Stopwatch LastUpdate;

            public PopulairPoint(Rectangle rect)
            {
                this.Rect = rect;
                this.Score = 0;
                this.LastUpdate = Stopwatch.StartNew();
            }
        }

        public override ulong CachedSize
        {
            get;
            internal set;
        }

        public override int BufferCount
        {
            get { return 1; }
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.RequireSameSize; }
        }

        public Size CheckBlock { get; private set; }
        private object ImageProcessLock = new object();
        private byte[] EncodeBuffer;
        private Bitmap decodedBitmap;
        private PixelFormat EncodedFormat;
        private int EncodedWidth;
        private int EncodedHeight;
        private List<PopulairPoint> populairPoints;
        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;
        private Stopwatch ScreenRefreshSW = Stopwatch.StartNew();

        //options
        /// <summary> If a part in the image is been changing for the last x milliseconds it will be seen as a video </summary>
        public uint AliveTimeForBeingVideo = 5000;
        /// <summary> This will check if the video went away or stopped playing so it will refresh the other parts in the image </summary>
        public uint ScreenRefreshTimer = 2000;
        /// <summary> The size for being a video, if bigger or equal to the VideoScreenSize it must be a video </summary>
        public Size VideoScreenSize = new Size(100, 100);

        /// <summary>
        /// Initialize a new object of UnsafeOptimizedCodec
        /// </summary>
        /// <param name="ImageQuality">The quality to use between 0-100</param>
        public UnsafeOptimizedCodec(int ImageQuality = 100)
            : base(ImageQuality)
        {
            this.populairPoints = new List<PopulairPoint>();
            this.CheckBlock = new Size(15, 1);
        }

        public override unsafe void CodeImage(IntPtr Scan0, Rectangle ScanArea, Size ImageSize, PixelFormat Format, Stream outStream)
        {
            lock (ImageProcessLock)
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

                if (ScreenRefreshSW.ElapsedMilliseconds > ScreenRefreshTimer)
                {
                    for (int i = 0; i < populairPoints.Count; i++)
                    {
                        if (populairPoints[i].Score == 0 || populairPoints[i].LastUpdate.Elapsed.Seconds > 5)
                        {
                            populairPoints.RemoveAt(i);
                        }
                    }
                    ScreenRefreshSW = Stopwatch.StartNew();
                }

                long oldPos = outStream.Position;
                outStream.Write(new byte[4], 0, 4);
                int TotalDataLength = 0;

                List<byte[]> updates = new List<byte[]>();
                MemoryStream ms = new MemoryStream();
                byte[] buffer = null;

                if (this.EncodedFormat != Format)
                    throw new Exception("PixelFormat is not equal to previous Bitmap");

                if (this.EncodedWidth != ImageSize.Width || this.EncodedHeight != ImageSize.Height)
                    throw new Exception("Bitmap width/height are not equal to previous bitmap");

                List<Rectangle> Blocks = new List<Rectangle>();
                int index = 0;

                Size s = new Size(ScanArea.Width, CheckBlock.Height);
                Size lastSize = new Size(ScanArea.Width % CheckBlock.Width, ScanArea.Height % CheckBlock.Height);

                int lasty = ScanArea.Height - lastSize.Height;
                int lastx = ScanArea.Width - lastSize.Width;

                Rectangle cBlock = new Rectangle();
                List<Rectangle> finalUpdates = new List<Rectangle>();

                PopulairPoint[] points = GetPossibleVideos();
                if (points.Length > 0)
                {
                    ScanArea = new Rectangle(points[0].Rect.X, points[0].Rect.Y, points[0].Rect.Width + points[0].Rect.X, points[0].Rect.Height + points[0].Rect.Y);
                }

                s = new Size(ScanArea.Width, s.Height);
                fixed (byte* encBuffer = EncodeBuffer)
                {
                    if (points.Length == 0) //only scan if there is no video
                    {
                        for (int y = ScanArea.Y; y != ScanArea.Height; )
                        {
                            if (y == lasty)
                                s = new Size(ScanArea.Width, lastSize.Height);
                            cBlock = new Rectangle(ScanArea.X, y, ScanArea.Width, s.Height);

                            int offset = (y * Stride) + (ScanArea.X * PixelSize);
                            if (NativeMethods.memcmp(encBuffer + offset, pScan0 + offset, (uint)Stride) != 0)
                            {
                                if (onCodeDebugScan != null)
                                    onCodeDebugScan(cBlock);

                                index = Blocks.Count - 1;
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
                            y += s.Height;
                        }

                        for (int i = 0, x = ScanArea.X; i < Blocks.Count; i++)
                        {
                            s = new Size(CheckBlock.Width, Blocks[i].Height);
                            x = ScanArea.X;
                            while (x != ScanArea.Width)
                            {
                                if (x == lastx)
                                    s = new Size(lastSize.Width, Blocks[i].Height);

                                cBlock = new Rectangle(x, Blocks[i].Y, s.Width, Blocks[i].Height);
                                bool FoundChanges = false;
                                int blockStride = PixelSize * cBlock.Width;

                                for (int j = 0; j < cBlock.Height; j++)
                                {
                                    int blockOffset = (Stride * (cBlock.Y + j)) + (PixelSize * cBlock.X);
                                    if (NativeMethods.memcmp(encBuffer + blockOffset, pScan0 + blockOffset, (uint)blockStride) != 0)
                                        FoundChanges = true;
                                    NativeMethods.memcpy(encBuffer + blockOffset, pScan0 + blockOffset, (uint)blockStride); //copy-changes
                                }

                                if (onCodeDebugScan != null)
                                    onCodeDebugScan(cBlock);

                                if (FoundChanges)
                                {
                                    index = finalUpdates.Count - 1;
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
                                x += s.Width;
                            }
                        }
                    }
                    else
                    {
                        finalUpdates.Add(points[0].Rect);
                    }
                }

                for (int i = 0; i < finalUpdates.Count; i++)
                {
                    Rectangle rect = finalUpdates[i];
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

                    if (rect.Width > VideoScreenSize.Width && rect.Height > VideoScreenSize.Height)
                    {
                        PopulairPoint point = null;
                        if (GetPopulairPoint(rect, ref point))
                        {
                            point.Score++;
                            point.LastUpdate = Stopwatch.StartNew();
                            //Console.WriteLine("[" + populairPoints.Count + "]Video spotted at x:" + rect.X + ", y:" + rect.Y + ", width:" + rect.Width + ", height:" + rect.Height);
                        }
                        else
                        {
                            populairPoints.Add(new PopulairPoint(rect));
                        }
                    }

                    TmpBmp.Dispose();
                    TotalDataLength += (int)length + (4 * 5);
                }

                /*for (int i = 0; i < finalUpdates.Count; i++)
                {
                    Rectangle rect = finalUpdates[i];
                    int blockStride = PixelSize * rect.Width;
                    buffer = new byte[blockStride * rect.Height];

                    fixed (byte* ptr = buffer)
                    {
                        for (int j = 0, offset = 0; j < rect.Height; j++)
                        {
                            int blockOffset = (Stride * (rect.Y + j)) + (PixelSize * rect.X);
                            NativeMethods.memcpy(ptr + offset, pScan0 + blockOffset, (uint)blockStride); //copy-changes
                            offset += blockStride;
                        }
                    
                        using (Bitmap TmpBmp = new Bitmap(rect.Width, rect.Height, rect.Width * PixelSize, Format, new IntPtr(ptr)))
                        {
                            buffer = base.jpgCompression.Compress(TmpBmp);

                            if (rect.Width > VideoScreenSize.Width && rect.Height > VideoScreenSize.Height)
                            {
                                PopulairPoint point = null;
                                if (GetPopulairPoint(rect, ref point))
                                {
                                     point.Score++;
                                     point.LastUpdate = Stopwatch.StartNew();
                                     Console.WriteLine("[" + populairPoints.Count + "]Video spotted at x:" + rect.X + ", y:" + rect.Y + ", width:" + rect.Width + ", height:" + rect.Height);
                                }
                                else
                                {
                                    populairPoints.Add(new PopulairPoint(rect));
                                }
                            }
                        }
                    }

                    outStream.Write(BitConverter.GetBytes(rect.X), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Y), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Width), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Height), 0, 4);
                    outStream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
                    outStream.Write(buffer, 0, buffer.Length);
                    TotalDataLength += buffer.Length + (4 * 5);
                }*/

                outStream.Position = oldPos;
                outStream.Write(BitConverter.GetBytes(TotalDataLength), 0, 4);
                Blocks.Clear();
                ms.Close();
                ms.Dispose();
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

                    fixed(byte* tempPtr = temp)
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

        private bool GetPopulairPoint(Rectangle rect, ref PopulairPoint PopuPoint)
        {
            for (int i = 0; i < populairPoints.Count; i++)
            {
                PopulairPoint point = populairPoints[i];
                if (point.Rect.Width == rect.Width &&
                    point.Rect.Height == rect.Height &&
                    point.Rect.X == rect.X &&
                    point.Rect.Y == rect.Y)
                {
                    PopuPoint = populairPoints[i];
                    return true;
                }
            }
            return false;
        }

        private PopulairPoint[] GetPossibleVideos()
        {
            List<PopulairPoint> points = new List<PopulairPoint>();
            for (int i = 0; i < populairPoints.Count; i++)
            {
                if (populairPoints[i].Score > 30)
                {
                    points.Add(populairPoints[i]);
                }
            }
            return points.ToArray();
        }
    }
}
