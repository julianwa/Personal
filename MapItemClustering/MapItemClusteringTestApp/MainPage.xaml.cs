using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MapItemClustering;
using Microsoft.Maps.MapControl;

namespace MapItemClusteringTestApp
{
    public partial class MainPage : UserControl
    {
        private Random _Rnd = new Random(0);

        private Stack<FrameworkElement> _PushpinPool;

        private int _CurrentMapItemSet;
        private List<MapItemSet> _MapItemSets;

        private bool _VisibilityUpdatePaused;

        public MainPage()
        {
            InitializeComponent();

            KeyDown += new KeyEventHandler(MainPage_KeyDown);

            _PushpinPool = new Stack<FrameworkElement>();

            _CurrentMapItemSet = 0;

            BuildMapItemSets();
        }

        private void BuildMapItemSets()
        {
            _MapItemSets = new List<MapItemSet>
            {                
                new QuadTreeMapItemSet(),
                new BruteForceMapItemSet()
            };

            List<MapItem> mapItems = new List<MapItem>();

            for (int i = 0; i < 10000; i++)
            {
                Location location = new Location(
                _Rnd.NextDouble() * 2 * MapMath.MercatorLatitudeLimit - MapMath.MercatorLatitudeLimit,
                _Rnd.NextDouble() * 10000 - 5000);

                MapItem item = new FixedSizeMapItem(location, PositionOrigin.Center, new Size(20, 20), 0, 18);

                mapItems.Add(item);
            }

            mapItems = new List<MapItem>(Clusterer.Cluster(mapItems));

            foreach (MapItem item in mapItems)
            {
                item.InViewChanged += new EventHandler(item_InViewChanged);
                _MapItemSets.ForEach((set) => set.Add(item));
            }
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
            else if (e.Key == Key.S)
            {
                _MapItemSets.ForEach((set) =>
                {
                    set.ClearVisibility();
                });

                _CurrentMapItemSet = (_CurrentMapItemSet + 1) % _MapItemSets.Count;

                _PushpinPool.Clear();

                UpdateVisibility();
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                _Map.ZoomLevel = (int)e.Key - (int)Key.D0;
            }
        }

        void item_InViewChanged(object sender, EventArgs e)
        {
            MapItem item = (MapItem)sender;

            if (item.InView)
            {
                FrameworkElement pushpin;

                if (_PushpinPool.Count == 0)
                {
                    pushpin = new Ellipse()
                    {
                        Fill = new SolidColorBrush(_CurrentMapItemSet % 2 == 0 ? Colors.Purple : Colors.Gray),
                        Width = 20,
                        Height = 20
                    };
                    pushpin.MouseRightButtonDown += new MouseButtonEventHandler(pushpin_MouseRightButtonDown);
                }
                else
                {
                    pushpin = _PushpinPool.Pop();
                }

                MapLayer.SetPositionOrigin(pushpin, PositionOrigin.Center);
                MapLayer.SetPosition(pushpin, item.Location);

                pushpin.Tag = item;
                item.Tag = pushpin;

                _PushPinLayer.Children.Add(pushpin);
            }
            else
            {
                FrameworkElement pushpin = (FrameworkElement)item.Tag;
                pushpin.Tag = null;
                item.Tag = null;
                _PushPinLayer.Children.Remove(pushpin);
                _PushpinPool.Push(pushpin);
            }
        }

        private void pushpin_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pushpin = (FrameworkElement)sender;
            var item = (MapItem)pushpin.Tag;

            _MapItemSets.ForEach((set) =>
            {
                bool removed = set.Remove(item);
                Debug.Assert(removed);
            });

            e.Handled = true;
        }

        private void _Map_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (!_VisibilityUpdatePaused)
            {
                // If we're very close to a discrete zoom level, then choose that discrete zoom level. 
                // Otherwise, choose the floor of the zoom level.
                int zoomLevel = (int)(Math.Abs(_Map.ZoomLevel - Math.Round(_Map.ZoomLevel)) < 1e-4 ?
                    Math.Round(_Map.ZoomLevel) :
                    Math.Floor(_Map.ZoomLevel));

                _MapItemSets[_CurrentMapItemSet].UpdateVisibilty(_Map.BoundingRectangle, zoomLevel);
            }
        }
    }
}
