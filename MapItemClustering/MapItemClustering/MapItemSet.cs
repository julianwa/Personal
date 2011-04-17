using System;
using System.Collections.Generic;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public class MapItemSet
    {
        private HashSet<MapItem> _MapItems;

        public MapItemSet()
        {
            _MapItems = new HashSet<MapItem>();
        }

        public void Add(MapItem mapItem)
        {
            _MapItems.Add(mapItem);
        }

        /// <summary>
        /// Returns an enumerator over all of the map items that are visible in the given rectangle
        /// and zoom level.
        /// </summary>
        /// <param name="rect">The rect used for the query.</param>
        /// <param name="zoomLevel">The zoom level used for the query.</param>
        /// <returns></returns>
        public void UpdateVisibilty(LocationRect locationRect, double zoomLevel)
        {
            NormalizedMercatorRect queryRect = new NormalizedMercatorRect(locationRect);

            int discreteZoomLevel = (int)Math.Floor(zoomLevel);

            foreach (MapItem mapItem in _MapItems)
            {
                bool inView = false;

                if (discreteZoomLevel <= mapItem.MaxZoomLevel && discreteZoomLevel >= mapItem.MinZoomLevel)
                {
                    NormalizedMercatorRect itemRect = mapItem.BoundingRectAtZoomLevel(zoomLevel);

                    if (queryRect.Intersects(itemRect))
                    {
                        inView = true;
                    }
                }

                mapItem.InView = inView;
            }
        }
    }
}
