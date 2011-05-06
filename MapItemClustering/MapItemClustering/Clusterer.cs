using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Maps.MapControl;
using System;

namespace MapItemClustering
{
    public class Clusterer
    {
        private static Random _Random = new Random(0);

        /// <summary>
        /// Clusters the specified items.
        /// </summary>
        /// <param name="itemsToBeClustered">The items to be clustered.</param>
        /// <returns>The set of clustered map items.</returns>
        public static IEnumerable<MapItem> Cluster(IEnumerable<MapItem> itemsToBeClustered)
        {
            // Add all of the items to a map item quad tree.
            MapItemQuadTree tree = new MapItemQuadTree();
            foreach (MapItem item in itemsToBeClustered)
            {
                tree.Add(item);
            }

            Dictionary<int, List<MapItemQuadTreeNode>> nodesByZoomLevel = GroupNodesByZoomLevel(tree);

            // Sort all of the zoom levels present in descending order.
            List<int> zoomLevels = new List<int>(nodesByZoomLevel.Keys);
            zoomLevels.Sort();
            zoomLevels.Reverse();

            // Iterate over all zoom levels from finest to coarsest...
            foreach (int zoomLevel in zoomLevels)
            {

                // ...and cluster each node, one by one.
                foreach (var node in nodesByZoomLevel[zoomLevel])
                {
                    ClusterNodeItems(tree, node);
                }
            }

            // A single map item may appear in multiple nodes, so take care not to return duplicates.
            HashSet<MapItem> items = new HashSet<MapItem>();
            foreach (var node in tree.Nodes)
            {
                foreach (var item in node.Items)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// Iterates over all nodes in the quad tree, grouping them by zoom level.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <returns></returns>
        private static Dictionary<int, List<MapItemQuadTreeNode>> GroupNodesByZoomLevel(MapItemQuadTree tree)
        {
            Dictionary<int, List<MapItemQuadTreeNode>> nodesByZoomLevel;

            nodesByZoomLevel = new Dictionary<int, List<MapItemQuadTreeNode>>();
            foreach (var node in tree.Nodes)
            {
                List<MapItemQuadTreeNode> nodesAtZoomLevel;
                nodesByZoomLevel.TryGetValue(node.ZoomLevel, out nodesAtZoomLevel);

                if (nodesAtZoomLevel == null)
                {
                    nodesAtZoomLevel = nodesByZoomLevel[node.ZoomLevel] = new List<MapItemQuadTreeNode>();
                }

                nodesAtZoomLevel.Add(node);
            }

            return nodesByZoomLevel;
        }

        /// <summary>
        /// Clusters the map items contained in the node. Clustered items are modified and new items are added
        /// to the tree to represent the clusters.
        /// </summary>
        private static void ClusterNodeItems(MapItemQuadTree tree, MapItemQuadTreeNode node)
        {
            var clumps = new List<List<MapItem>>();
            var clusters = new List<Tuple<List<MapItem>, Point>>();
            HashSet<MapItem> visitedNodes = new HashSet<MapItem>();

            // Create the set of clusters of intersecting items.
            foreach (var item in node.Items)
            {
                List<MapItem> clump = new List<MapItem>();

                GrowClump(tree, item, node.ZoomLevel, visitedNodes, clump);

                if (clump.Count > 1)
                {
                    foreach (var cluster in ClusterClump(clump, node.ZoomLevel))
                    {
                        if (cluster.Item1.Count > 1)
                        {
                            clusters.Add(cluster);
                        }
                    }
                }
            }

            // For each cluster, modify the existing items in the cluster such that they don't appear at the same
            // LOD as the cluster and add new item(s) to the tree that represent the cluster.
            foreach (var cluster in clusters)
            {
                Debug.Assert(cluster.Item1.Count > 1);

                Point clusterLocation = cluster.Item2;

                var clusterItem = new FixedSizeInScreenSpaceMapItem(
                    clusterLocation.ToLocation(),
                    PositionOrigin.Center,
                    new Size(20, 20),
                    0,
                    node.ZoomLevel);

                tree.Add(clusterItem);

                foreach (var item in cluster.Item1)
                {
                    tree.Remove(item);

                    // If this item isn't present at a finer LOD than this, then it's been totally
                    // subsumed by the cluster; otherwise, re-add it to the tree with adjusted
                    // min zoom level.
                    if (item.MaxZoomLevel > node.ZoomLevel)
                    {
                        var adjustedItem = new FixedSizeInScreenSpaceMapItem(
                            item.Location,
                            PositionOrigin.Center,
                            new Size(20, 20),
                            node.ZoomLevel + 1,
                            item.MaxZoomLevel)
                        {
                            Parent = clusterItem
                        };

                        // We're removing the item, so re-assign the children's parent to the cluster that's
                        // standing in for it.
                        foreach (var child in item.Children)
                        {
                            child.Parent = adjustedItem;
                            adjustedItem.AddChild(child);
                        }

                        clusterItem.AddChild(adjustedItem);

                        tree.Add(adjustedItem);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively adds all items that intersect the specified item to the clump.
        /// </summary>        
        private static void GrowClump(MapItemQuadTree tree, MapItem item, int zoomLevel, HashSet<MapItem> visitedNodes, List<MapItem> clump)
        {
            if (visitedNodes.Add(item))
            {
                clump.Add(item);

                List<MapItem> intersectingItems = new List<MapItem>(tree.IntersectingItems(item.BoundingRectAtZoomLevel(zoomLevel), zoomLevel));

                foreach (var intersectingItem in intersectingItems)
                {
                    GrowClump(tree, intersectingItem, zoomLevel, visitedNodes, clump);
                }
            }
        }

        /// <summary>
        /// A clump is a set of items that do not necessarily all intersect each other, but that is a connected component in 
        /// the transitive closure of A intersects B applied to the set of all items. The idea is to break this clump into 
        /// a set of clusters, each associated with a representative point in normalized mercator space.
        /// </summary>
        private static IEnumerable<Tuple<List<MapItem>, Point>> ClusterClump(List<MapItem> clump, int zoomLevel)
        {
            var clusters = new List<Tuple<List<MapItem>, Point>>();

            double spacing = 1.5 * ((FixedSizeInScreenSpaceMapItem)clump[0]).BoundingRectAtZoomLevel(zoomLevel).Width;

            var mapItemsRemaining = new List<MapItem>(clump);
            mapItemsRemaining.Sort((left, right) =>
            {
                Point leftCentroid = left.BoundingRectAtZoomLevel(zoomLevel).Centroid;
                Point rightCentroid = right.BoundingRectAtZoomLevel(zoomLevel).Centroid;

                double leftDistSqr = (leftCentroid.X % spacing) * (leftCentroid.X % spacing) + (leftCentroid.Y % spacing) * (leftCentroid.Y % spacing);
                double rightDistSqr = (rightCentroid.X % spacing) * (rightCentroid.X % spacing) + (rightCentroid.Y % spacing) * (rightCentroid.Y % spacing);

                return Comparer<Double>.Default.Compare(leftDistSqr, rightDistSqr);
            });

            while (mapItemsRemaining.Count != 0)
            {
                MapItem item = mapItemsRemaining[mapItemsRemaining.Count - 1];

                var itemBoundsAtNextCoarsestZoomLevel = item.BoundingRectAtZoomLevel(zoomLevel);

                var clusterItems = (from ixItem in mapItemsRemaining
                                    where ixItem.BoundingRectAtZoomLevel(zoomLevel).Intersects(itemBoundsAtNextCoarsestZoomLevel)
                                    select ixItem).ToList();

                foreach (var clusterItem in clusterItems)
                {
                    mapItemsRemaining.Remove(clusterItem);
                }

                Debug.Assert(clusterItems.Contains(item), "Item should be included because it intersects itself.");

                //MapItem representativeItem = SelectRepresentativeItem(clusterItems, zoomLevel);
                MapItem representativeItem = item;

                clusters.Add(Tuple.Create(clusterItems, representativeItem.BoundingRectAtZoomLevel(zoomLevel).Centroid));
            }

            return clusters;        
        }

        private static MapItem SelectRepresentativeItem(List<MapItem> cluster, double zoomLevel)
        {
            double sumX = 0, sumY = 0;

            for (int itemIdx = 0; itemIdx < cluster.Count; itemIdx++)
            {
                MapItem item = cluster[itemIdx];
                Point normalizedMercator = item.Location.ToNormalizedMercator();
                sumX += normalizedMercator.X;
                sumY += normalizedMercator.Y;
            }

            Point clusterMean = new Point(sumX / cluster.Count, sumY / cluster.Count);

            // Pick the item that is nearest the center of mass.
            int nearestItemIdx = -1;
            double nearestDistSqr = double.MaxValue;
            for (int itemIdx = 0; itemIdx < cluster.Count; itemIdx++)
            {
                MapItem item = cluster[itemIdx];

                Point centroid = item.BoundingRectAtZoomLevel(zoomLevel).Centroid;

                double distSqr = clusterMean.DistanceSquared(centroid);

                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearestItemIdx = itemIdx;
                }
            }

            return cluster[nearestItemIdx];
        }
    }
}