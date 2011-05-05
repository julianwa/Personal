using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MapItemClustering;

namespace KMeansClusteringTestApp
{
    public partial class MainPage : UserControl
    {
        private Random _Random;
        private DispatcherTimer _DispatcherTimer;
        private Point _MouseDownPoint;

        public MainPage()
        {
            InitializeComponent();

            MouseLeftButtonDown += new MouseButtonEventHandler(MainPage_MouseLeftButtonDown);
            MouseLeftButtonUp += new MouseButtonEventHandler(MainPage_MouseLeftButtonUp);
            MouseMove += new MouseEventHandler(MainPage_MouseMove);

            _Random = new Random(0);

            _DispatcherTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _DispatcherTimer.Tick += new EventHandler(_DispatcherTimer_Tick);
        }

        private void MainPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CaptureMouse())
            {
                _MouseDownPoint = e.GetPosition(this);
                _DispatcherTimer.Start();
            }
        }

        private void MainPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            _DispatcherTimer.Stop();
        }

        private void MainPage_MouseMove(object sender, MouseEventArgs e)
        {
            _MouseDownPoint = e.GetPosition(this);
        }

        private void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Rectangle r = new Rectangle()
            {
                Width = 100,
                Height = 100,
                Stroke = new SolidColorBrush() { Color = Colors.White }
            };
            Canvas.SetLeft(r, _MouseDownPoint.X + _Random.Next(100) - 50);
            Canvas.SetTop(r, _MouseDownPoint.Y + _Random.Next(100) - 50);

            Viewport.Children.Add(r);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var items = (from child in Viewport.Children
                         select new KMeansClustering.Item()
                             {
                                 Rect = new Rect(Canvas.GetLeft(child), Canvas.GetTop(child), ((FrameworkElement)child).ActualWidth, ((FrameworkElement)child).ActualHeight),
                                 Tag = child
                             }).ToList();

            const int TargetMaxItemsPerCluster = 20;
            int numClusters = (items.Count + TargetMaxItemsPerCluster - 1) / TargetMaxItemsPerCluster;

            var clusters = KMeansClustering.ClusterItems(items, numClusters);

            Color[] colors = new Color[] 
            {
                Colors.Red,
                Colors.Blue,
                Colors.Green,
                Colors.Magenta,
                Colors.Orange,
                Colors.Yellow,
                Colors.White,
                Colors.Cyan
            };

            for (int i = 0; i < clusters.Count; i++)
            {
                clusters[i].ForEach((item) =>
                    {
                        ((Rectangle)item.Tag).Stroke = new SolidColorBrush(colors[i % colors.Length]);
                    });
            }
        }
    }
}
