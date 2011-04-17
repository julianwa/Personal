using System.Diagnostics;
using System.Windows;
using Microsoft.Maps.MapControl;
using System;

namespace MapItemClustering
{
    public class NormalizedMercatorRect
    {
        public Rect[] _Rects;

        public NormalizedMercatorRect(LocationRect locationRect)
        {
            Point nw = locationRect.Northwest.ToNormalizedMercator();
            Point se = locationRect.Southeast.ToNormalizedMercator();

            InitRects(nw, se);
        }

        public NormalizedMercatorRect(Point center, double width, double height)
        {
            if (!new Rect(0, 0, 1, 1).Contains(center))
            {
                throw new ArgumentException("Center must be in Rect(0,0,1,1)");
            }

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Width and height must be positive.");
            }

            Point nw = new Point(center.X - width / 2, MapMath.Clamp(center.Y - height / 2, 0, 1));
            Point se = new Point(center.X + width / 2, MapMath.Clamp(center.Y + height / 2, 0, 1));

            if (width >= 1)
            {
                nw.X = 0;
                se.X = 1;
            }
            else
            {
                nw.X = nw.X >= 0 ? (nw.X % 1) : (1 + (nw.X % 1));
                se.X = se.X >= 0 ? (se.X % 1) : (1 + (se.X % 1));
            }

            InitRects(nw, se);
        }

        public bool Intersects(NormalizedMercatorRect other)
        {
            for (int i = 0; i < _Rects.Length; i++)
            {
                for (int j = 0; j < other._Rects.Length; j++)
                {
                    Rect r = _Rects[i];
                    r.Intersect(other._Rects[j]);

                    if (!r.IsEmpty)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void InitRects(Point nw, Point se)
        {
            Debug.Assert(new Rect(0, 0, 1, 1).Contains(nw));
            Debug.Assert(new Rect(0, 0, 1, 1).Contains(se));
            Debug.Assert(nw.Y < se.Y);

            // Check if the rect wraps around to the other side. If so, we need two
            // rectangles to represent its area.
            _Rects = nw.X < se.X ?
                new Rect[] { new Rect(nw, se) } :
                new Rect[] { 
                    new Rect(new Point(0, nw.Y), new Point(se.X, se.Y)),
                    new Rect(new Point(nw.X, nw.Y), new Point(1, se.Y))
                };
        }
    }
}
