using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace StreamLibrary.src
{
    public unsafe class SimpleBitmap
    {
        private object ProcessingLock = new object();

        public SimpleBitmapInfo Info { get; internal set; }
        public bool Locked { get { return Scan0 == IntPtr.Zero ? false : true; } }
        public IntPtr Scan0 { get; internal set; }
        public int Scan0_int { get; internal set; }
        public BitmapData bitmapData { get; internal set; }
        public Bitmap bitMap { get; set; }

        public class SimpleBitmapInfo
        {
            public SimpleBitmapInfo()
            {
                Clear();
            }
            public SimpleBitmapInfo(BitmapData data)
            {
                Load(data);
            }
            public int Stride { get; protected set; }
            public int PixelSize { get; protected set; }
            public int Width { get; protected set; }
            public int Height { get; protected set; }

            public int TotalSize { get; protected set; }

            internal void Clear()
            {
                Stride = 0; PixelSize = 0; Width = 0; Height = 0; TotalSize = 0;
            }
            internal void Load(BitmapData data)
            {
                Width = data.Width; Height = data.Height; Stride = data.Stride;

                PixelSize = Math.Abs(data.Stride) / data.Width;
                TotalSize = data.Width * data.Height * PixelSize;
            }
        }

        public static bool Compare(Rectangle block, int ptr1, int ptr2, SimpleBitmapInfo sharedInfo)
        {
            int calc = 0;
            int WidthSize = block.Width * sharedInfo.PixelSize;

            calc = (block.Y) * sharedInfo.Stride + block.X * sharedInfo.PixelSize;

            for (int i = 0; i < block.Height; i++)
            {
                if (NativeMethods.memcmp((byte*)(ptr1 + calc), (byte*)(ptr2 + calc), (uint)WidthSize) != 0)
                    return false;
                calc += sharedInfo.Stride;
            }
            return true;
        }
        public static bool Compare(int y, int rowsize, int ptr1, int ptr2, SimpleBitmapInfo sharedInfo)
        {
            int calc = 0;
            int Size = sharedInfo.Width * sharedInfo.PixelSize * rowsize;

            calc = y * sharedInfo.Stride;

            if (NativeMethods.memcmp((byte*)(ptr1 + calc), (byte*)(ptr2 + calc), (uint)Size) != 0)
                return false;
            return true;
        }
        public static bool FastCompare(int offset, int size, int ptr1, int ptr2, SimpleBitmapInfo sharedInfo)
        {
            if (NativeMethods.memcmp((byte*)(ptr1 + offset), (byte*)(ptr2 + offset), (uint)size) != 0)
                return false;
            return true;
        }

        public unsafe void CopyBlock(Rectangle block, ref byte[] dest)
        {
            int calc = 0;
            int WidthSize = block.Width * Info.PixelSize;
            int CopyOffset = 0;
            int scan0 = Scan0.ToInt32();
            int destSize = WidthSize * block.Height;

            if (dest == null || dest.Length != destSize)
                dest = new byte[destSize];

            calc = (block.Y) * Info.Stride + block.X * Info.PixelSize;

            fixed (byte* ptr = dest)
            {
                for (int i = 0; i < block.Height; i++)
                {
                    NativeMethods.memcpy(new IntPtr(ptr + CopyOffset), new IntPtr(scan0 + calc), (uint)WidthSize);
                    calc += Info.Stride;
                    CopyOffset += WidthSize;
                }
            }
        }

        public SimpleBitmap()
        {
            Scan0 = IntPtr.Zero;
            bitmapData = null;
            bitMap = null;

            Info = new SimpleBitmapInfo();
        }
        public SimpleBitmap(Bitmap bmp)
        {
            this.bitMap = bmp;
        }

        public void Lock()
        {
            if (Locked)
                throw new Exception("Already locked");

            lock (ProcessingLock)
            {
                bitmapData = bitMap.LockBits(new Rectangle(0, 0, bitMap.Width, bitMap.Height), ImageLockMode.ReadWrite, bitMap.PixelFormat);

                Info = new SimpleBitmapInfo(bitmapData);

                Scan0 = bitmapData.Scan0;
                Scan0_int = Scan0.ToInt32();
            }
        }
        public void Unlock()
        {
            if (!Locked)
                throw new Exception("Nothing to unlock");

            lock (ProcessingLock)
            {
                Scan0 = IntPtr.Zero;
                Scan0_int = 0;

                Info.Clear();
                bitMap.UnlockBits(bitmapData);
                bitmapData = null;
            }
        }
        public unsafe void PlaceBlockAtRectange(byte[] block, Rectangle loc)
        {
            int CopySize = Info.PixelSize * loc.Width;
            int OffsetX = loc.X * Info.PixelSize;
            int TotalCopied = 0;

            fixed (byte* ptr = block)
            {
                for (int i = 0; i < loc.Height; i++)
                {
                    NativeMethods.memcpy(new IntPtr(Scan0_int + ((loc.Y + i) * Info.Stride + OffsetX)), new IntPtr(ptr + TotalCopied), (uint)CopySize);
                    TotalCopied += CopySize;
                }
            }
        }
        public void Dispose(bool disposeBitmap = false)
        {
            if (Locked)
                Unlock();

            if (disposeBitmap)
                bitMap.Dispose();

            bitMap = null;
        }
    }
}