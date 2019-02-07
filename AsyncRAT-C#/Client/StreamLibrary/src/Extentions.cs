using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace StreamLibrary.src
{
    public static unsafe class Extensions
    {
        public static SortedList<int, SortedList<int, Rectangle>> RectanglesTo2D(this Rectangle[] rects)
        {
            SortedList<int, SortedList<int, Rectangle>> Rects = new SortedList<int, SortedList<int, Rectangle>>();
            for (int i = 0; i < rects.Length; i++)
            {
                if (!Rects.ContainsKey(rects[i].Y))
                    Rects.Add(rects[i].Y, new SortedList<int, Rectangle>());

                if (!Rects[rects[i].Y].ContainsKey(rects[i].X))
                    Rects[rects[i].Y].Add(rects[i].X, rects[i]);
            }
            return Rects;
        }

        public static SortedList<int, SortedList<int, Rectangle>> Rectangle2DToRows(this SortedList<int, SortedList<int, Rectangle>> Rects)
        {
            SortedList<int, SortedList<int, Rectangle>> RectRows = new SortedList<int, SortedList<int, Rectangle>>();

            for (int i = 0; i < Rects.Values.Count; i++)
            {
                if (!RectRows.ContainsKey(Rects.Values[i].Values[0].Y))
                {
                    RectRows.Add(Rects.Values[i].Values[0].Y, new SortedList<int, Rectangle>());
                }
                if (!RectRows[Rects.Values[i].Values[0].Y].ContainsKey(Rects.Values[i].Values[0].X))
                {
                    RectRows[Rects.Values[i].Values[0].Y].Add(Rects.Values[i].Values[0].X, Rects.Values[i].Values[0]);
                }

                Rectangle EndRect = Rects.Values[i].Values[0];
                for (int x = 1; x < Rects.Values[i].Values.Count; x++)
                {
                    Rectangle CurRect = Rects.Values[i].Values[x];
                    Rectangle tmpRect = RectRows[EndRect.Y].Values[RectRows[EndRect.Y].Count - 1];
                    if (tmpRect.IntersectsWith(new Rectangle(CurRect.X - 1, CurRect.Y, CurRect.Width, CurRect.Height)))
                    {
                        RectRows[EndRect.Y][tmpRect.X] = new Rectangle(tmpRect.X, tmpRect.Y, tmpRect.Width + EndRect.Width, tmpRect.Height);
                        EndRect = Rects.Values[i].Values[x];
                    }
                    else
                    {
                        EndRect = Rects.Values[i].Values[x];
                        RectRows[Rects.Values[i].Values[0].Y].Add(EndRect.X, EndRect);
                    }
                }
            }
            return RectRows;
        }
    }
}
