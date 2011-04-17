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
    public class MapItem
    {
        public MapItem(Location location, Size size)
        {
            Location = location;
            Size = size;

            LocationNormalizedMercator = Location.ToNormalizedMercator();
        }

        public Location Location
        {
            get;
            private set;
        }

        public Point LocationNormalizedMercator
        {
            get;
            private set;
        }

        public Size Size
        {
            get;
            private set;
        }

        public object Tag
        {
            get;
            set;
        }
    }
}
