using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace StreamLibrary.Codecs
{
    public class SmallCachedStreamCodec : IVideoCodec
    {
        public override event IVideoCodec.VideoCodeProgress onVideoStreamCoding;
        public override event IVideoCodec.VideoDecodeProgress onVideoStreamDecoding;
        public override event IVideoCodec.VideoDebugScanningDelegate onCodeDebugScan;
        public override event IVideoCodec.VideoDebugScanningDelegate onDecodeDebugScan;

        SortedList<int, byte[]> codeCached;
        SortedList<int, byte[]> decodeCached;
        public override int BufferCount
        {
            get { return codeCached.Count; }
        }
        public override ulong CachedSize
        {
            get;
            internal set;
        }

        public override CodecOption CodecOptions
        {
            get { return CodecOption.AutoDispose | CodecOption.HasBuffers | CodecOption.RequireSameSize; }
        }

        private Size CheckBlock { get; set; }
        public SimpleBitmap LastFrame { get; set; }
        private object ImageProcessLock = new object();
        private Bitmap decodedBitmap;
        private CRC32 hasher;
        public int MaxBuffers { get; private set; }

        /// <summary>
        /// Initialize a new object of SmallCachedStreamCodec
        /// </summary>
        /// <param name="MaxBuffers">The maximum amount of buffers, higher value will decrease stream size but could decrease performance</param>
        /// <param name="ImageQuality">The image quality 0-100%</param>
        public SmallCachedStreamCodec(int MaxBuffers = 5000, int ImageQuality = 100)
            : base(ImageQuality)
        {
            CheckBlock = new Size(20, 1);
            codeCached = new SortedList<int, byte[]>();
            decodeCached = new SortedList<int, byte[]>();
            hasher = new CRC32();
            this.MaxBuffers = MaxBuffers;
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

                List<CacheInfo> finalUpdates = new List<CacheInfo>();
                const int CheckHeight = 50;
                for (int i = 0; i < Blocks.Count; i++)
                {
                    s = new Size(CheckBlock.Width, Blocks[i].Height);
                    y = Blocks[i].Y;
                    lasty = (Blocks[i].Y + Blocks[i].Height);

                    while (y != lasty)
                    {
                        int ScanHeight = y + CheckHeight > lasty ? lasty - y : CheckHeight;
                        x = 0;
                        while (x != bitmap.Width)
                        {
                            if (x == lastx)
                                s = new Size(lastSize.Width, Blocks[i].Height);
                            cBlock = new Rectangle(x, y, s.Width, ScanHeight);

                            if (onCodeDebugScan != null)
                                onCodeDebugScan(cBlock);

                            if (!SimpleBitmap.Compare(cBlock, sbBmp.Scan0_int, LastFrame.Scan0_int, sbBmp.Info))
                            {
                                /*byte[] tempData = new byte[0];
                                LastFrame.CopyBlock(cBlock, ref tempData);
                                finalUpdates.Add(new CacheInfo(0, false, tempData, cBlock));*/

                                //hash it and see if exists in cache
                                hasher = new CRC32(); //re-initialize for seed
                                byte[] tempData = new byte[0];
                                LastFrame.CopyBlock(cBlock, ref tempData);
                                int hash = BitConverter.ToInt32(hasher.ComputeHash(tempData), 0);

                                if (codeCached.Count >= MaxBuffers)
                                    codeCached.RemoveAt(0);

                                if (codeCached.ContainsKey(hash))
                                {
                                    CachedSize += (ulong)tempData.Length;
                                    finalUpdates.Add(new CacheInfo(hash, true, new byte[0], cBlock));
                                }
                                else
                                {
                                    //nothing found in cache let's use the normal way
                                    codeCached.Add(hash, tempData);
                                    finalUpdates.Add(new CacheInfo(hash, false, tempData, cBlock));
                                }
                            }
                            x += s.Width;
                        }
                        y += ScanHeight;
                    }
                }

                for (int i = 0; i < finalUpdates.Count; i++)
                {
                    buffer = new byte[0];
                    Rectangle rect = finalUpdates[i].Rect;

                    if (!finalUpdates[i].isCached)
                    {
                        fixed (byte* ptr = finalUpdates[i].Data)
                        {
                            using (Bitmap TmpBmp = new Bitmap(rect.Width, rect.Height, rect.Width * LastFrame.Info.PixelSize, LastFrame.bitmapData.PixelFormat, new IntPtr(ptr)))
                            {
                                buffer = base.jpgCompression.Compress(TmpBmp);
                            }
                        }
                    }

                    outStream.WriteByte(finalUpdates[i].isCached ? (byte)1 : (byte)0);
                    outStream.Write(BitConverter.GetBytes(finalUpdates[i].Rect.X), 0, 4);
                    outStream.Write(BitConverter.GetBytes(finalUpdates[i].Rect.Y), 0, 4);
                    outStream.Write(BitConverter.GetBytes(finalUpdates[i].Rect.Width), 0, 4);
                    outStream.Write(BitConverter.GetBytes(finalUpdates[i].Rect.Height), 0, 4);
                    outStream.Write(BitConverter.GetBytes(finalUpdates[i].Hash), 0, 4);
                    outStream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
                    outStream.Write(buffer, 0, buffer.Length);
                    TotalDataLength += buffer.Length + (4 * 6) + 1;
                }

                outStream.Position = oldPos;
                outStream.Write(BitConverter.GetBytes(TotalDataLength), 0, 4);

                Blocks.Clear();
                SetLastFrame(sbBmp);
                ms.Close();
                ms.Dispose();
            }
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
            List<CacheInfo> cacheInfo = new List<CacheInfo>();
            byte[] HeaderData = new byte[(4 * 6) + 1];

            while (DataSize > 0)
            {
                inStream.Read(HeaderData, 0, HeaderData.Length);

                bool isCached = HeaderData[0] == 1;
                rect = new Rectangle(BitConverter.ToInt32(HeaderData, 1), BitConverter.ToInt32(HeaderData, 5),
                                     BitConverter.ToInt32(HeaderData, 9), BitConverter.ToInt32(HeaderData, 13));
                int Hash = BitConverter.ToInt32(HeaderData, 17);
                int UpdateLen = BitConverter.ToInt32(HeaderData, 21);

                buffer = new byte[UpdateLen];
                inStream.Read(buffer, 0, buffer.Length);

                //process update data
                if (isCached)
                {
                    //data is cached
                    if (decodeCached.ContainsKey(Hash))
                        buffer = decodeCached[Hash];
                    else
                    {

                    }
                    cacheInfo.Add(new CacheInfo(Hash, true, new byte[0], rect));
                }

                if (onDecodeDebugScan != null)
                    onDecodeDebugScan(rect);

                if (buffer.Length > 0)
                {
                    m = new MemoryStream(buffer);
                    tmp = (Bitmap)Image.FromStream(m);
                    g.DrawImage(tmp, rect.Location);

                    tmp.Dispose();
                    m.Close();
                    m.Dispose();
                }

                if (decodeCached.Count >= MaxBuffers)
                    decodeCached.RemoveAt(0);

                if(!decodeCached.ContainsKey(Hash))
                    this.decodeCached.Add(Hash, buffer);
                DataSize -= UpdateLen + HeaderData.Length;
            }

            int CachedSize = 0;
            foreach(CacheInfo inf in cacheInfo)
            {
                CachedSize += (inf.Rect.Width * 4) * inf.Rect.Height;


            }
            Console.WriteLine(cacheInfo.Count + ", " + CachedSize);
            g.Dispose();
            return decodedBitmap;
        }

        private class CacheInfo
        {
            public bool isCached = false;
            public byte[] Data;
            public int Hash;
            public Rectangle Rect;

            public CacheInfo(int Hash, bool isCached, byte[] Data, Rectangle Rect)
            {
                this.Hash = Hash;
                this.isCached = isCached;
                this.Data = Data;
                this.Rect = Rect;
            }

            /*public unsafe void CreateHashList(SimpleBitmap sBmp, Size CheckBlock, SmallCachedStreamCodec codec)
            {
                if (ScanRects.Count > 0)
                {
                    int scanX = ScanRects[0].X;
                    for (int i = 0; i < ScanRects.Count; i++)
                    {
                        int scanWidth = ScanRects[i].Width > CheckBlock.Width ? CheckBlock.Width : ScanRects[i].Width;
                        Rectangle rect = ScanRects[i];
                        rect.Width = scanWidth;
                        rect.X = scanX;
                        byte[] buffer = new byte[0];
                        sBmp.CopyBlock(rect, ref buffer);

                        int hash = BitConverter.ToInt32(new CRC32().ComputeHash(buffer), 0);

                        if (!HashList.ContainsKey(hash))
                            HashList.Add(hash, rect);

                        //fixed (byte* ptr = buffer)
                        //{
                        //    using (Bitmap TmpBmp = new Bitmap(rect.Width, rect.Height, rect.Width * sBmp.Info.PixelSize, sBmp.bitmapData.PixelFormat, new IntPtr(ptr)))
                        //    {
                        //        buffer = codec.lzwCompression.Compress(TmpBmp);
                        //
                        //    }
                        //}

                        scanX += scanWidth;
                    }
                }
            }*/
        }
    }
}