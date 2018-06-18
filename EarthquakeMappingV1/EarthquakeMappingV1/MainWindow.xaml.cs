using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//https://wiki.openstreetmap.org/wiki/Mercator
namespace EarthquakeMappingV1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region var´s 
        private static readonly double R_MAJOR = 6378137.0;
        private static readonly double R_MINOR = 6356752.3142;
        private static readonly double RATIO = R_MINOR / R_MAJOR;
        private static readonly double ECCENT = Math.Sqrt(1.0 - (RATIO * RATIO));
        private static readonly double COM = 0.5 * ECCENT;

        private static readonly double DEG2RAD = Math.PI / 180.0;
        private static readonly double RAD2Deg = 180.0 / Math.PI;
        private static readonly double PI_2 = Math.PI / 2.0;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DrawEllipse();
        }

        /// <summary>
        /// Simple logging function
        /// </summary>
        /// <param name="msg">Message to display</param>
        private void Log(string msg) => MessageBox.Show("[" + DateTime.Now + "] " + msg);

        private void DrawEllipse()
        {
            Ellipse e = new Ellipse
            {
                //Margin = new Thickness(LonToX(31.22222), LatToY(121.45806), 10, 10),
                Margin = new Thickness(10, 10, 10, 10),
                Fill = new SolidColorBrush(Colors.Red)
            };
            //left, top, right, bottem
            MapGrid.Children.Add(e);
        }

        /// <summary>
        /// Converts lon to pixel
        /// </summary>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double LonToX(double lon)
        {
            return R_MAJOR * DegToRad(lon);
        }

        /// <summary>
        /// Converts lat to pixel
        /// </summary>
        /// <param name="lat"></param>
        /// <returns></returns>
        public static double LatToY(double lat)
        {
            lat = Math.Min(89.5, Math.Max(lat, -89.5));
            double phi = DegToRad(lat);
            double sinphi = Math.Sin(phi);
            double con = ECCENT * sinphi;
            con = Math.Pow(((1.0 - con) / (1.0 + con)), COM);
            double ts = Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con;
            return 0 - R_MAJOR * Math.Log(ts);
        }

        /// <summary>
        /// Converts degree to radiant
        /// </summary>
        /// <param name="deg">Degree</param>
        /// <returns>Converted Radiant</returns>
        private static double DegToRad(double deg) => deg * DEG2RAD;
  
    }
}
