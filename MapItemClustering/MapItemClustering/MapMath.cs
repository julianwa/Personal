using System;

namespace MapItemClustering
{
    public class MapMath
    {
        /// <summary>
        /// Radians per degree.
        /// </summary>
        public static readonly double RadiansPerDegree = Math.PI / 180.0;

        /// <summary>
        /// Maximum allowed latitude in Mercator space.
        /// </summary>
        public static readonly double MercatorLatitudeLimit = 85.051128;
    }
}
