using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts the Location to normalized mercator, where longitude [-180,+180] and 
        /// latitude [MercatorLatitudeLimit, -MercatorLatitudeLimit] both map to [0,1].
        /// </summary>
        /// <param name="location">The location to convert.</param>
        /// <returns>The location in the normalized mercator coordinate system.</returns>
        public static Point ToNormalizedMercator(this Location location)
        {
            double x;
            {
                double normalizedLongitude = location.Longitude;

                if (normalizedLongitude < -180 || normalizedLongitude > 180)
                {
                    normalizedLongitude = normalizedLongitude - (Math.Floor((normalizedLongitude + 180.0) / 360.0) * 360.0);
                }
                x = normalizedLongitude / 360.0 + 0.5;
            }

            double y;
            {
                if (location.Latitude >= MapMath.MercatorLatitudeLimit)
                {
                    y = 0;
                }
                else if (location.Latitude <= -MapMath.MercatorLatitudeLimit)
                {
                    y = 1;
                }
                else
                {
                    double sinLatitude = Math.Sin(location.Latitude * MapMath.RadiansPerDegree);
                    y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4.0 * Math.PI);
                }
            }

            // Assert that the result roughly falls in Rect(0,0,1,1).
            Debug.Assert(MapMath.WithinEpsilon(x, MapMath.Clamp(x, 0, 1)));
            Debug.Assert(MapMath.WithinEpsilon(y, MapMath.Clamp(y, 0, 1)));

            return new Point(MapMath.Clamp(x, 0, 1), MapMath.Clamp(y, 0, 1));
        }

        /// <summary>
        /// Converts the specified point in normalized mercator, where longitude [-180,+180] and
        /// latitude [MercatorLatitudeLimit,-MercatorLatitudeLimit] both map to [0,1].
        /// </summary>
        /// <param name="normalizedMercatorPoint">The point to convert.</param>
        /// <returns>The location.</returns>
        public static Location ToLocation(this Point normalizedMercatorPoint)
        {
            double latitude = 90 - 2 * Math.Atan(Math.Exp((normalizedMercatorPoint.Y * 2.0 - 1.0) * Math.PI)) * MapMath.DegreesPerRadian;
            return new Location(latitude, (normalizedMercatorPoint.X - 0.5) * 360.0);
        }


        /// <summary>
        /// Returns whether this Rect intersects the other.
        /// </summary>
        public static bool Intersects(this Rect rect, Rect other)
        {
            other.Intersect(rect);
            return !other.IsEmpty;
        }

        /// <summary>
        /// Returns whether this Rect contains the other.
        /// </summary>
        public static bool Contains(this Rect rect, Rect other)
        {
            Rect ix = other;
            ix.Intersect(rect);
            return ix == other;
        }

        /// <summary>
        /// Returns the distance between this Rect and the other.
        /// </summary>
        public static double DistanceFrom(this Rect rect, Rect other)
        {
            double dx = Math.Max(0.0, Math.Abs(rect.X - other.X) - 0.5 * (rect.Width + other.Width));
            double dy = Math.Max(0.0, Math.Abs(rect.Y - other.Y) - 0.5 * (rect.Height + other.Height));

            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the distance from the this rect to the given point.
        /// </summary>
        public static double DistanceFrom(this Rect rect, Point point)
        {
            double dx = 0;
            if (point.X < rect.Left)
                dx = rect.Left - point.X;
            else if (point.X > rect.Right)
                dx = point.X - rect.Right;

            double dy = 0;
            if (point.Y < rect.Top)
                dy = rect.Top - point.Y;
            else if (point.Y > rect.Bottom)
                dy = point.Y - rect.Bottom;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the centroid of the rect.
        /// </summary>
        public static Point Centroid(this Rect rect)
        {
            return new Point(rect.X + 0.5 * rect.Width, rect.Y + 0.5 * rect.Height);
        }

        /// <summary>
        /// Returns a copy of the Rect translated by the specified amount.
        /// </summary>
        public static Rect Translated(this Rect rect, Point translation)
        {
            return new Rect(new Point(rect.X + translation.X, rect.Y + translation.Y), new Size(rect.Width, rect.Height));
        }
    }
}
