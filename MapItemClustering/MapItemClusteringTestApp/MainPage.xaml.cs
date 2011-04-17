using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Maps.MapControl;
using MapItemClustering;

namespace MapItemClusteringTestApp
{
    public partial class MainPage : UserControl
    {
        private Random _Rnd = new Random(0);

        private MapItemSet _MapItemSet;

        public MainPage()
        {
            InitializeComponent();

            _MapItemSet = new MapItemSet();

            for (int i = 0; i < 100; i++)
            {
                Location location = new Location(
                    _Rnd.NextDouble() * 2 * MapMath.MercatorLatitudeLimit - MapMath.MercatorLatitudeLimit,
                    _Rnd.NextDouble() * 360 - 180);

                Pushpin pushpin = new Pushpin()
                {
                    Location = location
                };

                _MapItemSet.Add(new MapItem(location, new Size(100, 100))
                {
                    Tag = pushpin
                });

                _PushPinLayer.Children.Add(pushpin);
            }
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (MapItem mapItem in _MapItemSet.Query(_Map.BoundingRectangle, _Map.ZoomLevel))
            {
                ((Pushpin)mapItem.Tag).Background = new SolidColorBrush(Colors.Gray);
            }
        }
    }
}
