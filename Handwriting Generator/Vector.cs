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
}
