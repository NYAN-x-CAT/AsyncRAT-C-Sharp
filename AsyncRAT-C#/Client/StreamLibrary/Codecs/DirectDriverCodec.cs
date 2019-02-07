using StreamLibrary.src;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace StreamLibrary.Codecs
{
    public class DirectDriverCodec
    {
        private Bitmap decodedBitmap;

        private byte[] EncodeBuffer;
        private PixelFormat EncodedFormat;
        private int EncodedWidth;
        private int EncodedHeight;
        private JpgCompression jpgCompression;

        public DirectDriverCodec(int Quality)
        {
            jpgCompression = new JpgCompression(Quality);
        }

        public unsafe void CodeImage(IntPtr Scan0, Rectangle[] Changes, Size ImageSize, PixelFormat Format, Stream outStream)
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
                        temp = jpgCompression.Compress(TmpBmp);
                    }

                    outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                    outStream.Write(temp, 0, temp.Length);
                    NativeMethods.memcpy(new IntPtr(ptr), Scan0, (uint)RawLength);
                }
                return;
            }

            long oldPos = outStream.Position;
            outStream.Write(new byte[4], 0, 4);
            int TotalDataLength = 0;

            for (int i = 0; i < Changes.Length; i++)
            {
                Rectangle rect = Changes[i];
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
                jpgCompression.Compress(TmpBmp, ref outStream);
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

        public Bitmap DecodeData(Stream inStream)
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
            return decodedBitmap;

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