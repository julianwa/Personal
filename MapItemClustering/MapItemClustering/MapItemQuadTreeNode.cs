using System;
using System.Collections.Generic;
using System.Windows;

namespace MapItemClustering
{
    internal class MapItemQuadTreeNode
    {
        private MapItemQuadTreeNode[] _Children;

        private HashSet<MapItem> _Items;

        public MapItemQuadTreeNode(MapItemQuadTreeNode parent, int childIdx)
        {
            Parent = parent;

            if (Parent == null)
            {
                Rect = new Rect(0, 0, 1, 1);
                ZoomLevel = 0;
                X = Y = 0L;
            }
            else
            {
                Rect = parent.GetChildRect(childIdx);
                ZoomLevel = parent.ZoomLevel + 1;

                X = (Parent.X << 1) + (childIdx % 2);
                Y = (Parent.Y << 1) + (childIdx / 2);
            }

            _Children = new MapItemQuadTreeNode[4];
            _Items = new HashSet<MapItem>();
        }

        public MapItemQuadTreeNode Parent
        {
            get;
            private set;
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

        public long X
        {
            get;
            private set;
        }

        public long Y
        {
            get;
            private set;
        }

        public bool IsLeafNode
        {
            get
            {
                return _Children[0] == null && _Children[1] == null && _Children[2] == null && _Children[3] == null;
            }
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
                child = _Children[childIdx] = new MapItemQuadTreeNode(this, childIdx);
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

        public bool AddMapItem(MapItem item)
        {
            return _Items.Add(item);
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
