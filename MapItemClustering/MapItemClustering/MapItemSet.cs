using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    /// <summary>
    /// A set of map items that is used to determine their visibility in a given view.
    /// </summary>
    public abstract class MapItemSet
    {
        /// <summary>
        /// Adds the specified map item to the set.
        /// </summary>
        /// <param name="item">The map item.</param>
        /// <returns>True if the element is added to the set; false if the element is already present.</returns>
        public abstract bool Add(MapItem item);

        /// <summary>
        /// Removes the given map item from the set.
        /// </summary>
        /// <param name="item">The map item to remove.</param>
        /// <returns>True if the element is successfully found and removed; otherwise, false.</returns>
        public abstract bool Remove(MapItem item);

        /// <summary>
        /// Updates each map item in the set, setting whether it's in the view specified by the given
        /// location rect and zoom level.
        /// </summary>
        /// <param name="locationRect">The location rect component of the view.</param>
        /// <param name="zoomLevel">The zoom level component of the view.</param>
        public abstract void UpdateVisibilty(LocationRect locationRect, int zoomLevel);
    }
}
