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

        public FixedSizeMapItem(Location location,
            PositionOrigin positionOrigin,
            Size sizeInPixels,
            int minZoomLevel = 0,
            int maxZoomLevel = int.MaxValue)
            : base(location, minZoomLevel, maxZoomLevel)
        {
            PositionOrigin = positionOrigin;
            _SizeInPixels = sizeInPixels;
            _LocationNormalizedMercator = location.ToNormalizedMercator();
        }

        public PositionOrigin PositionOrigin
        {
            get;
            private set;
        }

        public override NormalizedMercatorRect BoundingRectAtZoomLevel(double zoomLevel)
        {
            double mapWidthInPixelsAtZoomLevel = 256 * Math.Pow(2, zoomLevel);

            double width = _SizeInPixels.Width / mapWidthInPixelsAtZoomLevel;
            double height = _SizeInPixels.Height / mapWidthInPixelsAtZoomLevel;

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
