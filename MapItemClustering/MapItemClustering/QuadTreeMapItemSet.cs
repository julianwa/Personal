using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public class QuadTreeMapItemSet : MapItemSet
    {
        private MapItemQuadTree _Items;
        private HashSet<MapItem> _VisibleItems;
        private HashSet<MapItem> _NewVisibleItems;

        public QuadTreeMapItemSet()
        {
            _Items = new MapItemQuadTree();
            _VisibleItems = new HashSet<MapItem>();
            _NewVisibleItems = new HashSet<MapItem>();
        }

        public override void ClearVisibility()
        {
            foreach (MapItem item in _VisibleItems)
            {
                item.InView = false;
            }

            _VisibleItems.Clear();
        }

        public override void Add(MapItem item)
        {
            _Items.Add(item);
        }

        /// <summary>
        /// Removes the given element from the set.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
        public override bool Remove(MapItem item)
        {
            item.InView = false;
            _VisibleItems.Remove(item);
            return _Items.Remove(item);
        }

        public override void UpdateVisibilty(LocationRect locationRect, int zoomLevel)
        {
            Debug.Assert(_NewVisibleItems.Count == 0);

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
