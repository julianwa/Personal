using System.Collections.Generic;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace MapItemClustering
{
    public class Clusterer
    {
        static public IEnumerable<MapItem> Cluster(IEnumerable<MapItem> itemIt)
        {
            MapItemQuadTree tree = new MapItemQuadTree();
            foreach (MapItem item in itemIt)
            {
                tree.Add(item);
            }

            Dictionary<int, List<MapItemQuadTreeNode>> nodesByZoomLevel = GroupNodesByZoomLevel(tree);

            // Sort all of the zoom levels present in descending order.
            List<int> zoomLevels = new List<int>(nodesByZoomLevel.Keys);
            zoomLevels.Sort();
            zoomLevels.Reverse();

            foreach (int zoomLevel in zoomLevels)
            {
                List<MapItemQuadTreeNode> nodesAtZoomLevel = nodesByZoomLevel[zoomLevel];

                foreach (var node in nodesAtZoomLevel)
                {
                    ClusterNodeItems(tree, node);
                }
            }

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

        private static void ClusterNodeItems(MapItemQuadTree tree, MapItemQuadTreeNode node)
        {
            List<List<MapItem>> clusters = new List<List<MapItem>>();
            HashSet<MapItem> visitedNodes = new HashSet<MapItem>();

            foreach (var item in node.Items)
            {
                List<MapItem> cluster = new List<MapItem>();

                GrowCluster(tree, item, node.ZoomLevel, visitedNodes, cluster);

                if (cluster.Count > 1)
                {
                    clusters.Add(cluster);
                }
            }

            clusters.ForEach((cluster) =>
            {
                double sumX = 0, sumY = 0;

                cluster.ForEach((item) =>
                {
                    Point normalizedMercator = item.Location.ToNormalizedMercator();
                    sumX += normalizedMercator.X;
                    sumY += normalizedMercator.Y;

                    tree.Remove(item);

                    if (item.MaxZoomLevel > node.ZoomLevel)
                    {
                        tree.Add(new FixedSizeMapItem(
                            item.Location,
                            PositionOrigin.Center,
                            new Size(20, 20),
                            node.ZoomLevel + 1,
                            item.MaxZoomLevel));
                    }
                });

                Point clusterCenterOfMass = new Point(sumX / cluster.Count, sumY / cluster.Count);

                tree.Add(new FixedSizeMapItem(
                    clusterCenterOfMass.ToLocation(),
                    PositionOrigin.Center,
                    new Size(20, 20),
                    0,
                    node.ZoomLevel));
            });
        }

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
    }
}
