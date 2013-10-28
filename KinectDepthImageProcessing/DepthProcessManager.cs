using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
namespace KinectDepthImageProcessing
{
    class DepthProcessManager
    {
        private Int32 bytePerPixel = 4;

        private bool isNotOutOfLimitation(int index,int min,int max)
        {
            return (index >= min && index < max);
        }

        //基础景深数据处理
        public byte[] BasicDepthProcessing(byte[] result, short[] original, DepthImageFrame depthFrame, Int32 loThreashold, Int32 hiThreshold)
        {
            Int32 depth;
            Int32 gray;
            Int32 alpha;
            for (int i = 0, j = 0; i < original.Length; i++, j += bytePerPixel)
            {
                depth = original[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                if (depth < loThreashold || depth > hiThreshold)
                {
                    //this gray must be larger than minColorByte
                    //so that can draw the line the whole background
                    gray = 40;
                    alpha = 255;

                }
                else
                {
                    int temp = (depth / 100) % 10 / 3;
                    gray = 255 - (temp * 50);
                    alpha = 255;
                }
                result[j] = (byte)gray;
                result[j + 1] = (byte)gray;
                result[j + 2] = (byte)gray;
                result[j + 3] = (byte)alpha;


            }
            return result;
        }



        //马赛克处理
        public byte[] MosaicProcessing(byte[] temp, int val, int width, int height)
        {

            // int w = b.Width;
            // int h = b.Height;
            int stdR, stdG, stdB, stdA;
            stdR = 0;
            stdG = 0;
            stdB = 0;
            stdA = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int depthPixelIndex = (x + (y * width)) * bytePerPixel;

                    if ( isNotOutOfLimitation( depthPixelIndex, 0, temp.Count() ))
                    {
                        if (y % val == 0)
                        {
                            if (x % val == 0)
                            {
                                stdB = temp[depthPixelIndex];
                                stdG = temp[depthPixelIndex + 1];
                                stdR = temp[depthPixelIndex + 2];
                                //stdA = temp[depthPixelIndex + 3];
                            }
                            else
                            {
                                temp[depthPixelIndex] = (byte)stdB;
                                temp[depthPixelIndex + 1] = (byte)stdG;
                                temp[depthPixelIndex + 2] = (byte)stdR;
                                //temp[depthPixelIndex + 3] = (byte)stdA;
                            }
                        }
                        else
                        {
                            // 复制上一行
                            int upPixelIndex = depthPixelIndex - width * bytePerPixel;

                            if ( isNotOutOfLimitation( upPixelIndex, 0, temp.Count() ))
                            {
                                temp[depthPixelIndex] = temp[upPixelIndex];
                                temp[depthPixelIndex + 1] = temp[upPixelIndex + 1];
                                temp[depthPixelIndex + 2] = temp[upPixelIndex + 2];
                                //temp[depthPixelIndex + 3] = temp[upPixelIndex + 3];
                                  
                            }
                        }
                    }
                } // end of x
            } // end of y

            return temp;
        }

        public byte[] DrawLineProcessing(byte[] result, int cubeHeight, int cubeWidth, int lineColor, int lineWidth, int minColorByte, DepthImageFrame depthFrame)
        {
            for (int depthY = 0; depthY < depthFrame.Height; depthY++)
            {
                //竖线
                if (depthY % cubeHeight != 0)
                {
                    for (int depthX = 0, testIndex = 0; depthX < depthFrame.Width; depthX++, testIndex++)
                    {
                        int depthPixelIndex = (depthX + (depthY * depthFrame.Width)) * bytePerPixel;

                        if (result[depthPixelIndex] >= minColorByte)
                        {
                            if (testIndex % cubeWidth == 0)
                            {
                                result[depthPixelIndex] = (byte)lineColor;
                                result[depthPixelIndex + 1] = (byte)lineColor;
                                result[depthPixelIndex + 2] = (byte)lineColor;

                                for (int i = -(lineWidth / 2); i < (lineWidth / 2) + 1; i++)
                                {
                                    int tempPixelDataIndex = depthPixelIndex + i * bytePerPixel;
                                    result[tempPixelDataIndex] = (byte)lineColor;
                                    result[tempPixelDataIndex + 1] = (byte)lineColor;
                                    result[tempPixelDataIndex + 2] = (byte)lineColor;
                                }
                            }
                        }
                    }
                }
                //横线
                else
                {
                    for (int depthX = 0, testIndex = 0; depthX < depthFrame.Width; depthX++, testIndex++)
                    {
                        int depthPixelIndex = (depthX + (depthY * depthFrame.Width)) * bytePerPixel;

                        if (result[depthPixelIndex] >= minColorByte)
                        {

                            result[depthPixelIndex] = (byte)lineColor;
                            result[depthPixelIndex + 1] = (byte)lineColor;
                            result[depthPixelIndex + 2] = (byte)lineColor;
                            for (int i = -(lineWidth / 2); i < (lineWidth / 2) + 1; i++)
                            {
                                int tempPixelDataIndex = depthPixelIndex + i * depthFrame.Width * bytePerPixel;
                                if ( isNotOutOfLimitation( tempPixelDataIndex, 0, result.Count() )) 
                                {
                                    result[tempPixelDataIndex] = (byte)lineColor;
                                    result[tempPixelDataIndex + 1] = (byte)lineColor;
                                    result[tempPixelDataIndex + 2] = (byte)lineColor;
                                }
                            }

                        }
                    }
                }
            }

            return result;
        }

    }
}
