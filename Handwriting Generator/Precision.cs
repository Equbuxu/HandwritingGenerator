using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    static class Precision
    {
        private const double acceptableDiffPerUnit = 0.03;
        public static bool ApproxEquals(double a, double b)
        {
            bool result = Math.Abs(a - b) < acceptableDiffPerUnit * Math.Max(a, b);
            return result;
        }
        public static bool DefBigger(double a, double b)
        {
            return a > b && !ApproxEquals(a, b);
        }
        public static bool DefSmaller(double a, double b)
        {
            return a < b && !ApproxEquals(a, b);
        }
    }
}
