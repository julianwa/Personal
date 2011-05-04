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
            _RootNode = new MapItemQuadTreeNode(null, 0);
            _NodesToVisit = new Stack<MapItemQuadTreeNode>();
        }

        public IEnumerable<MapItemQuadTreeNode> Nodes
        {
            get
            {
                Debug.Assert(_NodesToVisit.Count == 0);

                _NodesToVisit.Push(_RootNode);

                while (_NodesToVisit.Count > 0)
                {
                    MapItemQuadTreeNode node = _NodesToVisit.Pop();

                    yield return node;

                    for (int childIdx = 0; childIdx < 4; childIdx++)
                    {
                        MapItemQuadTreeNode child = node.GetChild(childIdx);

                        if (child != null)
                        {
                            _NodesToVisit.Push(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the specified item to the tree.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>true if the element is added to the set; false if the element is already present.</returns>
        public bool Add(MapItem item)
        {
            Debug.Assert(_NodesToVisit.Count == 0);

            _NodesToVisit.Push(_RootNode);

            int numNodesVisited = 0;

            for (; _NodesToVisit.Count > 0; numNodesVisited++)
            {
                MapItemQuadTreeNode node = _NodesToVisit.Pop();
                Debug.Assert(node.ZoomLevel <= item.MaxZoomLevel);
                Debug.Assert(item.BoundingRectAtZoomLevel(node.ZoomLevel).Intersects(node.Rect));

                if (node.ZoomLevel >= item.MinZoomLevel)
                {
                    if (!node.AddMapItem(item))
                    {
                        Debug.Assert(numNodesVisited == 0);

                        // If the item's present at this node, it will be present in all others in its 
                        // [MinZoomLevel, MaxZoomLevel] range, so return early, indicating it's already in
                        // the tree.
                        return false;
                    }
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

            return true;
        }

        /// <summary>
        /// Removes the specified item from the tree.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
        public bool Remove(MapItem item)
        {
            Debug.Assert(_NodesToVisit.Count == 0);

            _NodesToVisit.Push(_RootNode);

            bool foundItem = false;

            while (_NodesToVisit.Count > 0)
            {
                MapItemQuadTreeNode node = _NodesToVisit.Pop();
                Debug.Assert(node.ZoomLevel <= item.MaxZoomLevel);
                Debug.Assert(item.BoundingRectAtZoomLevel(node.ZoomLevel).Intersects(node.Rect));

                if (node.ZoomLevel >= item.MinZoomLevel && node.ZoomLevel <= item.MaxZoomLevel)
                {
                    foundItem |= node.RemoveMapItem(item);
                }

                if (node.ZoomLevel < item.MaxZoomLevel)
                {
                    for (int childIdx = 0; childIdx < 4; childIdx++)
                    {
                        NormalizedMercatorRect itemRect = item.BoundingRectAtZoomLevel(node.ZoomLevel + 1);

                        MapItemQuadTreeNode child = node.GetChild(childIdx);
                        if (child != null && itemRect.Intersects(child.Rect))
                        {
                            _NodesToVisit.Push(node.EnsureChild(childIdx));
                        }
                    }
                }
            }

            return foundItem;
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
                throw new ArgumentException("zoom level must be non-negative");
            }

            if (!rect.Intersects(_RootNode.Rect))
            {
                throw new ArgumentException("rect out of range");
            }

            int nodesVisited = 0;

            Debug.Assert(_NodesToVisit.Count == 0);
            _NodesToVisit.Push(_RootNode);

            for (; _NodesToVisit.Count > 0; nodesVisited++)
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
