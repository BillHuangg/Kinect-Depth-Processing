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
        private int my_length;
        private DotState my_state;
        public Dot()
        {
            my_x = 0;
            my_y = 0;
            my_length = 0;
            my_state = DotState.BLANK;
        }
        public Dot(int x, int y, int length)
        {
            my_x = x;
            my_y = y;
            my_length = length;
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
        public int Length
        {
            get
            {
                return my_length;
            }
            set
            {
                my_length = value;
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
