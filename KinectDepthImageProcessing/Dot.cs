using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectDepthImageProcessing
{
    enum DotState
    {
        BLANK=0,
        WAIT,
        HIT,
        CHANGE,
        DISAPPEAR,

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
        public Dot()
        {
            my_x = 0;
            my_y = 0;
            my_EachBlocklength = 0;
            my_EachLineBlockNums = 1;
            my_state = DotState.BLANK;
        }
        public Dot(int x, int y, int length,int num)
        {
            my_x = x;
            my_y = y;
            my_EachBlocklength = length;
            my_EachLineBlockNums = num;
            my_state = DotState.WAIT;
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
                my_state = DotState.WAIT;
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
                my_state = DotState.WAIT;
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
                my_state = DotState.WAIT;
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
                my_state = DotState.WAIT;
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
