using System;

namespace MapItemClustering
{
    public static class MapMath
    {
        /// <summary>
        /// Radians per degree.
        /// </summary>
        public static readonly double RadiansPerDegree = Math.PI / 180.0;

        /// <summary>
        /// Maximum allowed latitude in Mercator space.
        /// </summary>
        public static readonly double MercatorLatitudeLimit = 85.051128;

        /// <summary>
        /// Clamps the value x to the range [a,b]
        /// </summary>        
        public static double Clamp(double x, double a, double b)
        {
            return Math.Max(a, Math.Min(b, x));
        }

        /// <summary>
        /// Returns whether the value x is withing epsilon of the target value.
        /// </summary>        
        public static bool WithinEpsilon(double x, double target)
        {
            return Math.Abs(target - x) < 1e-6;
        }
    }
}
