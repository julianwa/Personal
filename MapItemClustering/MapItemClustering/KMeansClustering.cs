using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace MapItemClustering
{
    public static class KMeansClustering
    {
        private static Random _Random = new Random(0);

        public struct Item
        {
            public Rect Rect;
            public object Tag;
        }

        public static IList<List<Item>> ClusterItems(IList<Item> items, int numClusters)
        {
            bool[] itemSelectedAsClusterSeed = new bool[items.Count];

            List<Point> clusterCenters = new List<Point>();

            // Seed the cluster centers with the centroids of randomly selected items.
            for (int clusterIdx = 0; clusterIdx < numClusters; clusterIdx++)
            {
                while (true)
                {
                    int itemIdx = _Random.Next(items.Count);
                    if (!itemSelectedAsClusterSeed[itemIdx])
                    {
                        itemSelectedAsClusterSeed[itemIdx] = true;
                        clusterCenters.Add(items[itemIdx].Rect.Centroid());
                        break;
                    }
                }
            }

            return ClusterItemsUsingKMeans_SeededClusters(items, clusterCenters);
        }

        private static List<List<Item>> ClusterItemsUsingKMeans_SeededClusters(IList<Item> items, List<Point> clusterCenters)
        {
            int numClusters;
            int[] clusterAssignments;
            int[] clusterCounts;

            while (true)
            {
                numClusters = clusterCenters.Count;
                clusterAssignments = Enumerable.Repeat(-1, items.Count).ToArray();
                clusterCounts = new int[numClusters];

                bool settled = false;

                while (!settled)
                {
                    settled = true;

                    for (int itemIdx = 0; itemIdx < items.Count; itemIdx++)
                    {
                        int minDistanceCluster = -1;
                        double minDistance = double.MaxValue;

                        for (int clusterIdx = 0; clusterIdx < numClusters; clusterIdx++)
                        {
                            double distance = Math.Sqrt(items[itemIdx].Rect.DistanceSquared(clusterCenters[clusterIdx]));
                            if (distance < minDistance)
                            {
                                minDistanceCluster = clusterIdx;
                                minDistance = distance;
                            }
                        }

                        if (minDistanceCluster != clusterAssignments[itemIdx])
                        {
                            clusterAssignments[itemIdx] = minDistanceCluster;
                            settled = false;
                        }
                    }

                    if (!settled)
                    {
                        UpdateClusterCenters(items, clusterAssignments, clusterCenters, clusterCounts);
                    }
                }

                // Remove all clusters that only contain one item. If any were removed
                // start the process over again!
                {
                    List<Point> newClusterCenters = new List<Point>();
                    for (int clusterIdx = 0; clusterIdx < numClusters; clusterIdx++)
                    {
                        if (clusterCounts[clusterIdx] >= 2)
                        {
                            newClusterCenters.Add(clusterCenters[clusterIdx]);
                        }
                    }

                    if (newClusterCenters.Count == numClusters)
                    {
                        break;
                    }
                    else
                    {
                        clusterCenters = newClusterCenters;
                    }
                }
            }

            var clusters = new List<List<Item>>();

            for (int clusterIdx = 0; clusterIdx < numClusters; clusterIdx++)
            {
                var cluster = new List<Item>();

                for (int itemIdx = 0; itemIdx < items.Count; itemIdx++)
                {
                    if (clusterAssignments[itemIdx] == clusterIdx)
                    {
                        cluster.Add(items[itemIdx]);
                    }
                }

                clusters.Add(cluster);
            }

            return clusters;
        }

        private static void UpdateClusterCenters(IList<Item> items, int[] clusterAssignments, List<Point> clusterCenters, int[] clusterCounts)
        {
            int numClusters = clusterCenters.Count;

            Array.Clear(clusterCounts, 0, clusterCounts.Length);

            for (int clusterIdx = 0; clusterIdx < numClusters; clusterIdx++)
            {
                Point accumulator = new Point();

                for (int itemIdx = 0; itemIdx < items.Count; itemIdx++)
                {
                    if (clusterAssignments[itemIdx] == clusterIdx)
                    {
                        Point itemCentroid = items[itemIdx].Rect.Centroid();
                        accumulator.X += itemCentroid.X;
                        accumulator.Y += itemCentroid.Y;

                        clusterCounts[clusterIdx]++;
                    }
                }

                int count = clusterCounts[clusterIdx];
                if (count > 0)
                {
                    clusterCenters[clusterIdx] = new Point(accumulator.X / count, accumulator.Y / count);
                }
            }
        }
    }
}
