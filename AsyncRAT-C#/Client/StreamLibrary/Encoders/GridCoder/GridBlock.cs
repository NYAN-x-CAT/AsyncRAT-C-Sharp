using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace StreamLibrary.Encoders.GridCoder
{
    internal class GridBlock
    {
        public Rectangle Rect { get; private set; }
        public ulong Hash { get; private set; }
        private GridEncoder encoder;

        public GridBlock(Rectangle Rect, GridEncoder encoder)
        {
            this.encoder = encoder;
            this.Rect = Rect;
            CalculateHash();
        }

        public void CalculateHash()
        {
            
        }
    }
}