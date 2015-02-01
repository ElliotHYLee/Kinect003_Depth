using Kinect003_Depth.Controller;
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

namespace Kinect003_Depth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        myKinect _kinect;
        public MainWindow()
        {
            InitializeComponent();
            _kinect = new myKinect(this);
            
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (this._kinect.status())
            {
                this._kinect.disconnect();
                Console.WriteLine("disconnect");
                this.btnConnect.Content = "Connect";
            }
            else
            {
                this._kinect.connect();
                Console.WriteLine("connect");
                this.btnConnect.Content = "Disconnect";
            }
            
        }

        private void btnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (this._kinect.status())
            {
                this._kinect.disconnect();
                this.Close();
            }
        }
    }
}
