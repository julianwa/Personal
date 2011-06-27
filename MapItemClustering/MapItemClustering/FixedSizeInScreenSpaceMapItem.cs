using System;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    /// <summary>
    /// A map item whose size in screen space remains fixed across zoom levels, such as a push-pin.
    /// </summary>
    public class FixedSizeInScreenSpaceMapItem : MapItem
    {
        private Point _LocationNormalizedMercator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedSizeInScreenSpaceMapItem"/> class.
        /// </summary>
        /// <param name="location">The location of the map item.</param>
        /// <param name="positionOrigin">The position origin of the map item.</param>
        /// <param name="sizeInPixels">The size in pixels of the map item.</param>
        /// <param name="minZoomLevel">The min zoom level at which the map item appears.</param>
        /// <param name="maxZoomLevel">The max zoom level at which the map item appears.</param>
        public FixedSizeInScreenSpaceMapItem(Location location,
            PositionOrigin positionOrigin,
            Size sizeInPixels,
            int minZoomLevel = 0,
            int maxZoomLevel = int.MaxValue)
            : base(location, minZoomLevel, maxZoomLevel)
        {
            PositionOrigin = positionOrigin;
            SizeInPixels = sizeInPixels;
            _LocationNormalizedMercator = location.ToNormalizedMercator();
        }

        /// <summary>
        /// Gets the position origin, which is the relative position of the map item that is anchored
        /// to the map item's location.
        /// </summary>
        public PositionOrigin PositionOrigin
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size in pixels of the map item.
        /// </summary>
        public Size SizeInPixels
        {
            get;
            private set;
        }

        /// <summary>
        /// Specifies the size of the map item at a given zoom level. This size of the map item in screen
        /// pixels is preserved, so as the zoom level increases, the bounding rect of the map item decreases
        /// because it's covering less and less of the map.
        /// </summary>
        /// <param name="zoomLevel">The zoom level.</param>
        /// <returns>
        /// The bounding rectangle of the map item at the given zoom level in
        /// the form of a normalized mercator rectangle.
        /// </returns>
        public override NormalizedMercatorRect BoundingRectAtZoomLevel(double zoomLevel)
        {
            double mapWidthInPixelsAtZoomLevel = 256 * Math.Pow(2, zoomLevel);

            double width = SizeInPixels.Width / mapWidthInPixelsAtZoomLevel;
            double height = SizeInPixels.Height / mapWidthInPixelsAtZoomLevel;

            Point center = _LocationNormalizedMercator;

            if (PositionOrigin == PositionOrigin.BottomCenter)
            {
                center.Y -= height / 2;
            }
            else if (PositionOrigin == PositionOrigin.BottomLeft)
            {
                center.X += width / 2;
                center.Y -= height / 2;
            }
            else if (PositionOrigin == PositionOrigin.BottomRight)
            {
                center.X -= width / 2;
                center.Y -= height / 2;
            }
            else if (PositionOrigin == PositionOrigin.CenterLeft)
            {
                center.X += width / 2;
            }
            else if (PositionOrigin == PositionOrigin.CenterRight)
            {
                center.X -= width / 2;
            }

            return new NormalizedMercatorRect(center, width, height);
        }
    }
}
