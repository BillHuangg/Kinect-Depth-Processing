using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
namespace KinectDepthImageProcessing
{
    class KinectManager
    {
        private KinectSensor kinect;

        public KinectSensor Kinect
        {
            get { return kinect; }
            set
            {
                //if (kinect != null)
                //{
                //    UninitializeKinectSensor(this.kinect);
                //    kinect = null;
                //}
                if (value != null && value.Status == KinectStatus.Connected)
                {
                    kinect = value;
                    InitializeKinectSensor(kinect);
                }
            }
        }


        private void InitializeKinectSensor(KinectSensor kinectSensor)
        {
            if (kinectSensor != null)
            {
                //DepthImageStream depthStream = kinectSensor.DepthStream;

                ////正常运行: format Resolution640x480Fps30
                //depthStream.Enable(DepthImageFormat.Resolution640x480Fps30);



                //kinectSensor.Start();
            }


        }
        public void StartkinectSensor()
        {
            kinect.Start();
        }
        private void UninitializeKinectSensor(KinectSensor kinect)
        {
            if (kinect != null)
            {
                kinect.Stop();
                //kinect.DepthFrameReady -= new EventHandler<DepthImageFrameReadyEventArgs>(kinectSensor_DepthFrameReady);
            }
        }
    }

}
