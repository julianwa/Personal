using System;
using System.Collections.Generic;
using System.Windows;

namespace MapItemClustering
{
    internal class MapItemQuadTreeNode
    {
        private MapItemQuadTreeNode[] _Children;

        private List<MapItem> _Items;

        public MapItemQuadTreeNode(Rect rect, int zoomLevel)
        {
            Rect = rect;
            ZoomLevel = zoomLevel;
            _Children = new MapItemQuadTreeNode[4];
            _Items = new List<MapItem>();
        }

        public Rect Rect
        {
            get;
            private set;
        }

        public int ZoomLevel
        {
            get;
            private set;
        }

        public MapItemQuadTreeNode GetChild(int childIdx)
        {
            return _Children[childIdx];
        }

        public MapItemQuadTreeNode EnsureChild(int childIdx)
        {
            MapItemQuadTreeNode child = _Children[childIdx];

            if (child == null)
            {
                child = _Children[childIdx] = new MapItemQuadTreeNode(GetChildRect(childIdx), ZoomLevel + 1);
            }

            return child;
        }

        public Rect GetChildRect(int childIdx)
        {
            Point offset;

            Size size = new Size(Rect.Width / 2, Rect.Height / 2);

            switch (childIdx)
            {
                case 0:
                    offset = new Point(0, 0);
                    break;
                case 1:
                    offset = new Point(size.Width, 0);
                    break;
                case 2:
                    offset = new Point(0, size.Height);
                    break;
                case 3:
                    offset = new Point(size.Width, size.Height);
                    break;
                default:
                    throw new ArgumentException("childIdx must be in [0,4)");
            };

            return new Rect(new Point(Rect.X + offset.X, Rect.Y + offset.Y), size);
        }

        public void AddMapItem(MapItem item)
        {
            _Items.Add(item);
        }

        public bool RemoveMapItem(MapItem item)
        {
            return _Items.Remove(item);
        }

        public IEnumerable<MapItem> Items
        {
            get { return _Items; }
        }
    }
}
