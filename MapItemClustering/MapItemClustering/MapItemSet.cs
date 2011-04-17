using System.Collections.Generic;
using System.Windows;
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
        public IEnumerable<MapItem> Query(LocationRect rect, double zoomLevel)
        {
            // Transform the northwest and southeast corners of the rect to normalized mercator.
            Point nw = rect.Northwest.ToNormalizedMercator();
            Point se = rect.Southeast.ToNormalizedMercator();

            // Check if the rect wraps around to the other side.
            if (nw.X < se.X)
            {
                Rect normalizedMercatorRect = new Rect(nw, se);

                foreach (MapItem mapItem in _MapItems)
                {
                    if (normalizedMercatorRect.Contains(mapItem.LocationNormalizedMercator))
                    {
                        yield return mapItem;
                    }
                }
            }
            else
            {
                // If so, we need two normalized mercator rects to represent its area.
                Rect normalizedMercatorRect0 = new Rect(new Point(0, nw.Y), new Point(se.X, se.Y));
                Rect normalizedMercatorRect1 = new Rect(new Point(nw.X, nw.Y), new Point(1, se.Y));

                foreach (MapItem mapItem in _MapItems)
                {
                    if (normalizedMercatorRect0.Contains(mapItem.LocationNormalizedMercator) ||
                        normalizedMercatorRect1.Contains(mapItem.LocationNormalizedMercator))
                    {
                        yield return mapItem;
                    }
                }
            }
        }
    }
}
