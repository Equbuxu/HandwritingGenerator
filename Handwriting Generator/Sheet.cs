using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class Sheet
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        /// <summary>Distance from the top to the first line</summary>
        public double FirstLineHeight { get; set; }
        public double DistBetweenLines { get; set; }
        public int LineCount { get; set; }

        public Sheet Copy()
        {
            return new Sheet()
            {
                Width = Width,
                Height = Height,
                LeftMargin = LeftMargin,
                RightMargin = RightMargin,
                FirstLineHeight = FirstLineHeight,
                DistBetweenLines = DistBetweenLines,
                LineCount = LineCount,
            };
        }

        public static Sheet LeftLinedSheet()
        {
            Sheet sheet = new Sheet()
            {
                Width = 16.2,
                Height = 20.3,
                LeftMargin = 2.5,
                RightMargin = 0.1,
                FirstLineHeight = 1.55,
                DistBetweenLines = 0.8,
                LineCount = 23,
            };
            return sheet;
        }

        public static Sheet RightLinedSheet()
        {
            Sheet sheet = new Sheet()
            {
                Width = 16.2,
                Height = 20.3,
                LeftMargin = 0.1,
                RightMargin = 2.5,
                FirstLineHeight = 1.55,
                DistBetweenLines = 0.8,
                LineCount = 23,
            };
            return sheet;
        }

        public static Sheet LeftCheckeredSheet()
        {
            Sheet sheet = new Sheet()
            {
                Width = 16.5,
                Height = 20.3,
                LeftMargin = 2.0,
                RightMargin = 0.1,
                FirstLineHeight = 0.9,
                DistBetweenLines = 0.497435897,
                LineCount = 38,
            };
            return sheet;
        }

        public static Sheet RightCheckeredSheet()
        {
            Sheet sheet = new Sheet()
            {
                Width = 16.5,
                Height = 20.3,
                LeftMargin = 0.1,
                RightMargin = 2.0,
                FirstLineHeight = 0.9,
                DistBetweenLines = 0.497435897,
                LineCount = 38,
            };
            return sheet;
        }

        public static Sheet A4Sheet()
        {
            Sheet sheet = new Sheet()
            {
                Width = 21.0,
                Height = 29.7,
                LeftMargin = 1.0,
                RightMargin = 1.0,
                FirstLineHeight = 1.5,
                DistBetweenLines = 0.8,
                LineCount = 36,
            };
            return sheet;
        }
    }
}
