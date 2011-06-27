using System.Collections.Generic;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    /// <summary>
    /// A map item set implemented using brute force methods for determing map item visibility.
    /// </summary>
    public class BruteForceMapItemSet : MapItemSet
    {
        private HashSet<MapItem> _Items;

        /// <summary>
        /// Initializes a new instance of the <see cref="BruteForceMapItemSet"/> class.
        /// </summary>
        public BruteForceMapItemSet()
        {
            _Items = new HashSet<MapItem>();
        }

        /// <summary>
        /// Adds the specified map item to the set.
        /// </summary>
        /// <param name="item">The map item.</param>
        /// <returns>
        /// True if the element is added to the set; false if the element is already present.
        /// </returns>
        public override bool Add(MapItem item)
        {
            if (!_Items.Contains(item))
            {
                _Items.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the given map item from the set.
        /// </summary>
        /// <param name="item">The map item to remove.</param>
        /// <returns>
        /// True if the element is successfully found and removed; otherwise, false.
        /// </returns>
        public override bool Remove(MapItem item)
        {
            item.InView = false;
            return _Items.Remove(item);
        }

        /// <summary>
        /// Updates each map item in the set, setting whether it's in the view specified by the given
        /// location rect and zoom level.
        /// </summary>
        /// <param name="locationRect">The location rect component of the view.</param>
        /// <param name="zoomLevel">The zoom level component of the view.</param>
        public override void UpdateVisibilty(LocationRect locationRect, int zoomLevel)
        {
            // If the location rect has no area...
            if (locationRect.Width <= 0 || locationRect.Height <= 0)
            {
                // ...then clear the visibility on each map item.
                foreach (MapItem item in _Items)
                {
                    item.InView = false;
                }
            }
            else
            {
                // ...otherwise, do the normal visibility update.
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
}
