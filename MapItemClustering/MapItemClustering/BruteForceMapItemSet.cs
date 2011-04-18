using System.Collections.Generic;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public class BruteForceMapItemSet : MapItemSet
    {
        private HashSet<MapItem> _MapItems;

        public BruteForceMapItemSet()
        {
            _MapItems = new HashSet<MapItem>();
        }

        public override void Add(MapItem mapItem)
        {
            _MapItems.Add(mapItem);
        }

        public override void ClearVisibility()
        {
            foreach (MapItem mapItem in _MapItems)
            {
                mapItem.InView = false;
            }
        }

        /// <summary>
        /// Returns an enumerator over all of the map items that are visible in the given rectangle
        /// and zoom level.
        /// </summary>
        /// <param name="rect">The rect used for the query.</param>
        /// <param name="zoomLevel">The zoom level used for the query.</param>
        /// <returns></returns>
        public override void UpdateVisibilty(LocationRect locationRect, int zoomLevel)
        {
            NormalizedMercatorRect queryRect = new NormalizedMercatorRect(locationRect);

            foreach (MapItem mapItem in _MapItems)
            {
                bool inView = false;

                if (zoomLevel <= mapItem.MaxZoomLevel && zoomLevel >= mapItem.MinZoomLevel)
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
