using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kinect003_Depth.Controller
{
    class myKinect
    {
        #region Member Variables
        private KinectSensor _Kinect;
        //private WriteableBitmap _ColorImageBitmap;
        //private Int32Rect _ColorImageBitmapRect;
        //private int _ColorImageStride;
        private MainWindow _main;
        private short[] _DepthImagePixelData;

        private DepthImageFrame _lastDepthFrame;

        #endregion Member Variables


        public myKinect(MainWindow x)
        {
            this._main = x;
            this._main.DepthImage.MouseLeftButtonUp +=DepthImage_MouseLeftButtonUp;
        }

        public void connect()
        {
            this.DiscoverKinectSensor();
        }

        public void disconnect()
        {
            this.UninitializeKinectSensor(_Kinect);
            //this._main.ColorImageElement = new Image();
        }

        public bool status()
        {
            bool result;
            if (Kinect == null || Kinect.Status == KinectStatus.Disconnected)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            Console.WriteLine(result);
            return result;
        }

        #region Methods
        private void DiscoverKinectSensor()
        {
            Console.WriteLine("Detecting Kinect Sensor...");
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;

            if (this.Kinect == null)
            {
                Console.WriteLine("Kinect Sensor Null");
                this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            }


        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            Console.WriteLine("Kinect sensor status: " + e.Status);

            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                    }
                    break;

                case KinectStatus.Disconnected:
                    if (this.Kinect == e.Sensor)
                    {
                        this.Kinect = null;
                        this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

                        if (this.Kinect == null)
                        {
                            // Notify the user that the sensor is disconnected
                            Console.WriteLine("Kinect sensor is disconnected.");
                        }
                    }
                    break;
                //Handle all other statuses according to needs
            }
        }
        #endregion Methods

        #region Properties
        public KinectSensor Kinect
        {
            get { return this._Kinect; }

            set
            {
                if (this._Kinect != value)
                {
                    if (this._Kinect != null)
                    {
                        Console.WriteLine("Uninitializing stream...");
                        UninitializeKinectSensor(this._Kinect);
                        
                        Console.WriteLine("Stream Uninitialized");
                    }

                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        Console.WriteLine("Initializing stream...");
                        this._Kinect = value;
                        InitializeKinectSensor(this._Kinect);
                        Console.WriteLine("Stream Initialized");
                    }
                }
            }


        }
        #endregion Properties

        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();//dont know why but somehow the sensor has not stopped during uninitialization
                //ColorImageStream colorStream = sensor.ColorStream;
                //sensor.ColorStream.Enable();

                //this._ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                //this._ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                //this._ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                //this._main.ColorImageElement.Source = this._ColorImageBitmap;

                //sensor.ColorFrameReady += Kinect_ColorFrameReady;

                this.Kinect.DepthStream.Enable();
                //this.Kinect.DepthFrameReady += depthFrameReady;
                this.Kinect.DepthFrameReady += depthFrameReady;

                sensor.Start();

            }
        }

        private void depthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (this._lastDepthFrame != null)
            {
                this._lastDepthFrame.Dispose();
                this._lastDepthFrame = null;
            }
            
            this._lastDepthFrame = e.OpenDepthImageFrame();

            if (this._lastDepthFrame != null)
            {
                this._DepthImagePixelData = new short[this._lastDepthFrame.PixelDataLength];
                this._lastDepthFrame.CopyPixelDataTo(_DepthImagePixelData);
                int stride = this._lastDepthFrame.Width * this._lastDepthFrame.BytesPerPixel;
                this._main.DepthImage.Source = BitmapSource.Create(this._lastDepthFrame.Width, this._lastDepthFrame.Height, 96, 96, PixelFormats.Gray16, null, this._DepthImagePixelData, stride);
            }
        }

        private void DepthImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("Im here");
            Point p = e.GetPosition(this._main.DepthImage);

            if (this._DepthImagePixelData != null && this._DepthImagePixelData.Length > 0)
            {
                //Console.WriteLine("Im here2");
                int pixelIndex = (int)(p.X + ((int)p.Y * this._lastDepthFrame.Width));
                int depth = this._DepthImagePixelData[pixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                int depthInches = (int)(depth * 0.0393700787);
                int depthFt = depthInches / 12;
                depthInches = depthInches % 12;

                this._main.PixelDepth.Text = depth + " [mm] " + depthFt + "' " + depthInches + " \"";
                    //string.Format("{0]mm~ {1}'{2}\"", depth, depthFt, depthInches);
            }

        }


        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                //sensor.ColorFrameReady -= Kinect_ColorFrameReady;
                this._Kinect = null;
            }
        }

        //private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        //{
        //    using (ColorImageFrame frame = e.OpenColorImageFrame())
        //    {
        //        if (frame != null)
        //        {
        //            byte[] pixelData = new byte[frame.PixelDataLength];
        //            frame.CopyPixelDataTo(pixelData);

        //            this._main.ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixelData, frame.Width * frame.BytesPerPixel);
        //            //this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, pixelData, this._ColorImageStride, 0);
        //        }
        //    }
        //}

    }
}
