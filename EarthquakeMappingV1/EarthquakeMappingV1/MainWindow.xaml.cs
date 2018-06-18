using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsPresentation;
using GMapMarker = GMap.NET.WindowsPresentation.GMapMarker;

//https://wiki.openstreetmap.org/wiki/Mercator
namespace EarthquakeMappingV1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region var´s 
            #region SourceUrl´s
            private const string SourceThirtyUrl =
            @"https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_month.csv";
            private const string SourceSevenUrl =
            @"https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_month.csv";
            private const string SourcePastUrl =
            @"https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_month.csv";
        #endregion
        private int _days;
        private List<string> _cordinatesList;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _cordinatesList = new List<string>();
        }

       
        /// <summary>
        /// Draws the marker
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        private void Draw(double x, double y, double size)
        {
            try
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(x,y))
                {
                    Shape = new Ellipse
                    {
                        Width = size * 2,
                        Height = size * 2,
                        Stroke = Brushes.Red,
                        Fill = Brushes.Black,
                        StrokeThickness = 1.5,
                        Opacity = 50
                    },
                    Offset = new Point(-size / 2, y: -size / 2)
                };

                MapView.Markers.Add(marker);
            }
            catch (Exception ex)
            {
                Log(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Simple logging function
        /// </summary>
        /// <param name="msg">Message to display</param>
        /// <param name="caption">Caption to display</param>
        private void Log(string msg, string caption = "Info")
        {
            if (caption != null) MessageBox.Show("[" + DateTime.Now + "] " + msg, caption);
        }

        /// <summary>
        /// Sets map properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            // choose your provider here
            //mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            MapView.MapProvider = GMap.NET.MapProviders.ArcGIS_StreetMap_World_2D_MapProvider.Instance;
            MapView.MinZoom = 0;
            MapView.MaxZoom = 15;
            // whole world zoom
            MapView.Zoom = 0;
            // lets the map use the mousewheel to zoom
            MapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            MapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            MapView.DragButton = MouseButton.Left;
        }

        /// <summary>
        /// Loads the days
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadDropDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock tb = sender as TextBlock;
                if (tb != null && tb.Text.Contains("7"))
                    _days = 7;
                else if (tb != null && tb.Text.Contains("30"))
                    _days = 30;
                else
                    _days = 1;

                BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
                worker.DoWork += LoadCordinates;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync(10000);

                Thread cordListTh = new Thread(LoadCordinates);
                cordListTh.Start();
                
                //todo wait for thread (make a worker)

                _cordinatesList.RemoveAt(0);
                foreach (string s in _cordinatesList)
                {
                    if (!s.Equals(string.Empty))
                    {
                        string[] splitCord = s.Split(';');
                        double strength = Convert.ToDouble(splitCord[4].Replace('.', ','));
                        if (!splitCord[4].Contains('-') || strength > 0.5)
                        Draw(Convert.ToDouble(splitCord[1].Replace('.', ',')),
                            Convert.ToDouble(splitCord[2].Replace('.', ',')), strength);
                    }
                }

            }
            catch (Exception ex)
            {
                Log(ex.Message + "| " + ex.StackTrace, "Error");
            }
        }

        /// <summary>
        /// Gets the cordinates from 
        /// </summary>
        /// <returns></returns>
        private void LoadCordinates()
        {
            WebClient webClient = new WebClient();
            string[] lines;

            if (_days.Equals(1))
                lines = webClient.DownloadString(SourcePastUrl).Split('\n');
            else if (_days.Equals(7))
                lines = webClient.DownloadString(SourceSevenUrl).Split('\n');
            else
                lines = webClient.DownloadString(SourceThirtyUrl).Split('\n');

            List<string> cordianates = new List<string>();

            foreach (string line in lines) //todo make progress bar
            {
                cordianates.Add(line.Replace(',', ';'));
            }

            _cordinatesList = cordianates;
        }
    }
}
