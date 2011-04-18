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
    }
}
