using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public abstract class MapItemSet
    {
        /// <summary>
        /// Adds the specified item to the set.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>true if the element is added to the set; false if the element is already present.</returns>
        public abstract bool Add(MapItem item);

        /// <summary>
        /// Removes the given element from the set.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
        public abstract bool Remove(MapItem item);

        public abstract void ClearVisibility();

        /// <summary>
        /// Returns an enumerator over all of the map items that are visible in the given rectangle
        /// and zoom level.
        /// </summary>
        /// <param name="rect">The rect used for the query.</param>
        /// <param name="zoomLevel">The zoom level used for the query.</param>
        /// <returns></returns>
        public abstract void UpdateVisibilty(LocationRect locationRect, int zoomLevel);
    }
}
