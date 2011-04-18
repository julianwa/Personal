using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MapItemClustering;
using Microsoft.Maps.MapControl;

namespace MapItemClusteringTestApp
{
    public partial class MainPage : UserControl
    {
        private Random _Rnd = new Random(0);

        private int _CurrentMapItemSet;
        private List<MapItem> _MapItems;
        private List<MapItemSet> _MapItemSets;

        private bool _VisibilityUpdatePaused;

        public MainPage()
        {
            InitializeComponent();

            _MapItems = new List<MapItem>();

            _CurrentMapItemSet = 0;
            _MapItemSets = new List<MapItemSet>
            {
                new BruteForceMapItemSet(),
                new QuadTreeMapItemSet()
            };

            //for (int maxZoomLevel = 2; maxZoomLevel < 12; maxZoomLevel++)
            //{
            //    for (int numPushpins = 0; numPushpins < 10 * (1 << maxZoomLevel); numPushpins++)
            //    {
            //        AddMapItem(maxZoomLevel);
            //    }
            //}

            for (int i = 0; i < 100; i++)
            {
                AddMapItem(0, 18);
            }

            KeyDown += new KeyEventHandler(MainPage_KeyDown);
        }

        private void AddMapItem(int minZoomLevel, int maxZoomLevel)
        {
            Location location = new Location(
                _Rnd.NextDouble() * 2 * MapMath.MercatorLatitudeLimit - MapMath.MercatorLatitudeLimit,
                _Rnd.NextDouble() * 10000 - 5000);

            MapItem item = new FixedSizeMapItem(location, PositionOrigin.BottomCenter, new Size(35, 41), minZoomLevel, maxZoomLevel);
            item.InViewChanged += new EventHandler(item_InViewChanged);

            _MapItems.Add(item);
            _MapItemSets.ForEach((set) => set.Add(item));
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

                UpdateVisibility();
            }
        }

        void item_InViewChanged(object sender, EventArgs e)
        {
            MapItem item = (MapItem)sender;

            if (item.InView)
            {
                Pushpin pushpin = new Pushpin()
                {
                    Location = item.Location,
                    Background = new SolidColorBrush(_CurrentMapItemSet % 2 == 0 ? Colors.Gray : Colors.Purple),
                    Tag = item
                };
                Canvas.SetZIndex(pushpin, _MapItems.IndexOf(item));

                pushpin.MouseRightButtonDown += new MouseButtonEventHandler(pushpin_MouseRightButtonDown);

                item.Tag = pushpin;

                _PushPinLayer.Children.Add(pushpin);
            }
            else
            {
                Pushpin pushpin = (Pushpin)item.Tag;
                pushpin.Tag = null;
                item.Tag = null;
                _PushPinLayer.Children.Remove(pushpin);
            }
        }

        private void pushpin_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pushpin = (Pushpin)sender;
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
                // Otherwise, choose the ceiling of the zoom level.
                int zoomLevel = (int)(Math.Abs(_Map.ZoomLevel - Math.Round(_Map.ZoomLevel)) < 1e-4 ?
                    Math.Round(_Map.ZoomLevel) :
                    Math.Ceiling(_Map.ZoomLevel));

                _MapItemSets[_CurrentMapItemSet].UpdateVisibilty(_Map.BoundingRectangle, zoomLevel);
            }
        }
    }
}
