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
        //process imageBitMap
        ///1
        private WriteableBitmap processImageBitMap_1;
        private Int32Rect processImageBitmapRect_1;
        private Int32 processImageStride_1;
        ///2
        private WriteableBitmap processImageBitMap_2;
        private Int32Rect processImageBitmapRect_2;
        private Int32 processImageStride_2;
        ///3
        private WriteableBitmap processImageBitMap_3;
        private Int32Rect processImageBitmapRect_3;
        private Int32 processImageStride_3;
        //
        int DepthStreamFrameWidth = 0;
        int DepthStreamFrameHeight = 0;
        int DepthStreamFramePixelDataLength = 0;

        //store each frame depth data for all kinects
        private short[] depthPixelData;
        private List<List<short[]>> KinectIDdepthPixelList;


        //颜色模式: rgba32
        //正常运行: format Resolution640x480Fps30
        //          cubeWidth=cubeHeight=30 lineWidth=5

        //景深范围
        Int32 loThreashold = 1500;
        Int32 hiThreshold =3000;

        //像素格式大小
        Int32 bytePerPixel = 4;

        //方格尺寸
        int cubeWidth = 8;
        int cubeHeight = 8;

        //线颜色
        int lineColor = 0;

        //线宽度
        int lineWidth = 0;

        //最低可认的像素颜色界限
        int minColorByte = 20;



        private void initProcessImagesArray(int kinectID)
        {
            this.processImage_1.Dispatcher.BeginInvoke(new Action(() =>
            {
                //初始化UI image 载体数据
                processImageBitMap_1 = new WriteableBitmap(DepthStreamFrameWidth, DepthStreamFrameHeight, 96, 96,
                                                                            PixelFormats.Bgr32, null);
                processImageBitmapRect_1 = new Int32Rect(0, 0, DepthStreamFrameWidth, DepthStreamFrameHeight);
                processImageStride_1 = DepthStreamFrameWidth * bytePerPixel;
                processImage_1.Source = processImageBitMap_1;
            }));

            this.processImage_2.Dispatcher.BeginInvoke(new Action(() =>
            {
                //初始化UI image 载体数据
                processImageBitMap_2 = new WriteableBitmap(DepthStreamFrameWidth, DepthStreamFrameHeight, 96, 96,
                                                                            PixelFormats.Bgr32, null);
                processImageBitmapRect_2 = new Int32Rect(0, 0, DepthStreamFrameWidth, DepthStreamFrameHeight);
                processImageStride_2 = DepthStreamFrameWidth * bytePerPixel;
                processImage_2.Source = processImageBitMap_2;
            }));
            //this.processImage_3.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    //初始化UI image 载体数据
            //    processImageBitMap_3 = new WriteableBitmap(DepthStreamFrameWidth, DepthStreamFrameHeight, 96, 96,
            //                                                                PixelFormats.Bgr32, null);
            //    processImageBitmapRect_3 = new Int32Rect(0, 0, DepthStreamFrameWidth, DepthStreamFrameHeight);
            //    processImageStride_3 = DepthStreamFrameWidth * bytePerPixel;
            //    processImage_3.Source = processImageBitMap_3;
            //}));
        }

        private void initKinectIDdepthPixelList(int KinectID)
        {
            //List<short[]>[] KinectIDdepthPixelList;
            KinectIDdepthPixelList=new List<List<short[]>>();
            for (int i = 0; i < KinectArray.Count(); i++)
            {
                List<short[]> temp = new List<short[]>();
                for (int j = 0; j < 3; j++)
                {
                    short[] tempShort = new short[DepthStreamFramePixelDataLength];
                    temp.Add( tempShort);
                }
                    //new short[DepthStreamFramePixelDataLength];
                KinectIDdepthPixelList.Add(temp);

            }


        }


        private void initDataArray()
        {
            //预先初始化原始深度数据数组
            depthPixelData = new short[DepthStreamFramePixelDataLength];
            //预先初始化图像处理数据数组
            //或不需建立 直接于处理函数中建立局部变量即可
            //效率问题
            //enhPixelData = new byte[DepthStreamFrameWidth * DepthStreamFrameHeight * bytePerPixel];
        }




        private Dot[] initDotsArray(Dot[] tempArray)
        {
            Random randomHeight = new Random(3);
            Random randomWidth = new Random(2);
            //DotsArray
            for (int i = 0; i < tempArray.Length; i++)
            {
                

                int tempHeight = -1;
                int tempWidth = -1;
                while (tempHeight < 4)
                {
                    tempHeight = randomHeight.Next(10);
                }

                while (tempWidth <6)
                {
                    tempWidth = randomWidth.Next(9);
                }
                //dot height
                int dotY = cubeWidth * tempHeight;
                int dotX=(i + 1) * cubeWidth * tempWidth;
                if (i == 0)
                {
                    tempWidth = 2;
                }

                tempArray[i] = new Dot(dotX, dotY, cubeWidth, 2);
            }
            return tempArray;
        }

        //private int countFiveFrame = 0;
        //private bool isFull = false;
        void DepthFrameReadyToProcess(DepthImageFrame temp, int kinectID)
        {

            DepthImageFrame lastDepthFrame = temp;

            if (lastDepthFrame != null)
            {
                depthPixelData = new short[lastDepthFrame.PixelDataLength];
                lastDepthFrame.CopyPixelDataTo(depthPixelData);
                //KinectIDdepthPixelList[kinectID][countFiveFrame] = depthPixelData;


                //计时
                //countFiveFrame++;
                //每5帧执行一次
                //if (isFull)
                //{
                //    Int32[] tempData = CalculateAverageDepthData(KinectIDdepthPixelList[kinectID]);
                //    //处理数据并显示
                //    DataProcess(lastDepthFrame, tempData, kinectID);
                //}
                //else if (countFiveFrame >= KinectIDdepthPixelList[kinectID].Count())
                //{
                //    Int32[] tempData = CalculateAverageDepthData(KinectIDdepthPixelList[kinectID]);
                //    //处理数据并显示
                //    DataProcess(lastDepthFrame, tempData, kinectID);
                //    //reset
                //    //countFiveFrame = 0;
                //    if (!isFull)
                //    {
                //        isFull = true;
                //    }
                    
                //}
                //if (countFiveFrame >= KinectIDdepthPixelList[kinectID].Count())
                //{
                    
                //    //reset
                //    countFiveFrame = 0;
                //}

                //if (countFiveFrame >= KinectIDdepthPixelList[kinectID].Count())
                //{
                //    Int32[] tempData = CalculateAverageDepthData(KinectIDdepthPixelList[kinectID]);
                //    //处理数据并显示
                //    DataProcess(lastDepthFrame, tempData, kinectID);
                //    //reset
                //    //countFiveFrame = 0;
                //    if (!isFull)
                //    {
                //        isFull = true;
                //    }
                //    countFiveFrame = 0;
                //}


                //处理数据并显示
                DataProcess(lastDepthFrame, depthPixelData, kinectID);
            }
        }
        //private Int32[] CalculateAverageDepthData(List<short[]> depthArray)
        //{
        //    //short[] result = depthArray[0];

        //    Int32[] result = new Int32[depthArray[0].Count()];

            
                
        //        //depth = original[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
        //    for (int i = 0; i < result.Count(); i++)
        //    {
        //        Int32 all = 0;
        //        int countBlank = 0;
        //        int countFull = 0;
        //        for (int j = 0; j < depthArray.Count; j++)
        //        {
        //            Int32 each = depthArray[j][i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
        //            //if (each > 1000 || each < 4000)
        //            //{
        //            all += each;
        //            if (each < 1000)
        //            {
        //                countBlank++;
        //            }
        //            else if (each > 3000)
        //            {
        //                countFull++;
        //            }
        //            //}
        //        }
        //        Int32 temp;
        //        if (countBlank >= 1)
        //        {
        //            temp = 2000;
        //        }
        //        else if (countFull >= 1)
        //        {
        //            temp = 0;
        //        }
        //        //if (countBlank >= countFull)
        //        //{
        //        //    temp=2000;
        //        //}
        //        //else if (countBlank < countFull)
        //        //{
        //        //    temp =0;
        //        //}
        //        else
        //        {
        //            temp = all / depthArray.Count;
        //        }
        //        result[i] = temp;
        //    }
        //        //calculation

            

        //    return result;
        //}

        private void DataProcess(DepthImageFrame depthFrame, short[] pixelData, int kinectID)
        //private void DataProcess(DepthImageFrame depthFrame, Int32[] pixelData, int kinectID)
        {
            //直接建立局部变量
            byte[] enhPixelData = new byte[depthFrame.Width * depthFrame.Height * bytePerPixel];

            

            //基础景深范围限定 并 处理
            enhPixelData = depthProcessManager.BasicDepthProcessing(enhPixelData, pixelData, depthFrame, loThreashold, hiThreshold);

            //优化 - 影响边缘
            enhPixelData = depthProcessManager.OptimizeDepthProcessing(enhPixelData, depthFrame);

            //add dots
            enhPixelData = depthProcessManager.DrawDotsProcessing(enhPixelData, DotsArrayList[kinectID], depthFrame.Width, depthFrame.Height);

            //像素马赛克化
            enhPixelData = depthProcessManager.MosaicProcessing(enhPixelData, cubeWidth, depthFrame.Width, depthFrame.Height);

            //dot collision detection
            depthProcessManager.DotsCollisionDetection(enhPixelData, DotsArrayList[kinectID], depthFrame.Width, depthFrame.Height, 1);

            //绘制线 //方格
            enhPixelData = depthProcessManager.DrawLineProcessing(enhPixelData, cubeHeight, cubeWidth, lineColor, lineWidth, minColorByte, depthFrame);
            
            //绘制图像
            switch (kinectID)
            {
                case 0:
                    this.processImage_1.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //直接 new bitmap, 赋予processImage
                        //processImage_1.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytePerPixel);
                        //事先处理好 bitmap, 并与 processImage 事先建立 source 关系
                        processImageBitMap_1.WritePixels(processImageBitmapRect_1, enhPixelData, processImageStride_1, 0);
                    }));
                    break;
                case 1:
                    this.processImage_2.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //直接 new bitmap, 赋予processImage
                        //processImage_2.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytePerPixel);
                        //事先处理好 bitmap, 并与 processImage 事先建立 source 关系
                        processImageBitMap_2.WritePixels(processImageBitmapRect_2, enhPixelData, processImageStride_2, 0);
                    }));
                    break;
                case 2:
                    //this.processImage_3.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    //直接 new bitmap, 赋予processImage
                    //    //processImage_2.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytePerPixel);
                    //    //事先处理好 bitmap, 并与 processImage 事先建立 source 关系
                    //    processImageBitMap_3.WritePixels(processImageBitmapRect_3, enhPixelData, processImageStride_3, 0);
                    //}));
                    break;

            }

            for (int count = 0; count < DotsArrayList.Count; count++)
            {

                int countDisappearDot = 0;
                Dot[] temp = DotsArrayList[count];
                //恢复所有点
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].dotState == DotState.DISAPPEAR)
                    {
                        countDisappearDot++;
                    }
                }
                if (countDisappearDot >= temp.Length)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].dotState == DotState.DISAPPEAR)
                        {
                            temp[i].resetDot();
                            //temp[i].dotState = DotState.WAIT;
                        }
                    }
                }
            }
           

        }


        //




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
