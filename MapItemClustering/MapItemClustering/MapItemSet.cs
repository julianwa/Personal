using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public abstract class MapItemSet
    {
        public abstract void Add(MapItem mapItem);

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
