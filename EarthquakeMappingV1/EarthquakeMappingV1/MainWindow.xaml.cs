using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GMap.NET;
using GMapMarker = GMap.NET.WindowsPresentation.GMapMarker;

//https://wiki.openstreetmap.org/wiki/Mercator
namespace EarthquakeMappingV1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
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
        private static List<GMapMarker> _markers;
        private const int MinStrength = 1;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _cordinatesList = new List<string>();
            _markers = new List<GMapMarker>();
        }

       
        /// <summary>
        /// Draws the marker
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="size">size of the marker</param>
        private void Draw(double x, double y, double size)
        {
            try
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(x,y))
                {
                    Shape = new Ellipse
                    {
                        Width = size * 2.5,
                        Height = size * 2.5,
                        Stroke = Brushes.Red,
                        Fill = Brushes.Black,
                        StrokeThickness = 1.5,
                    },
                    Offset = new Point(-size / 2, y: -size / 2)
                };

                _markers.Add(marker);
            }
            catch (Exception ex)
            {
                Log(ex.Message + " | " + ex.StackTrace, "Error");
            }
        }

        /// <summary>
        /// Simple logging function
        /// </summary>
        /// <param name="msg">Message to display</param>
        /// <param name="caption">Caption to display</param>
        static void Log(string msg, string caption = "Info") => MessageBox.Show("[" + DateTime.Now + "] " + msg, caption);

        #region Animations

        /// <summary>
        /// Animates the Rectangle from LightGRay to DarkGray
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseEnterChangeColorFromLightToDarkGray(object sender, MouseEventArgs e)
        {
            Rectangle r = (Rectangle)sender;

            r.Fill = new SolidColorBrush(Colors
                .LightGray); //If I don´t have this this shit does not work but i have no idea why

            r.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetAnimFromLightoToDarkGray());
        }

        /// <summary>
        /// Animates the Rectangle from DarkGray to LightGray
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseLeaveChangeColorFromDarkToLightGray(object sender, MouseEventArgs e)
        {
            Rectangle r = (Rectangle)sender;

            r.Fill = new SolidColorBrush(Colors
                .DarkGray); //If I don´t have this this shit does not work but i have no idea why

            r.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetAnimFromDarkToLightkGray());
        }

        /// <summary>
        /// Generates an animation from LightGray to DarkGray
        /// </summary>
        /// <returns></returns>
        private ColorAnimation GetAnimFromLightoToDarkGray()
        {
            ColorAnimation animation = new ColorAnimation
            {
                From = Colors.LightGray,
                To = Colors.DarkGray,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };

            return animation;
        }

        /// <summary>
        /// Generates an animation from DarkGray to LightGray
        /// </summary>
        /// <returns></returns>
        private ColorAnimation GetAnimFromDarkToLightkGray()
        {
            ColorAnimation animation = new ColorAnimation
            {
                From = Colors.DarkGray,
                To = Colors.LightGray,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };

            return animation;
        }

        #endregion

        #region Event´s

        /// <summary>
        /// Loadrec mousedown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadRec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadComboBox.IsDropDownOpen = true;
        }

        /// <summary>
        /// Sets map properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            MapView.MapProvider = GMap.NET.MapProviders.ArcGIS_Topo_US_2D_MapProvider.Instance;
            MapView.MinZoom = 0;
            MapView.MaxZoom = 2;
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

                LoadTextBlock.Text = "Loading...";

                BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
                worker.DoWork += Worker_LoadCordinates;
                worker.RunWorkerCompleted += Worker_CordinatesCompleted;
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Log(ex.Message + "| " + ex.StackTrace, "Error");
            }
        }
        #endregion

        #region Threads
        /// <summary>
        /// Gets the cordinates from 
        /// </summary>
        /// <returns></returns>
        private void Worker_LoadCordinates(object sender, DoWorkEventArgs e)
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

            foreach (string line in lines)
            {
                cordianates.Add(line.Replace(',', ';'));
            }

            _cordinatesList = cordianates;
        }

        /// <summary>
        /// Cordinates completely loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_CordinatesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.ProgressChanged += Worker_HandleCordinatesProgressChanged;
            worker.DoWork += Worker_HandleCordinates;
            worker.RunWorkerCompleted += Worker_HandleCordinatesCompleted;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// HandleCordinates is finish
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_HandleCordinatesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (GMapMarker marker in _markers)
            {
                MapView.Markers.Add(marker);
            }

            LoadTextBlock.Text = "Load";
        }

        /// <summary>
        /// Handels the HandleCordinatesProgressChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_HandleCordinatesProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is object[] eObjects)
            {
                Draw(Convert.ToDouble(eObjects[0]), Convert.ToDouble(eObjects[1]), Convert.ToDouble(eObjects[2]));
            }
        }

        /// <summary>
        /// Handels the cordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_HandleCordinates(object sender, DoWorkEventArgs e)
        {
            _cordinatesList.RemoveAt(0);
            for (int idx = 0; idx < _cordinatesList.Count; idx++)
            {
                string cordinate = _cordinatesList[idx];
                if (!cordinate.Equals(string.Empty))
                {
                    string[] splitCord = cordinate.Split(';');
                    double strength = Convert.ToDouble(splitCord[4].Replace('.', ','));
                    if (!splitCord[4].Contains('-') || strength > MinStrength)
                    {
                        (sender as BackgroundWorker)?.ReportProgress(idx,
                            new object[]
                            {
                                Convert.ToDouble(splitCord[1].Replace('.', ',')),
                                Convert.ToDouble(splitCord[2].Replace('.', ',')), strength
                            });
                    }
                }
            }
        }
        #endregion
    }
}
