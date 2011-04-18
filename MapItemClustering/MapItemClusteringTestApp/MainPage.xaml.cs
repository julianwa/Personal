using System;
using System.Collections.Generic;
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

            for (int maxZoomLevel = 2; maxZoomLevel < 12; maxZoomLevel++)
            {
                for (int numPushpins = 0; numPushpins < 10 * (1 << maxZoomLevel); numPushpins++)
                {
                    AddMapItem(maxZoomLevel);
                }
            }

            KeyDown += new KeyEventHandler(MainPage_KeyDown);
        }

        private void AddMapItem(int maxZoomLevel)
        {
            int minZoomLevel = maxZoomLevel;

            Location location = new Location(
                _Rnd.NextDouble() * 2 * MapMath.MercatorLatitudeLimit - MapMath.MercatorLatitudeLimit,
                _Rnd.NextDouble() * 10000 - 5000);

            MapItem mapItem = new FixedSizeMapItem(location, PositionOrigin.BottomCenter, new Size(35, 41), minZoomLevel, maxZoomLevel);
            mapItem.InViewChanged += new EventHandler(mapItem_InViewChanged);

            _MapItems.Add(mapItem);
            _MapItemSets.ForEach((set) => set.Add(mapItem));
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

        void mapItem_InViewChanged(object sender, EventArgs e)
        {
            MapItem mapItem = (MapItem)sender;

            if (mapItem.InView)
            {
                Pushpin pushpin = new Pushpin()
                {
                    Location = mapItem.Location,
                    Background = new SolidColorBrush(_CurrentMapItemSet % 2 == 0 ? Colors.Gray : Colors.Purple),
                };
                Canvas.SetZIndex(pushpin, _MapItems.IndexOf(mapItem));

                mapItem.Tag = pushpin;

                _PushPinLayer.Children.Add(pushpin);
            }
            else
            {
                _PushPinLayer.Children.Remove((Pushpin)mapItem.Tag);
            }
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
