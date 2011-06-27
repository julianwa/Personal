using System;
using System.Windows;
using Microsoft.Maps.MapControl;
using System.Collections.Generic;

namespace MapItemClustering
{
    /// <summary>
    /// A visual item that is placed on the map. All map items have a location and are visible only
    /// in a specified range of zoom levels. Derived classes specify how large the map item appears
    /// at a given zoom level.
    /// </summary>
    public abstract class MapItem
    {
        /// <summary>
        /// True if the map item is currently in view.
        /// </summary>
        private bool _InView;

        /// <summary>
        /// The children of this map item.
        /// </summary>
        private List<MapItem> _Children;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapItem"/> class.
        /// </summary>
        /// <param name="location">The location of the map item.</param>
        /// <param name="minZoomLevel">The min zoom level at which the map item appears.</param>
        /// <param name="maxZoomLevel">The max zoom level at which the map item appears.</param>
        public MapItem(Location location, int minZoomLevel, int maxZoomLevel)
        {
            if (minZoomLevel < 0 || maxZoomLevel < 0)
            {
                throw new ArgumentException("zoom levels must be non-negative");
            }

            if (minZoomLevel > maxZoomLevel)
            {
                throw new ArgumentException("max zoom level must be greater than or equal to min zoom level");
            }

            Location = location;
            MinZoomLevel = minZoomLevel;
            MaxZoomLevel = maxZoomLevel;

            _Children = new List<MapItem>();
        }

        /// <summary>
        /// Gets or sets the parent map item, which is optionally used if map items are
        /// composed hierarchically. 
        /// </summary>
        public MapItem Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the children, which is optionally used if map items are composed
        /// hierarchically.
        /// </summary>        
        public IEnumerable<MapItem> Children
        {
            get { return _Children; }
        }

        /// <summary>
        /// Gets the location of the map item.
        /// </summary>
        public Location Location
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the min zoom level at which the map item appears.
        /// </summary>
        public int MinZoomLevel
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the max zoom level at which the map item appears.
        /// </summary>
        public int MaxZoomLevel
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the tag, an arbitrary object value that can be used to store custom information about this map item.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the map item is currently in view.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the map item is currently in view; otherwise, <c>false</c>.
        /// </value>
        public bool InView
        {
            get
            {
                return _InView;
            }

            internal set
            {
                if (_InView != value)
                {
                    _InView = value;

                    if (InViewChanged != null)
                    {
                        InViewChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the map item enters or leaves the view.
        /// </summary>
        public event EventHandler InViewChanged;

        /// <summary>
        /// Specifies the size of the map item at a given zoom level. The size of some map items may remain
        /// fixed; for example, if they are tied to a geological entity on the map. Others, like a push-pin, 
        /// may remain fixed in size in screen pixels, causing them to vary in size on the map as a function
        /// of zoom level.
        /// </summary>
        /// <param name="zoomLevel">The zoom level.</param>
        /// <returns>The bounding rectangle of the map item at the given zoom level in 
        /// the form of a normalized mercator rectangle.</returns>
        public abstract NormalizedMercatorRect BoundingRectAtZoomLevel(double zoomLevel);

        internal void AddChild(MapItem child)
        {
            _Children.Add(child);
        }
    }
}
