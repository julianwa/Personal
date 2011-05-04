using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    /// <summary>
    /// A rectangle in normalized mercator coordinates, where longitude [-180,+180] and 
    /// latitude [MercatorLatitudeLimit, -MercatorLatitudeLimit] both map to [0,1].
    /// </summary>
    public class NormalizedMercatorRect
    {
        private Rect[] _Rects;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedMercatorRect"/> class.
        /// </summary>
        /// <param name="rect">The a normal rectangle in normalized mercator coordinates.</param>
        public NormalizedMercatorRect(Rect rect)
        {
            if (!new Rect(0, 0, 1, 1).Contains(rect))
            {
                throw new ArgumentException("rect");
            }

            _Rects = new Rect[] { rect };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedMercatorRect"/> class.
        /// </summary>
        /// <param name="locationRect">The location rect.</param>
        public NormalizedMercatorRect(LocationRect locationRect)
        {
            Point nw = locationRect.Northwest.ToNormalizedMercator();
            Point se = locationRect.Southeast.ToNormalizedMercator();

            InitRects(nw, se);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedMercatorRect"/> class.
        /// </summary>
        /// <param name="center">The center in normalized mercator.</param>
        /// <param name="width">The width in normalized mercator.</param>
        /// <param name="height">The height in normalized mercator.</param>
        public NormalizedMercatorRect(Point center, double width, double height)
        {
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

        /// <summary>
        /// Returns whether the two rectangle intersect.
        /// </summary>
        /// <param name="other">The other rect.</param>
        /// <returns></returns>
        public bool Intersects(NormalizedMercatorRect other)
        {
            for (int i = 0; i < _Rects.Length; i++)
            {
                for (int j = 0; j < other._Rects.Length; j++)
                {
                    if (_Rects[i].Intersects(other._Rects[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether the two rectangles intersect.
        /// </summary>
        /// <param name="other">The other rectangle, specified in normalized mercator coordinates.</param>
        /// <returns></returns>
        public bool Intersects(Rect other)
        {
            for (int i = 0; i < _Rects.Length; i++)
            {
                if (_Rects[i].Intersects(other))
                {
                    return true;
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
