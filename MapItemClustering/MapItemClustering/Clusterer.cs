using System.Collections.Generic;
using System.Windows;
using Microsoft.Maps.MapControl;
using System;
using System.Linq;
using System.Diagnostics;

namespace MapItemClustering
{
    public class Clusterer
    {
        /// <summary>
        /// Clusters the specified items.
        /// </summary>
        /// <param name="itemsToBeClustered">The items to be clustered.</param>
        /// <returns>The set of clustered map items.</returns>
        static public IEnumerable<MapItem> Cluster(IEnumerable<MapItem> itemsToBeClustered)
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
        /// Clusters the map items contained in the node. This operation modifies existing items in the
        /// tree and adds new items that represent the clusters.
        /// </summary>
        private static void ClusterNodeItems(MapItemQuadTree tree, MapItemQuadTreeNode node)
        {
            List<List<MapItem>> clusters = new List<List<MapItem>>();
            HashSet<MapItem> visitedNodes = new HashSet<MapItem>();

            // Create the set of clusters of intersecting items.
            foreach (var item in node.Items)
            {
                List<MapItem> cluster = new List<MapItem>();

                GrowCluster(tree, item, node.ZoomLevel, visitedNodes, cluster);

                if (cluster.Count > 1)
                {
                    foreach (var subCluster in PossiblySubdivideCluster(cluster, node.ZoomLevel))
                    {
                        clusters.Add(subCluster);
                    }
                }
            }

            // For each cluster, modify the existing items in the cluster such that they don't appear at the same
            // LOD as the cluster and add new item(s) to the tree that represent the cluster.
            clusters.ForEach((cluster) =>
            {
                Debug.Assert(cluster.Count > 1);

                double sumX = 0, sumY = 0;

                cluster.ForEach((item) =>
                {
                    Point normalizedMercator = item.Location.ToNormalizedMercator();
                    sumX += normalizedMercator.X;
                    sumY += normalizedMercator.Y;
                });

                Point clusterCenterOfMass = new Point(sumX / cluster.Count, sumY / cluster.Count);

                var clusterItem = new FixedSizeInScreenSpaceMapItem(
                    clusterCenterOfMass.ToLocation(),
                    PositionOrigin.Center,
                    new Size(20, 20),
                    0,
                    node.ZoomLevel);

                tree.Add(clusterItem);

                cluster.ForEach((item) =>
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

                        clusterItem.AddChild(adjustedItem);

                        tree.Add(adjustedItem);
                    }
                });
            });
        }

        /// <summary>
        /// Recursively adds all items that intersect the specified item to the cluster.
        /// </summary>        
        private static void GrowCluster(MapItemQuadTree tree, MapItem item, int zoomLevel, HashSet<MapItem> visitedNodes, List<MapItem> cluster)
        {
            if (visitedNodes.Add(item))
            {
                cluster.Add(item);

                List<MapItem> intersectingItems = new List<MapItem>(tree.IntersectingItems(item.BoundingRectAtZoomLevel(zoomLevel), zoomLevel));

                foreach (var intersectingItem in intersectingItems)
                {
                    GrowCluster(tree, intersectingItem, zoomLevel, visitedNodes, cluster);
                }
            }
        }

        static private IEnumerable<List<MapItem>> PossiblySubdivideCluster(List<MapItem> cluster, int zoomLevel)
        {
            const int TargetMaxItemsPerCluster = 20;

            int numClusters = (cluster.Count + TargetMaxItemsPerCluster - 1) / TargetMaxItemsPerCluster;

            if (numClusters > 1)
            {
                var items = (from item in cluster
                             select new KMeansClustering.Item()
                                 {
                                     Rect = item.BoundingRectAtZoomLevel(zoomLevel).AsRect(),
                                     Tag = item
                                 }).ToList();

                var clusters = new List<List<MapItem>>();
                foreach (var subCluster in KMeansClustering.ClusterItems(items, numClusters))
                {
                    clusters.Add((from item in subCluster select (MapItem)item.Tag).ToList());
                }
                return clusters;
            }
            else
            {
                return new List<MapItem>[] { cluster };
            }
        }
    }
}
