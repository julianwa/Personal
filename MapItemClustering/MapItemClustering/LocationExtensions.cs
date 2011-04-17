using System;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public static class LocationExtensions
    {
        /// <summary>
        /// Converts the Location to normalized mercator, where latitude [-180,+180] and 
        /// longitude [MercatorLatitudeLimit, -MercatorLatitudeLimit] both map to [0,1].
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The location in the normalized mercator coordinate system.</returns>
        public static Point ToNormalizedMercator(this Location location)
        {
            double y;

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

            return new Point(location.Longitude / 360.0 + 0.5, y);
        }
    }
}
