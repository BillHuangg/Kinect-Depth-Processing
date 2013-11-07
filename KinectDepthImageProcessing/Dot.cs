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

using System.ComponentModel; 
namespace KinectDepthImageProcessing
{
    enum DotState
    {
        BLANK=0,
        WAIT,
        HIT,
        ANIMATING,
        DISAPPEAR,

    }
    enum AnimationMode
    {
        BlockMode=0,
        FragmentMode,
    }
    //dot for game
    //the Coordinate for dots is at the left top of a image
    //right = x 
    //down = y
    class Dot
    {
        private int my_x;
        private int my_y;
        private int my_EachBlocklength;
        private int my_EachLineBlockNums;  //
        private DotState my_state;
        private int[] my_ColorBGR;

        ///animation
        //for counting the animation time 
        //
        private AnimationMode my_animationMode;
        private int AnimationLifeTimer = 0;
        private bool isStartAnimation = false;
        //block mode
        private int my_AnimationBlockNums;
        //smaller , faster
        private int AnimationBlockNumChangingSpeed = 5;
        private int AnimationBlockNumChangeTimes = 2;
        //fragment mode
        private Point[] FragmentsPosition;
        private int currentFragmentAnimationTime;
        private int FragmentAnimationFrameTime = 1;
        private int currentFragmentStage =-1;
        private List<Point[]> FragmentMovePathList;

        private Point[] Path0 = {   new Point(-1, -1), 
                                    new Point(-1, -1), 
                                    new Point(-1, 1), 
                                    new Point(-1, 1), 
                                    new Point(-1, 1) };

        private Point[] Path1 = {   new Point(1, -1), 
                                    new Point(1, -1), 
                                    new Point(1, 1), 
                                    new Point(1, 1), 
                                    new Point(1, 1) };

        private Point[] Path2 = {   new Point(-1, -1), 
                                    new Point(-1, 1), 
                                    new Point(-1, 1), 
                                    new Point(-1, 1), 
                                    new Point(-1, 1) };

        private Point[] Path3 = {   new Point(1, -1), 
                                    new Point(1, 1), 
                                    new Point(1, 1), 
                                    new Point(1, 1), 
                                    new Point(1, 1) };


        //reset all the values
        public void resetDot()
        {
            my_state = DotState.WAIT;
            isStartAnimation = false;
            AnimationLifeTimer = 0;
            currentFragmentStage = -1;
        }


        public void startAnimation()
        {
            isStartAnimation = true;
            my_AnimationBlockNums = my_EachLineBlockNums;

            my_animationMode = AnimationMode.BlockMode;
            //my_animationMode = AnimationMode.FragmentMode;
        }

        
        public byte[] NormalRenderingDot(byte[] temp, int width, int bytePerPixel)
        {
            DepthProcessManager tempManager = new DepthProcessManager();

            //int positionIndex = (dot.XPosition + (dot.YPosition * width)) * bytePerPixel;
            Dot dot = this;
            //get the dot postion in the left top of the big dot
            //int leftTopPositionIndex = (dot.XPosition - dot.BlockNum / 2 * dot.BlockLength + ((dot.YPosition - dot.BlockNum / 2 * dot.BlockLength) * width)) * bytePerPixel;
            int leftTopDotPositionX = dot.XPosition - dot.BlockNum / 2 * dot.BlockLength;
            int leftTopDotPositionY = dot.YPosition - dot.BlockNum / 2 * dot.BlockLength;
            for (int y = 0; y < dot.BlockNum; y++)
            {
                for (int x = 0; x < dot.BlockNum; x++)
                {
                    int positionIndex = (leftTopDotPositionX + x * dot.BlockLength + ((leftTopDotPositionY + y * dot.BlockLength) * width)) * bytePerPixel;
                    if (tempManager.isNotOutOfLimitation(positionIndex, 0, temp.Length))
                    {
                        // b g r => yellow
                        temp[positionIndex] = 111;
                        temp[positionIndex + 1] = 247;
                        temp[positionIndex + 2] = 236;
                    }
                }
            }
            return temp;
        }



        public byte[] AnimationRenderingDot(byte[] temp, int width, int bytePerPixel)
        {
            UpdateAnimation();
            DepthProcessManager tempManager = new DepthProcessManager();
            Dot dot = this;

            switch (my_animationMode)
            {
                case AnimationMode.BlockMode:
                    int leftTopDotPositionX = dot.XPosition - dot.AnimationBlockNum / 2 * dot.BlockLength;
                    int leftTopDotPositionY = dot.YPosition - dot.AnimationBlockNum / 2 * dot.BlockLength;
                    for (int y = 0; y < dot.AnimationBlockNum; y++)
                    {
                        for (int x = 0; x < dot.AnimationBlockNum; x++)
                        {
                            int positionIndex = (leftTopDotPositionX + x * dot.BlockLength + ((leftTopDotPositionY + y * dot.BlockLength) * width)) * bytePerPixel;
                            if (tempManager.isNotOutOfLimitation(positionIndex, 0, temp.Length))
                            {
                                // b g r => yellow
                                temp[positionIndex] = (byte)dot.ColorBGR[0];
                                temp[positionIndex + 1] = (byte)dot.ColorBGR[1];
                                temp[positionIndex + 2] = (byte)dot.ColorBGR[2];
                            }
                        }
                    }
                    break;
                case AnimationMode.FragmentMode:
                    temp = PlayAnimationFragment(temp, width, bytePerPixel, tempManager);
                    break;
            }


            return temp;
        }



