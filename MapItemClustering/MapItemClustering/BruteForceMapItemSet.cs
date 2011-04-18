using System.Collections.Generic;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public class BruteForceMapItemSet : MapItemSet
    {
        private HashSet<MapItem> _Items;

        public BruteForceMapItemSet()
        {
            _Items = new HashSet<MapItem>();
        }

        public override void Add(MapItem item)
        {
            _Items.Add(item);
        }

        /// <summary>
        /// Removes the given element from the set.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false.
        /// </returns>
        public override bool Remove(MapItem item)
        {
            item.InView = false;
            return _Items.Remove(item);
        }

        public override void ClearVisibility()
        {
            foreach (MapItem item in _Items)
            {
                item.InView = false;
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

            foreach (MapItem item in _Items)
            {
                bool inView = false;

                if (zoomLevel <= item.MaxZoomLevel && zoomLevel >= item.MinZoomLevel)
                {
                    NormalizedMercatorRect itemRect = item.BoundingRectAtZoomLevel(zoomLevel);

                    if (queryRect.Intersects(itemRect))
                    {
                        inView = true;
                    }
                }

                item.InView = inView;
            }
        }
    }
}
