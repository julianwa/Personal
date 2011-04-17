using System;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public abstract class MapItem
    {
        bool _InView;

        public MapItem(Location location, int minZoomLevel, int maxZoomLevel)
        {
            Location = location;
            MinZoomLevel = minZoomLevel;
            MaxZoomLevel = maxZoomLevel;
        }

        public Location Location
        {
            get;
            private set;
        }

        public int MinZoomLevel
        {
            get;
            private set;
        }

        public int MaxZoomLevel
        {
            get;
            private set;
        }

        public object Tag
        {
            get;
            set;
        }

        public bool InView
        {
            get
            {
                return _InView;
            }

            internal set
            {
                if (_InView != value)
                {
                    _InView = value;

                    if (InViewChanged != null)
                    {
                        InViewChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event EventHandler InViewChanged;

        public abstract NormalizedMercatorRect BoundingRectAtZoomLevel(double zoomLevel);
    }

    public class FixedSizeMapItem : MapItem
    {
        private Size _SizeInPixels;
        private Point _LocationNormalizedMercator;

        public FixedSizeMapItem(Location location, Size sizeInPixels, int minZoomLevel = 0, int maxZoomLevel = int.MaxValue)
            : base(location, minZoomLevel, maxZoomLevel)
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