        private void UpdateAnimation()
        {
            if (isStartAnimation)
            {
                countAnimationTime();
                //UpdateAnimation
                //in fact change the my_AnimationBlockNums to change the rendering block number
                if (my_animationMode == AnimationMode.BlockMode)
                {
                    setAnimationBlockNumByTime();
                    ChangeAnimationMode();
                }
            }
        }
        private void ChangeAnimationMode()
        {
            if (AnimationLifeTimer > AnimationBlockNumChangingSpeed * AnimationBlockNumChangeTimes)
            {
                my_animationMode = AnimationMode.FragmentMode;
                currentFragmentAnimationTime = AnimationLifeTimer;
                currentFragmentStage = -1;
            }
        }
        private int setAnimationBlockNumByTime()
        {
            if (AnimationLifeTimer % AnimationBlockNumChangingSpeed == 0)
            {
                //expand in the center
                my_AnimationBlockNums += 2;
            }
            return my_AnimationBlockNums;
        }
        private void countAnimationTime()
        {
            AnimationLifeTimer++;
        }
        private void initFragmentMovePathList()
        {
            //4 path for 4 fragments
            FragmentMovePathList = new List<Point[]>(4);

            //0
            FragmentMovePathList.Add(Path0);
            //1
            FragmentMovePathList.Add(Path1);
            //2
            FragmentMovePathList.Add(Path2);
            //3
            FragmentMovePathList.Add(Path3);



        }

        //set 4 fragments
        //set 5 frame for test.
        private byte[] PlayAnimationFragment(byte[] temp, int width, int bytePerPixel, DepthProcessManager tempManager)
        {
            if (currentFragmentStage == -1)
            {
                //init fragment positions
                //int leftTopDotPositionX = dot.XPosition - dot.AnimationBlockNum / 2 * dot.BlockLength;
                //0
                Point tempPoint = new Point(this.XPosition - 1 * this.BlockLength, this.YPosition - 1 * this.BlockLength);
                FragmentsPosition[0] = tempPoint;
                //1
                tempPoint = new Point(this.XPosition + 1 * this.BlockLength, this.YPosition - 1 * this.BlockLength);
                FragmentsPosition[1] = tempPoint;
                //2
                tempPoint = new Point(this.XPosition - 2 * this.BlockLength, this.YPosition + 1 * this.BlockLength);
                FragmentsPosition[2] = tempPoint;
                //3
                tempPoint = new Point(this.XPosition + 2 * this.BlockLength, this.YPosition + 1 * this.BlockLength);
                FragmentsPosition[3] = tempPoint;
            }
            else
            {
                for (int i = 0; i < FragmentsPosition.Count(); i++)
                {

                    FragmentsPosition[i].X += FragmentMovePathList[i][currentFragmentStage].X * this.BlockLength;
                    FragmentsPosition[i].Y += FragmentMovePathList[i][currentFragmentStage].Y * this.BlockLength;

                }
            }

            //render

            for (int i = 0; i < FragmentsPosition.Count(); i++)
            {
                if (FragmentsPosition[i].X >= 0 && FragmentsPosition[i].Y >= 0)
                {
                    int positionIndex = (int)(FragmentsPosition[i].X + (FragmentsPosition[i].Y * width)) * bytePerPixel;
                    if (tempManager.isNotOutOfLimitation(positionIndex, 0, temp.Length))
                    {
                        // b g r => yellow
                        temp[positionIndex] = (byte)this.ColorBGR[0];
                        temp[positionIndex + 1] = (byte)this.ColorBGR[1];
                        temp[positionIndex + 2] = (byte)this.ColorBGR[2];
                    }
                }

            }
            //update stage
            if (currentFragmentStage == -1 || AnimationLifeTimer > currentFragmentAnimationTime + FragmentAnimationFrameTime)
            {
                currentFragmentStage++;
                currentFragmentAnimationTime = AnimationLifeTimer;

                if (currentFragmentStage >= 5)
                {
                    EndAnimation();
                }
            }

            return temp;


        }

        private void EndAnimation()
        {
            my_state = DotState.DISAPPEAR;
        }


















        public Dot()
        {
            my_x = 0;
            my_y = 0;
            my_EachBlocklength = 0;
            my_EachLineBlockNums = 1;
            my_state = DotState.WAIT;
        }
        public Dot(int x, int y, int length,int num)
        {
            my_x = x;
            my_y = y;
            my_EachBlocklength = length;
            my_EachLineBlockNums = num;
            my_state = DotState.WAIT;

            my_ColorBGR = new int[3];
            my_ColorBGR[0] = 111;
            my_ColorBGR[1] = 247;
            my_ColorBGR[2] = 236;


            FragmentsPosition = new Point[4];
            initFragmentMovePathList();
        }

        public AnimationMode animationMode
        {
            get
            {
                return my_animationMode;
            }
        }
        public int[] ColorBGR
        {
            get
            {
                return my_ColorBGR;
            }
        }

        public int XPosition
        {
            get 
            {
                return my_x;
            }
            set
            {
                my_x = value;
                //my_state = DotState.WAIT;
            }
        }
        public int YPosition
        {
            get
            {
                return my_y;
            }
            set
            {
                my_y = value;
                //my_state = DotState.WAIT;
            }
        }
        public int BlockLength
        {
            get
            {
                return my_EachBlocklength;
            }
            set
            {
                my_EachBlocklength = value;
                //my_state = DotState.WAIT;
            }
        }
        public int BlockNum
        {
            get
            {
                return my_EachLineBlockNums;
            }
            set
            {
                my_EachLineBlockNums = value;
                //my_state = DotState.WAIT;
            }
        }

        public int AnimationBlockNum
        {
            get
            {
                return my_AnimationBlockNums;
            }
            set
            {
                my_AnimationBlockNums = value;
                //my_state = DotState.WAIT;
            }
        }

        public DotState dotState
        {
            get
            {
                return my_state;
            }
            set
            {
                my_state = value;
            }
        }




    }
}
