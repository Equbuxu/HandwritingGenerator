using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    static class MathUtils
    {
        /// <summary>Returns an angle between 0 and 2*PI</summary>
        public static double NormalizeAngle(double angle)
        {
            if (angle < 0)
            {
                double factor = Math.Floor(Math.Abs(angle / Math.PI * 2)) + 1;
                angle += Math.PI * 2 * factor;
            }
            angle = angle % (Math.PI * 2);
            return angle;
        }

        public static int Constrain(int value, int bot, int top)
        {
            if (value < bot)
                return bot;
            if (value > top)
                return top;
            return value;
        }
    }
}
