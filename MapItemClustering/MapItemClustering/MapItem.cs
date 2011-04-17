using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public abstract class MapItem
    {
        public MapItem(Location location)
        {
            Location = location;
        }

        public Location Location
        {
            get;
            private set;
        }

        public abstract NormalizedMercatorRect BoundingRectAtZoomLevel(double zoomLevel);

        public object Tag
        {
            get;
            set;
        }
    }

    public class FixedSizeMapItem : MapItem
    {
        private Size _SizeInPixels;
        private Point _LocationNormalizedMercator;

        public FixedSizeMapItem(Location location, Size sizeInPixels)
            : base(location)
        {
            _SizeInPixels = sizeInPixels;
            _LocationNormalizedMercator = location.ToNormalizedMercator();
        }

        public override NormalizedMercatorRect BoundingRectAtZoomLevel(double zoomLevel)
        {
            double mapWidthInPixelsAtZoomLevel = 256 * Math.Pow(2, zoomLevel);

            return new NormalizedMercatorRect(
                _LocationNormalizedMercator,
                _SizeInPixels.Width / mapWidthInPixelsAtZoomLevel,
                _SizeInPixels.Height / mapWidthInPixelsAtZoomLevel);
        }
    }
}
