using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace MapItemClustering
{
    internal class MapItemQuadTree
    {
        private MapItemQuadTreeNode _RootNode;
        private Stack<MapItemQuadTreeNode> _NodesToVisit;

        public MapItemQuadTree()
        {
            _RootNode = new MapItemQuadTreeNode(new Rect(0, 0, 1, 1), 0);
            _NodesToVisit = new Stack<MapItemQuadTreeNode>();
        }

        public void Add(MapItem item)
        {
            Debug.Assert(_NodesToVisit.Count == 0);

            _NodesToVisit.Push(_RootNode);

            while (_NodesToVisit.Count > 0)
            {
                MapItemQuadTreeNode node = _NodesToVisit.Pop();
                Debug.Assert(node.ZoomLevel <= item.MaxZoomLevel);
                Debug.Assert(item.BoundingRectAtZoomLevel(node.ZoomLevel).Intersects(node.Rect));

                if (node.ZoomLevel >= item.MinZoomLevel)
                {
                    node.AddMapItem(item);
                }

                if (node.ZoomLevel < item.MaxZoomLevel)
                {
                    for (int childIdx = 0; childIdx < 4; childIdx++)
                    {
                        Rect childRect = node.GetChildRect(childIdx);
                        NormalizedMercatorRect itemRect = item.BoundingRectAtZoomLevel(node.ZoomLevel + 1);

                        if (itemRect.Intersects(childRect))
                        {
                            _NodesToVisit.Push(node.EnsureChild(childIdx));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns all of the items that intersect the given rect at the given zoom level. This list
        /// may contain duplicate items.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="zoomLevel">The zoom level.</param>
        /// <returns></returns>
        public IEnumerable<MapItem> IntersectingItems(NormalizedMercatorRect rect, int zoomLevel)
        {
            if (zoomLevel < 0)
            {
                throw new ArgumentException("zoom level must be positive");
            }

            if (!rect.Intersects(_RootNode.Rect))
            {
                throw new ArgumentException("rect out of range");
            }

            Debug.Assert(_NodesToVisit.Count == 0);
            _NodesToVisit.Push(_RootNode);

            while (_NodesToVisit.Count > 0)
            {
                MapItemQuadTreeNode node = _NodesToVisit.Pop();
                Debug.Assert(node.ZoomLevel <= zoomLevel);
                Debug.Assert(rect.Intersects(node.Rect));

                if (zoomLevel == node.ZoomLevel)
                {
                    foreach (MapItem item in node.Items)
                    {
                        if (item.BoundingRectAtZoomLevel(node.ZoomLevel).Intersects(rect))
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    for (int childIdx = 0; childIdx < 4; childIdx++)
                    {
                        MapItemQuadTreeNode child = node.GetChild(childIdx);

                        if (child != null && rect.Intersects(child.Rect))
                        {
                            _NodesToVisit.Push(child);
                        }
                    }
                }
            }
        }
    }
}
