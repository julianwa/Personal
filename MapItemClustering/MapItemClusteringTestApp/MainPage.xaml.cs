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

        private bool _VisibilityUpdatePaused;

        public MainPage()
        {
            InitializeComponent();

            _MapItemSet = new MapItemSet();

            for (int i = 0; i < 100; i++)
            {
                Location location = new Location(
                    _Rnd.NextDouble() * 2 * MapMath.MercatorLatitudeLimit - MapMath.MercatorLatitudeLimit,
                    _Rnd.NextDouble() * 10000 - 5000);

                Pushpin pushpin = new Pushpin()
                {
                    Location = location,
                    Background = new SolidColorBrush(Colors.Gray)
                };

                //int maxZoomLevel = _Rnd.Next(20) + 1;
                int maxZoomLevel = 40;

                MapItem mapItem = new FixedSizeMapItem(location, new Size(35, 41), maxZoomLevel: maxZoomLevel)
                {
                    Tag = pushpin
                };
                mapItem.InViewChanged += new EventHandler(mapItem_InViewChanged);

                _MapItemSet.Add(mapItem);

                _PushPinLayer.Children.Add(pushpin);
            }

            KeyDown += new KeyEventHandler(MainPage_KeyDown);
        }

        private void MainPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                _VisibilityUpdatePaused = !_VisibilityUpdatePaused;

                if (_VisibilityUpdatePaused)
                {
                    _ViewportRectLayer.Children.Add(new MapPolyline()
                    {
                        Locations = new LocationCollection()
                        {
                            _Map.BoundingRectangle.Northwest,
                            _Map.BoundingRectangle.Northeast,
                            _Map.BoundingRectangle.Southeast,
                            _Map.BoundingRectangle.Southwest,
                            _Map.BoundingRectangle.Northwest
                        },
                        Stroke = new SolidColorBrush(Colors.Red),
                        StrokeThickness = 2
                    });
                }
                else
                {
                    _ViewportRectLayer.Children.Clear();
                }
            }
        }

        void mapItem_InViewChanged(object sender, EventArgs e)
        {
            MapItem mapItem = (MapItem)sender;
            ((Pushpin)mapItem.Tag).Background = new SolidColorBrush(mapItem.InView ? Colors.Red : Colors.Gray);
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            //_MapItemSet.UpdateVisibilty(_Map.BoundingRectangle, _Map.ZoomLevel);
        }

        private void _Map_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (!_VisibilityUpdatePaused)
            {
                _MapItemSet.UpdateVisibilty(_Map.BoundingRectangle, _Map.ZoomLevel);
            }
        }
    }
}
