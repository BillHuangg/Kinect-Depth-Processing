using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.ComponentModel; 

namespace KinectDepthImageProcessing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        //process manager
        private DepthProcessManager depthProcessManager;

        //background thread
        private System.ComponentModel.BackgroundWorker backgroundWorker;

        //kinect array
        private List <KinectSensor> KinectArray;

        private int MAX_KINECT_NUM = 1;

        private Dot[] DotsArray;
        private List<Dot[]> DotsArrayList;

        public MainWindow()
        {
            
            InitializeComponent();

            //CompositionTarget.Rendering += CompositionTarget_Rendering;

            //build background thread for data processing
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += Worker_DoWork;
            backgroundWorker.RunWorkerAsync();

            //this.Loaded += (s, e) => DiscoverKinectSensor();
            //this.Unloaded += (s, e) => this.kinect = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            depthProcessManager = new DepthProcessManager();
            DotsArrayList=new List<Dot[]>();
            for (int i = 0; i < 3; i++)
            {
                Dot[] tempArray = new Dot[5];
                DotsArrayList.Add(initDotsArray(tempArray));

            }
                

        }

        //the background thread for process data
        //
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker != null)
            {
                while (!worker.CancellationPending)
                {
                    DiscoverKinectSensor();
                    PollDepthImageStream();
                }
            }
        }



        private void DiscoverKinectSensor()
        {
            //if (this.kinect != null && this.kinect.Status != KinectStatus.Connected)
            //{
            //    this.Kinect = null;
            //}
            if (KinectArray == null)
            {
                KinectArray = new List<KinectSensor>();
            }
            //stop and release
            else if (KinectArray != null)
            {
                bool isUnConnected = false;
                for (int i = 0; i < KinectArray.Count; i++)
                {
                    if (KinectArray[i].Status != KinectStatus.Connected)
                    {
                        isUnConnected = true;
                        //UninitializeKinectSensor(KinectArray[i]);
                        //break;
                    }
                }
                //if (isUnConnected)
                //{
                //    for (int i = 0; i < KinectArray.Count; i++)
                //    {
                //        UninitializeKinectSensor(KinectArray[i]);
                //    }
                //}
            }

            if (KinectArray.Count == 0)
            {
                // Look through all sensors and start the first connected one.
                // This requires that a Kinect is connected at the time of app startup.
                // To make your app robust against plug/unplug, 
                // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
                foreach (var potentialSensor in KinectSensor.KinectSensors)
                {
                    if (potentialSensor.Status == KinectStatus.Connected)
                    {
                        KinectArray.Add(potentialSensor);

                        //this.sensor = potentialSensor;
                        //break;
                    }
                }
                if (KinectArray.Count >= MAX_KINECT_NUM)
                {
                    for (int i = 0; i < KinectArray.Count; i++)
                    {
                        InitializeKinectSensor(KinectArray[i], i);
                    }
                    initDataArray();
                }
            }
        }



        private void PollDepthImageStream()
        {
            if (KinectArray.Count == 0)
            {
                //TODO: Display a message to plug-in a Kinect.
            }
            else
            {
                try
                {
                    for (int i = 0; i < KinectArray.Count; i++)
                    {
                        using (DepthImageFrame frame = KinectArray[i].DepthStream.OpenNextFrame(-1))
                        {
                            if (frame != null)
                            {
                                DepthFrameReadyToProcess(frame,i);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Report an error message
                }
            }
        }



        private void InitializeKinectSensor(KinectSensor kinectSensor,int kinectID)
        {
            if (kinectSensor != null)
            {
                DepthImageStream depthStream = kinectSensor.DepthStream;

                //正常运行: format Resolution640x480Fps30
                depthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

                DepthStreamFrameWidth = depthStream.FrameWidth;
                DepthStreamFrameHeight = depthStream.FrameHeight;
                DepthStreamFramePixelDataLength = depthStream.FramePixelDataLength;

                //初始化 数组相关变量
                initProcessImagesArray(kinectID);

                kinectSensor.Start();
            }
        }



        private void UninitializeKinectSensor(KinectSensor kinect)
        {
            if (kinect != null)
            {
                kinect.Stop();
            }
        }

        private void processImage_1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        //private void initProcessImagesArray(int kinectID)
        //{
        //    this.processImage_1.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        //初始化UI image 载体数据
        //        processImageBitMap_1 = new WriteableBitmap(DepthStreamFrameWidth, DepthStreamFrameHeight, 96, 96,
        //                                                                    PixelFormats.Bgr32, null);
        //        processImageBitmapRect_1 = new Int32Rect(0, 0, DepthStreamFrameWidth, DepthStreamFrameHeight);
        //        processImageStride_1 = DepthStreamFrameWidth * bytePerPixel;
        //        processImage_1.Source = processImageBitMap_1;
        //    }));

        //    this.processImage_2.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        //初始化UI image 载体数据
        //        processImageBitMap_2 = new WriteableBitmap(DepthStreamFrameWidth, DepthStreamFrameHeight, 96, 96,
        //                                                                    PixelFormats.Bgr32, null);
        //        processImageBitmapRect_2 = new Int32Rect(0, 0, DepthStreamFrameWidth, DepthStreamFrameHeight);
        //        processImageStride_2 = DepthStreamFrameWidth * bytePerPixel;
        //        processImage_2.Source = processImageBitMap_2;
        //    }));
        //    this.processImage_3.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        //初始化UI image 载体数据
        //        processImageBitMap_3 = new WriteableBitmap(DepthStreamFrameWidth, DepthStreamFrameHeight, 96, 96,
        //                                                                    PixelFormats.Bgr32, null);
        //        processImageBitmapRect_3 = new Int32Rect(0, 0, DepthStreamFrameWidth, DepthStreamFrameHeight);
        //        processImageStride_3 = DepthStreamFrameWidth * bytePerPixel;
        //        processImage_3.Source = processImageBitMap_3;
        //    }));
        //}
        //private void initDataArray()
        //{
        //    //预先初始化原始深度数据数组
        //    depthPixelData = new short[DepthStreamFramePixelDataLength];

        //    //预先初始化图像处理数据数组
        //    //或不需建立 直接于处理函数中建立局部变量即可
        //    //效率问题
        //    //enhPixelData = new byte[DepthStreamFrameWidth * DepthStreamFrameHeight * bytePerPixel];
        //}


        //void DepthFrameReadyToProcess(DepthImageFrame temp,int kinectID)
        //{

        //    DepthImageFrame lastDepthFrame = temp;

        //    if (lastDepthFrame != null)
        //    {
        //        //depthPixelData = new short[lastDepthFrame.PixelDataLength];
        //        lastDepthFrame.CopyPixelDataTo(depthPixelData);

        //        //处理数据并显示
        //        DataProcess(lastDepthFrame, depthPixelData,kinectID);

        //    }
        //}

        ////颜色模式: rgba32
        ////正常运行: format Resolution640x480Fps30
        ////          cubeWidth=cubeHeight=30 lineWidth=5

        ////景深范围
        //Int32 loThreashold = 2000;
        //Int32 hiThreshold = 2800;

        ////像素格式大小
        //Int32 bytePerPixel = 4;

        ////方格尺寸
        //int cubeWidth = 10;
        //int cubeHeight = 10;

        ////线颜色
        //int lineColor = 0;

        ////线宽度
        //int lineWidth = 1;

        ////最低可认的像素颜色界限
        //int minColorByte = 30;

        //private void DataProcess(DepthImageFrame depthFrame, short[] pixelData, int kinectID)
        //{
        //    //直接建立局部变量
        //    byte[] enhPixelData = new byte[depthFrame.Width * depthFrame.Height * bytePerPixel];

        //    //基础景深范围限定 并 处理
        //    enhPixelData = depthProcessManager.BasicDepthProcessing(enhPixelData, pixelData, depthFrame, loThreashold, hiThreshold);
            
        //    //像素马赛克化
        //    enhPixelData = depthProcessManager.MosaicProcessing(enhPixelData, cubeWidth, depthFrame.Width, depthFrame.Height);

        //    //绘制线 //方格
        //    enhPixelData = depthProcessManager.DrawLineProcessing(enhPixelData, cubeHeight, cubeWidth, lineColor, lineWidth, minColorByte, depthFrame);

        //    //绘制图像
        //    switch (kinectID)
        //    {
        //        case 0:
        //            this.processImage_1.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                //直接 new bitmap, 赋予processImage
        //                //processImage_1.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytePerPixel);
        //                //事先处理好 bitmap, 并与 processImage 事先建立 source 关系
        //                processImageBitMap_1.WritePixels(processImageBitmapRect_1, enhPixelData, processImageStride_1, 0);
        //            }));
        //            break;
        //        case 1:
        //            this.processImage_2.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                //直接 new bitmap, 赋予processImage
        //                //processImage_2.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytePerPixel);
        //                //事先处理好 bitmap, 并与 processImage 事先建立 source 关系
        //                processImageBitMap_2.WritePixels(processImageBitmapRect_2, enhPixelData, processImageStride_2, 0);
        //            }));
        //            break;
        //        case 2:
        //            this.processImage_3.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                //直接 new bitmap, 赋予processImage
        //                //processImage_2.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytePerPixel);
        //                //事先处理好 bitmap, 并与 processImage 事先建立 source 关系
        //                processImageBitMap_3.WritePixels(processImageBitmapRect_3, enhPixelData, processImageStride_3, 0);
        //            }));
        //            break;

        //    }
        //}



        //private void DiscoverKinectSensor()
        //{
        //    KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
        //    this.Kinnect = KinectSensor.KinectSensors.FirstOrDefault(sensor => sensor.Status == KinectStatus.Connected);
        //}

        //void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        //{
        //    switch (e.Status)
        //    {
        //        case KinectStatus.Connected:
        //            if (this.kinect == null)
        //                this.kinect = e.Sensor;
        //            break;
        //        case KinectStatus.Disconnected:
        //            if (this.kinect == e.Sensor)
        //            {
        //                this.kinect = null;
        //                this.kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        //                if (this.kinect == null)
        //                {
        //                    //TODO:通知用于Kinect已拔出
        //                }
        //            }
        //            break;
        //        //TODO:处理其他情况下的状态
        //    }
        //}
        //private void CompositionTarget_Rendering(object sender, EventArgs e)
        //{
        //    DiscoverKinectSensor();
        //    PollColorImageStream();
        //}



        //public KinectSensor Kinect
        //{
        //    get { return kinect; }
        //    set
        //    {
        //        if (kinect != null)
        //        {
        //            UninitializeKinectSensor(this.kinect);
        //            kinect = null;
        //        }
        //        if (value != null && value.Status == KinectStatus.Connected)
        //        {
        //            kinect = value;
        //            //InitializeKinectSensor(this.kinect);
        //        }
        //    }
        //}
    }
}
