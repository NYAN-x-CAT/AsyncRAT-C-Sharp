using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace StreamLibrary.src
{
    public unsafe class FastBitmap
    {
        public Bitmap bitmap { get; set; }
        public BitmapData bitmapData { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public PixelFormat format { get; private set; }
        public DateTime BitmapCreatedAt;
        public bool IsLocked { get; private set; }

        public int Stride
        {
            get { return bitmapData.Stride; }
        }

        public FastBitmap(Bitmap bitmap, PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    break;
                default:
                    throw new NotSupportedException(format + " is not supported.");
            }

            this.bitmap = bitmap;
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
            this.format = format;
            Lock();
            BitmapCreatedAt = DateTime.Now;
        }

        public FastBitmap(Bitmap bitmap)
        {
            this.format = bitmap.PixelFormat;

            switch (format)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    break;
                default:
                    throw new NotSupportedException(format + " is not supported.");
            }

            this.bitmap = bitmap;
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
            this.format = format;
            Lock();
            BitmapCreatedAt = DateTime.Now;
        }

        public Color GetPixel(int x, int y)
        {
            byte* position = (byte*)bitmapData.Scan0.ToPointer();
            position += CalcOffset(x, y);

            byte A = position[3];
            byte R = position[2];
            byte G = position[1];
            byte B = position[0];
            return Color.FromArgb(A, R, G, B);
        }

        public void SetPixel(int x, int y, Color color)
        {
            byte* position = (byte*)bitmapData.Scan0.ToPointer();
            position += CalcOffset(x, y);

            position[3] = color.A;
            position[2] = color.R;
            position[1] = color.G;
            position[0] = color.B;
        }

        public Color GetPixel(int x, int y, byte[] ImgData)
        {
            long offset = CalcOffset(x, y) + 4;
            if (offset + 4 < ImgData.Length)
            {
                byte R = ImgData[offset];
                byte G = ImgData[offset + 1];
                byte B = ImgData[offset + 2];
                return Color.FromArgb(255, R, G, B);
            }
            return Color.FromArgb(255, 0, 0, 0);
        }
        public void SetPixel(int x, int y, Color color, byte[] ImgData)
        {
            long offset = CalcOffset(x, y) + 4;
            if (offset + 4 < ImgData.Length)
            {
                ImgData[offset] = color.R;
                ImgData[offset + 1] = color.G;
                ImgData[offset + 2] = color.B;
                ByteArrayToBitmap(ImgData);
            }
        }

        public void DrawRectangle(Point begin, Point end, Color color)
        {
            for (int x = begin.X; x < end.X; x++)
            {
                for (int y = begin.Y; y < end.Y; y++)
                {
                    SetPixel(x, y, color);
                }
            }
        }

        public Int64 CalcOffset(int x, int y)
        {
            switch (format)
            {
                case PixelFormat.Format32bppArgb:
                    return (y * bitmapData.Stride) + (x * 4);
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return (y * bitmapData.Stride) + (x * 3);
                case PixelFormat.Format8bppIndexed:
                    return (y * bitmapData.Stride) + x;
                case PixelFormat.Format4bppIndexed:
                    return (y * bitmapData.Stride) + (x / 2);
                case PixelFormat.Format1bppIndexed:
                    return (y * bitmapData.Stride) + (x * 8);
            }
            return 0;
        }

        public static int CalcImageOffset(int x, int y, PixelFormat format, int width)
        {
            switch (format)
            {
                case PixelFormat.Format32bppArgb:
                    return (y * (width * 4)) + (x * 4);
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return (y * (width * 3)) + (x * 3);
                case PixelFormat.Format8bppIndexed:
                    return (y * width) + x;
                case PixelFormat.Format4bppIndexed:
                    return (y * (width / 2)) + (x / 2);
                case PixelFormat.Format1bppIndexed:
                    return (y * (width * 8)) + (x * 8);
                default:
                    throw new NotSupportedException(format + " is not supported.");
            }
        }

        public void ScanPixelDuplicates(Point BeginPoint, ref Point EndPoint, ref Color RetColor)
        {
            Color curColor = GetPixel(BeginPoint.X, BeginPoint.Y);
            for (int x = BeginPoint.X; x < this.Width; x++)
            {
                Color prevColor = GetPixel(x, BeginPoint.Y);

                if (curColor.R != prevColor.R ||
                    curColor.G != prevColor.G ||
                    curColor.B != prevColor.B)
                {
                    EndPoint = new Point(x, BeginPoint.Y);
                    RetColor = curColor;
                    return;
                }
            }

            EndPoint = new Point(this.Width, BeginPoint.Y);
            RetColor = curColor;
        }

        public void Unlock()
        {
            if (IsLocked)
            {
                bitmap.UnlockBits(bitmapData);
                IsLocked = false;
            }
        }
        public void Lock()
        {
            if (!IsLocked)
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, format);
                IsLocked = true;
            }
        }

        public byte[] ToByteArray()
        {
            int bytes = Math.Abs(bitmapData.Stride) * Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(bitmapData.Scan0.ToInt32()), rgbValues, 0, bytes);
            return rgbValues;
        }

        public void ByteArrayToBitmap(byte[] data)
        {
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
        }

        public void Dispose()
        {
            if (bitmap != null)
            {
                try { bitmap.UnlockBits(bitmapData); }
                catch { }
                try { bitmap.Dispose(); }
                catch { }
                try
                {
                    bitmap = null;
                    bitmapData = null;
                }
                catch { }
            }
        }

        /// <summary> Get the byte points where to read from in a byte array </summary>
        /// <param name="beginPoint">The beginning of the X, Y</param>
        /// <param name="endPoint">The end of the X, Y</param>
        /// <param name="ImgSize">The size of the image</param>
        /// <param name="SlicePieces">Slice the byte points into pieces to get the byte points faster</param>
        public static ArrayOffset[] GetBytePoints(Point beginPoint, Point endPoint, Size ImgSize, PixelFormat format)
        {
            List<ArrayOffset> offsets = new List<ArrayOffset>();

            for (int y = beginPoint.Y; y < endPoint.Y; y++)
            {
                int BeginOffset = (int)FastBitmap.CalcImageOffset(beginPoint.X, y, format, ImgSize.Width);//(y * ImgSize.Width * 4) + (beginPoint.X * 4);
                int EndOffset = (int)FastBitmap.CalcImageOffset(endPoint.X, y, format, ImgSize.Width);//(y * ImgSize.Width * 4) + (endPoint.X * 4);

                switch (format)
                {
                    case PixelFormat.Format32bppArgb:
                        {
                            if (EndOffset + ((endPoint.X - beginPoint.X) * 4) >= (ImgSize.Width * ImgSize.Height) * 4)
                                break;
                            offsets.Add(new ArrayOffset(BeginOffset, EndOffset, ((endPoint.X - beginPoint.X) * 4), beginPoint.X, y, (endPoint.X - beginPoint.X), 1));
                            break;
                        }
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        {
                            if (EndOffset + ((endPoint.X - beginPoint.X) * 3) >= (ImgSize.Width * ImgSize.Height) * 3)
                                break;
                            offsets.Add(new ArrayOffset(BeginOffset, EndOffset, ((endPoint.X - beginPoint.X) * 3), beginPoint.X, y, (endPoint.X - beginPoint.X), 1));
                            break;
                        }
                    case PixelFormat.Format8bppIndexed:
                        {
                            if (EndOffset + ((endPoint.X - beginPoint.X)) >= (ImgSize.Width * ImgSize.Height))
                                break;
                            offsets.Add(new ArrayOffset(BeginOffset, EndOffset, ((endPoint.X - beginPoint.X)), beginPoint.X, y, (endPoint.X - beginPoint.X), 1));
                            break;
                        }
                    case PixelFormat.Format4bppIndexed:
                        {
                            if (EndOffset + ((endPoint.X - beginPoint.X) / 2) >= (ImgSize.Width * ImgSize.Height) / 2)
                                break;
                            offsets.Add(new ArrayOffset(BeginOffset, EndOffset, ((endPoint.X - beginPoint.X) / 2), beginPoint.X, y, (endPoint.X - beginPoint.X), 1));
                            break;
                        }
                    case PixelFormat.Format1bppIndexed:
                        {
                            if (EndOffset + ((endPoint.X - beginPoint.X) * 8) >= (ImgSize.Width * ImgSize.Height) * 8)
                                break;
                            offsets.Add(new ArrayOffset(BeginOffset, EndOffset, ((endPoint.X - beginPoint.X) * 8), beginPoint.X, y, (endPoint.X - beginPoint.X), 1));
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException(format + " is not supported.");
                        }
                }

            }
            return offsets.ToArray();
        }

        /// <summary> Get the byte points in 2D </summary>
        /// <param name="beginPoint">The beginning of the X, Y</param>
        /// <param name="endPoint">The end of the X, Y</param>
        /// <param name="ImgSize">The size of the image</param>
        /// <param name="SlicePieces">Slice the byte points into pieces to get the byte points faster</param>
        public static ArrayOffset[,][] Get2DBytePoints(Point beginPoint, Point endPoint, Size ImgSize, int SlicePieces, PixelFormat format)
        {
            int Width = endPoint.X - beginPoint.X;
            int Height = endPoint.Y - beginPoint.Y;

            float Wsize = ((float)Width / (float)SlicePieces);
            float Hsize = ((float)Height / (float)SlicePieces);

            //+1 just to make sure we are not going outside the array
            if (Wsize - (int)Wsize > 0.0F) Wsize += 1.0F;
            if (Hsize - (int)Hsize > 0.0F) Hsize += 1.0F;

            ArrayOffset[,][] ImageArrayOffsets = new ArrayOffset[(int)Hsize, (int)Wsize][];
            Point tmp = new Point(0, 0);
            for (int y = beginPoint.Y; y < Height; y += SlicePieces)
            {
                for (int x = beginPoint.X; x < Width; x += SlicePieces)
                {
                    ImageArrayOffsets[tmp.Y, tmp.X] = FastBitmap.GetBytePoints(new Point(x, y), new Point(x + SlicePieces, y + SlicePieces), ImgSize, format);
                    tmp.X++;
                }
                tmp.X = 0;
                tmp.Y++;
            }
            return ImageArrayOffsets;
        }
    }

    public class ArrayOffset
    {
        public int BeginOffset { get; private set; }
        public int EndOffset { get; private set; }
        public int Stride { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ArrayOffset(int begin, int end, int Stride, int x, int y, int width, int height)
        {
            this.BeginOffset = begin;
            this.EndOffset = end;
            this.Stride = Stride;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }
}