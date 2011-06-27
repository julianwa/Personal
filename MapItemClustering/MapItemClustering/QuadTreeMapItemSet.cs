using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    /// <summary>
    /// A map item set that uses a quad tree to determing map item visibility.
    /// </summary>
    public class QuadTreeMapItemSet : MapItemSet
    {
        private MapItemQuadTree _Items;
        private HashSet<MapItem> _VisibleItems;
        private HashSet<MapItem> _NewVisibleItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadTreeMapItemSet"/> class.
        /// </summary>
        public QuadTreeMapItemSet()
        {
            _Items = new MapItemQuadTree();
            _VisibleItems = new HashSet<MapItem>();
            _NewVisibleItems = new HashSet<MapItem>();
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
            return _Items.Add(item);
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
            _VisibleItems.Remove(item);
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
            Debug.Assert(_NewVisibleItems.Count == 0);

            // If the location rect has no area...
            if (locationRect.Width <= 0 || locationRect.Height <= 0)
            {
                // ...then clear the visibility on each map item.

                foreach (MapItem item in _VisibleItems)
                {
                    item.InView = false;
                }

                _VisibleItems.Clear();
            }
            else
            {
                // ... otherwise, do the normal visibility update.

                foreach (MapItem item in _Items.IntersectingItems(new NormalizedMercatorRect(locationRect), zoomLevel))
                {
                    _NewVisibleItems.Add(item);
                }

                _VisibleItems.ExceptWith(_NewVisibleItems);

                foreach (MapItem item in _VisibleItems)
                {
                    item.InView = false;
                }

                foreach (MapItem item in _NewVisibleItems)
                {
                    item.InView = true;
                }

                var temp = _VisibleItems;
                _VisibleItems = _NewVisibleItems;
                _NewVisibleItems = temp;
                _NewVisibleItems.Clear();
            }
        }
    }
}
