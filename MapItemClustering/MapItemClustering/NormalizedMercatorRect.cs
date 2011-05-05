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
        private Point _Centroid;
        private double _Width;
        private double _Height;

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

            _Centroid = rect.Centroid();
            _Width = rect.Width;
            _Height = rect.Height;
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
        /// Gets the centroid.
        /// </summary>
        public Point Centroid
        {
            get
            {
                return _Centroid;
            }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        public double Width
        {
            get { return _Width; }
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        public double Height
        {
            get { return _Height; }
        }

        /// <summary>
        /// Gets the west boundary of the rect.
        /// </summary>
        public double West
        {
            get { return _Rects.Length > 1 ? _Rects[1].Left : _Rects[0].Left; }
        }

        /// <summary>
        /// Gets the east boundary of the rect.
        /// </summary>
        public double East
        {
            get { return _Rects[0].Right; }
        }

        /// <summary>
        /// Gets the north boundary of the rect.
        /// </summary>
        public double North
        {
            get { return _Rects[0].Top; }
        }

        /// <summary>
        /// Gets the south boundary of the rect.
        /// </summary>
        public double South
        {
            get { return _Rects[0].Bottom; }
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

        /// <summary>
        /// Returns whether this rect contains the other.
        /// </summary>
        public bool Contains(NormalizedMercatorRect other)
        {
            if (_Rects.Length == 1)
            {
                return other._Rects.Length == 1 && _Rects[0].Contains(other._Rects[0]);
            }
            else
            {
                if (other._Rects.Length == 2)
                {
                    return _Rects[0].Contains(other._Rects[0]) && _Rects[1].Contains(other._Rects[1]);
                }
                else
                {
                    return _Rects[0].Contains(other._Rects[0]) || _Rects[1].Contains(other._Rects[0]);
                }
            }
        }

        /// <summary>
        /// Unions this rect with the other.
        /// </summary>
        public void Union(NormalizedMercatorRect other)
        {
            if (_Rects.Length == 1 && other._Rects.Length == 1)
            {
                _Rects[0].Union(other._Rects[0]);
            }
            else if (_Rects.Length == 2 && other._Rects.Length == 2)
            {
                _Rects[0].Union(other._Rects[0]);
                _Rects[1].Union(other._Rects[1]);
            }
            else
            {
                Rect l, r, c;

                if (_Rects.Length == 1)
                {
                    l = other._Rects[0];
                    r = other._Rects[1];
                    c = _Rects[0];
                }
                else
                {
                    l = _Rects[0];
                    r = _Rects[1];
                    c = other._Rects[0];
                }

                if (HorizontalDistance(l, c) < HorizontalDistance(r, c))
                {
                    l.Union(c);
                }
                else
                {
                    r.Union(c);
                }

                double top = Math.Min(l.Top, r.Top);
                double bottom = Math.Max(l.Bottom, r.Bottom);

                l.Y = r.Y = top;
                l.Height = r.Height = bottom - top;

                _Rects = new Rect[] { l, r };

                UpdateDimsAndCentroid();
            }

            Debug.Assert(_Rects.Length == 1 || _Rects[0].Top == _Rects[1].Top);
            Debug.Assert(_Rects.Length == 1 || _Rects[0].Bottom == _Rects[1].Bottom);
        }

        /// <summary>
        /// Returns the square of the distance from this rect to the other.
        /// </summary>
        public double DistanceSquared(Rect rect)
        {
            if (_Rects.Length == 1)
            {
                return _Rects[0].DistanceFrom(rect);
            }
            else
            {
                return Math.Min(_Rects[0].DistanceFrom(rect), _Rects[1].DistanceFrom(rect));
            }
        }

        /// <summary>
        /// Returns the square of the distance from this rect to the other.
        /// </summary>
        public double DistanceSquared(NormalizedMercatorRect other)
        {
            if (other._Rects.Length == 1)
            {
                return DistanceSquared(other._Rects[0]);
            }
            else
            {
                return Math.Min(
                    Math.Min(_Rects[0].DistanceFrom(other._Rects[0]), _Rects[0].DistanceFrom(other._Rects[1])),
                    Math.Min(_Rects[1].DistanceFrom(other._Rects[0]), _Rects[1].DistanceFrom(other._Rects[1])));
            }
        }

        /// <summary>
        /// Returns the distance between two rects in the horizontal axis.
        /// </summary>
        private static double HorizontalDistance(Rect a, Rect b)
        {
            return Math.Abs(Math.Abs(a.X - b.X) - 0.5 * (a.Width + b.Width));
        }

        private void InitRects(Point nw, Point se)
        {
            Debug.Assert(new Rect(0, 0, 1, 1).Contains(nw));
            Debug.Assert(new Rect(0, 0, 1, 1).Contains(se));
            Debug.Assert(nw.Y < se.Y);

            // Check if the rect wraps around to the other side. If so, we need two
            // rectangles to represent its area.
            if (nw.X < se.X)
            {
                _Rects = new Rect[] { new Rect(nw, se) };

            }
            else
            {
                _Rects = new Rect[] 
                { 
                    new Rect(new Point(0, nw.Y), new Point(se.X, se.Y)),
                    new Rect(new Point(nw.X, nw.Y), new Point(1, se.Y))
                };
            }

            UpdateDimsAndCentroid();
        }

        private void UpdateDimsAndCentroid()
        {
            _Height = _Rects[0].Height;

            _Width = 0;
            for (int i = 0; i < _Rects.Length; i++)
            {
                _Width += _Rects[i].Width;
            }

            if (_Rects.Length == 1)
            {
                _Centroid = _Rects[0].Centroid();
            }
            else
            {
                Debug.Assert(_Rects[1].Left > _Rects[0].Left);

                _Centroid = new Point(
                    _Rects[1].Left + 0.5 * _Width,
                    _Rects[1].Top + 0.5 * _Height);

                if (_Centroid.X > 1)
                {
                    _Centroid.X = _Centroid.X - 1;
                }
            }

            Debug.Assert(_Centroid.X >= 0 && Centroid.X <= 1);
            Debug.Assert(_Centroid.Y >= 0 && Centroid.Y <= 1);
        }
    }
}
