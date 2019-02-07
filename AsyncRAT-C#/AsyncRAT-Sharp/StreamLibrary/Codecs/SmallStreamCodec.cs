using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace StreamLibrary.Codecs
{
    public class SmallStreamCodec : IVideoCodec
    {
        public override event IVideoCodec.VideoCodeProgress onVideoStreamCoding;
        public override event IVideoCodec.VideoDecodeProgress onVideoStreamDecoding;
        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        public override int BufferCount
        {
            get { return 1; }
        }
        public override ulong CachedSize
        {
            get;
            internal set;
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.AutoDispose | CodecOption.RequireSameSize; }
        }

        public Size CheckBlock { get; set; }
        public SimpleBitmap LastFrame { get; set; }
        private object ImageProcessLock = new object();
        private Bitmap decodedBitmap;

        /// <summary>
        /// Initialize a new object of SmallStreamCodec
        /// </summary>
        /// <param name="ImageQuality">The image quality 0-100%</param>
        public SmallStreamCodec(int ImageQuality = 100)
            : base(ImageQuality)
        {
            CheckBlock = new Size(20, 1);
        }

        private void SetLastFrame(ref Bitmap bmp)
        {
            SetLastFrame(new SimpleBitmap(bmp));
        }
        private void SetLastFrame(SimpleBitmap bmp)
        {
            lock (ImageProcessLock)
            {
                if (LastFrame != null && LastFrame.Locked)
                    LastFrame.Dispose(true);
                LastFrame = bmp;
            }
        }

        /// <summary>
        /// Encode the image
        /// </summary>
        /// <param name="bitmap">The image you want to encode.</param>
        /// <param name="outStream">The output stream</param>
        public override unsafe void CodeImage(Bitmap bitmap, Stream outStream)
        {
            lock (ImageProcessLock)
            {
                if (!outStream.CanWrite)
                    throw new Exception("Must have access to Write in the Stream");

                if (LastFrame == null)
                {
                    byte[] temp = base.jpgCompression.Compress(bitmap);
                    outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                    outStream.Write(temp, 0, temp.Length);
                    SetLastFrame(ref bitmap);
                    return;
                }

                long oldPos = outStream.Position;
                outStream.Write(new byte[4], 0, 4);
                int TotalDataLength = 0;

                List<byte[]> updates = new List<byte[]>();
                SimpleBitmap sbBmp = new SimpleBitmap(bitmap);
                MemoryStream ms = new MemoryStream();
                byte[] buffer = null;

                if (!LastFrame.Locked)
                    LastFrame.Lock();

                sbBmp.Lock();

                if (sbBmp.Info.PixelSize != LastFrame.Info.PixelSize)
                    throw new Exception("PixelFormat is not equal to previous Bitmap");

                if (LastFrame.Info.Width != sbBmp.Info.Width || LastFrame.Info.Height != sbBmp.Info.Height)
                {
                    sbBmp.Unlock();
                    throw new Exception("Bitmap width/height are not equal to previous bitmap");
                }

                List<Rectangle> Blocks = new List<Rectangle>();
                int index = 0;

                int y = 0;
                int x = 0;

                Size s = new Size(bitmap.Width, CheckBlock.Height);
                Size lastSize = new Size(bitmap.Width % CheckBlock.Width, bitmap.Height % CheckBlock.Height);

                int lasty = bitmap.Height - lastSize.Height;
                int lastx = bitmap.Width - lastSize.Width;

                Rectangle cBlock = new Rectangle();

                s = new Size(bitmap.Width, s.Height);
                while (y != bitmap.Height)
                {
                    if (y == lasty)
                        s = new Size(bitmap.Width, lastSize.Height);

                    cBlock = new Rectangle(0, y, bitmap.Width, s.Height);

                    if (onCodeDebugScan != null)
                        onCodeDebugScan(cBlock);

                    if (!SimpleBitmap.Compare(cBlock, LastFrame.Scan0_int, sbBmp.Scan0_int, sbBmp.Info))
                    //if (!SimpleBitmap.Compare(y, s.Height, LastFrame.Scan0_int, sbBmp.Scan0_int, sbBmp.Info))
                    {
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

                List<Rectangle> finalUpdates = new List<Rectangle>();
                for (int i = 0; i < Blocks.Count; i++)
                {
                    s = new Size(CheckBlock.Width, Blocks[i].Height);
                    x = 0;
                    while (x != bitmap.Width)
                    {
                        if (x == lastx)
                            s = new Size(lastSize.Width, Blocks[i].Height);

                        cBlock = new Rectangle(x, Blocks[i].Y, s.Width, Blocks[i].Height);

                        if (onCodeDebugScan != null)
                            onCodeDebugScan(cBlock);

                        if (!SimpleBitmap.Compare(cBlock, sbBmp.Scan0_int, LastFrame.Scan0_int, sbBmp.Info))
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

                for (int i = 0; i < finalUpdates.Count; i++)
                {
                    Rectangle rect = finalUpdates[i];
                    sbBmp.CopyBlock(rect, ref buffer);

                    fixed (byte* ptr = buffer)
                    {
                        using (Bitmap TmpBmp = new Bitmap(rect.Width, rect.Height, rect.Width * LastFrame.Info.PixelSize, LastFrame.bitmapData.PixelFormat, new IntPtr(ptr)))
                        {
                            buffer = base.jpgCompression.Compress(TmpBmp);
                        }
                    }
                    
                    outStream.Write(BitConverter.GetBytes(rect.X), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Y), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Width), 0, 4);
                    outStream.Write(BitConverter.GetBytes(rect.Height), 0, 4);
                    outStream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
                    outStream.Write(buffer, 0, buffer.Length);
                    TotalDataLength += buffer.Length + (4 * 5);
                }

                outStream.Position = oldPos;
                outStream.Write(BitConverter.GetBytes(TotalDataLength), 0, 4);

                Blocks.Clear();
                SetLastFrame(sbBmp);
                ms.Close();
                ms.Dispose();
            }
        }

        /// <summary>
        /// Decode the video stream
        /// </summary>
        /// <param name="inStream">The input stream</param>
        /// <returns>The image that has been decoded</returns>
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
                DataSize -= buffer.Length + (4 * 5);
            }
            g.Dispose();
            return decodedBitmap;
        }
    }
}