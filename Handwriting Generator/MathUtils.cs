using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double LengthSq { get { return X * X + Y * Y; } }
        public double Length { get { return Math.Sqrt(LengthSq); } }
        public double Angle { get { return MathUtils.NormalizeAngle(Math.Atan2(Y, X)); } }

        public Vector(double x, double y) { X = x; Y = y; }

        public static Vector operator +(Vector l, Vector r)
        {
            return new Vector(l.X + r.X, l.Y + r.Y);
        }

        public static Vector operator -(Vector vector)
        {
            return new Vector(-vector.X, -vector.Y);
        }

        public static Vector operator -(Vector l, Vector r)
        {
            return l + (-r);
        }

        public static Vector operator *(Vector vector, double number)
        {
            return new Vector(vector.X * number, vector.Y * number);
        }

        public static Vector operator *(double number, Vector vector)
        {
            return vector * number;
        }

        /// <summary>Dot product of 2 vectors</summary>
        public static double operator *(Vector l, Vector r)
        {
            return l.X * r.X + l.Y * r.Y;
        }

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static bool operator !=(Vector v1, Vector v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(Object other)
        {
            if (other is Vector)
                return this == other as Vector;
            return base.Equals(other);
        }

        public Vector Rotate(double angle)
        {
            return new Vector(X * Math.Cos(angle) - Y * Math.Sin(angle), X * Math.Sin(angle) + Y * Math.Cos(angle));
        }
    }

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
