using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    static class BitmapUtils
    {
        public static Bitmap MakeGrayscale(Bitmap bitmap)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            BitmapData pixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            BitmapData newPixels = newBitmap.LockBits(new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.ReadWrite, newBitmap.PixelFormat);
            unsafe
            {
                byte* arr = (byte*)pixels.Scan0;
                byte* newArr = (byte*)newPixels.Scan0;
                int pixelSize = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                if (pixelSize == 4)
                {
                    for (int i = 0; i < bitmap.Height * bitmap.Width * pixelSize; i += pixelSize)
                    {
                        byte avg = arr[i + 3] == 0 ? (byte)255 : (byte)((arr[i] + arr[i + 1] + arr[i + 2]) / 3);
                        newArr[i] = newArr[i + 1] = newArr[i + 2] = avg;
                        newArr[i + 3] = 255; //ignore alpha
                    }
                }
                else if (pixelSize == 3)
                {
                    for (int i = 0; i < bitmap.Height * bitmap.Width * pixelSize; i += pixelSize)
                    {
                        byte avg = (byte)((arr[i] + arr[i + 1] + arr[i + 2]) / 3);
                        newArr[i] = newArr[i + 1] = newArr[i + 2] = avg;
                    }
                }
                else
                {
                    throw new Exception("Weird image format"); //TODO write this code properly
                }
            }

            bitmap.UnlockBits(pixels);
            newBitmap.UnlockBits(newPixels);

            return newBitmap;
        }

        /// <summary>
        /// Resizes a bitmap using nearest interpolation
        /// </summary>
        public static Bitmap Resize(Bitmap orig, int w, int h)
        {
            Bitmap result = new Bitmap(w, h);
            Graphics graphics = Graphics.FromImage(result);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            graphics.DrawImage(orig, 0, 0, w, h);
            graphics.Dispose();

            return result;
        }

        /// <summary>
        /// Operates on the original image instead of returning new one.
        /// </summary>
        public static void Autocontrast(Bitmap bitmap)
        {
            //Generate histograms
            //bitmap = MakeGrayscale(bitmap);
            Bitmap downscaledBitmap = new Bitmap(bitmap, new Size(80, 120));


            int[] histogram = new int[256];
            for (int j = 0; j < downscaledBitmap.Height; j++)
            {
                for (int i = 0; i < downscaledBitmap.Width; i++)
                {
                    histogram[GetGrayscale(downscaledBitmap.GetPixel(i, j))]++;
                }
            }

            int[] cumulativeHistogram = new int[256];
            cumulativeHistogram[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cumulativeHistogram[i] = histogram[i] + cumulativeHistogram[i - 1];
            }

            //find cutoff boundaries
            double cutoffRatio = 0.0005;
            int totalPixels = cumulativeHistogram[255];
            int leftCutoffBoundary = 0;
            for (int i = 0; i < 256; i++)
            {
                if (cumulativeHistogram[i] / (double)totalPixels >= cutoffRatio)
                {
                    leftCutoffBoundary = i;
                    break;
                }
            }

            int rightCutoffBoundary = 255;
            for (int i = 255; i >= 0; i--)
            {
                if (cumulativeHistogram[i] / (double)totalPixels <= 1 - cutoffRatio)
                {
                    rightCutoffBoundary = i;
                    break;
                }
            }

            //modify the image

            BitmapData pixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            unsafe
            {
                byte* arr = (byte*)pixels.Scan0;
                int pixelSize = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                for (int i = 0; i < bitmap.Height * bitmap.Width * pixelSize; i += pixelSize)
                {
                    int r = arr[i];
                    int g = arr[i + 1];
                    int b = arr[i + 2];

                    int grayscale = GetGrayscale(r, g, b);
                    double factor = 1;
                    if (grayscale < leftCutoffBoundary)
                        factor = 0;
                    else if (grayscale > rightCutoffBoundary)
                        factor = 255 / (double)grayscale;
                    else
                    {
                        double rescaled = (grayscale - leftCutoffBoundary) * 255 / (rightCutoffBoundary - leftCutoffBoundary);
                        factor = rescaled / grayscale;
                        //Console.WriteLine(factor);
                    }
                    arr[i] = (byte)MathUtils.Constrain((int)(r * factor), 0, 255);
                    arr[i + 1] = (byte)MathUtils.Constrain((int)(g * factor), 0, 255);
                    arr[i + 2] = (byte)MathUtils.Constrain((int)(b * factor), 0, 255);
                }
            }

            bitmap.UnlockBits(pixels);
        }

        public static int GetGrayscale(Color c)
        {
            return GetGrayscale(c.R, c.G, c.B);
        }

        public static int GetGrayscale(int r, int g, int b)
        {
            return (r + g + b) / 3;
        }

        private static bool IsWhite(Bitmap map, int x, int y)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return false;
            return map.GetPixel(x, y).R == 255 && map.GetPixel(x, y).G == 255 && map.GetPixel(x, y).B == 255;
        }

        public static bool IsBelowThreshold(Bitmap map, int x, int y, byte threshold)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return false;
            return (map.GetPixel(x, y).R + map.GetPixel(x, y).G + map.GetPixel(x, y).B) / 3 < threshold;
        }

        /// <summary>
        /// Assumes a small image
        /// </summary>
        public static Bitmap ExpandWhite(Bitmap bitmap)
        {
            Bitmap expanded = new Bitmap(bitmap);

            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    if (IsWhite(bitmap, i - 1, j) ||
                        IsWhite(bitmap, i + 1, j) ||
                        IsWhite(bitmap, i, j - 1) ||
                        IsWhite(bitmap, i, j + 1) ||
                        IsWhite(bitmap, i + 1, j + 1) ||
                        IsWhite(bitmap, i - 1, j + 1) ||
                        IsWhite(bitmap, i + 1, j - 1) ||
                        IsWhite(bitmap, i - 1, j - 1))
                        expanded.SetPixel(i, j, Color.White);
                }
            }

            return expanded;
        }

        /// <summary>
        /// Finds darkest and brightest color in a bitmap and returns their average. Takes full color image, returns single grayscale value.
        /// </summary>
        public static byte GetMinMaxMiddleColorThreshold(Bitmap bitmap)
        {
            BitmapData pixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int min = int.MaxValue;
            int max = 0;
            unsafe
            {
                byte* arr = (byte*)pixels.Scan0;
                int pixelSize = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;

                for (int i = 0; i < bitmap.Height * bitmap.Width * pixelSize; i += pixelSize)
                {
                    int grayscale = GetGrayscale(arr[i], arr[i + 1], arr[i + 2]);
                    if (grayscale > max)
                        max = grayscale;
                    if (grayscale < min)
                        min = grayscale;
                }
            }

            bitmap.UnlockBits(pixels);

            return (byte)((max + min) / 2);
        }

        /// <summary>
        /// Assumes a grayscale image. Less than threshold = black, otherwise white
        /// </summary>
        public static Bitmap MakeBlackAndWhite(Bitmap bitmap, byte threshold)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            BitmapData pixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            BitmapData newPixels = newBitmap.LockBits(new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.ReadWrite, newBitmap.PixelFormat);
            unsafe
            {
                byte* arr = (byte*)pixels.Scan0;
                byte* newArr = (byte*)newPixels.Scan0;
                int pixelSize = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                for (int i = 0; i < bitmap.Height * bitmap.Width * pixelSize; i += pixelSize)
                {
                    byte newColor = arr[i] < threshold ? (byte)0 : (byte)255;
                    newArr[i] = newArr[i + 1] = newArr[i + 2] = newColor;
                    if (pixelSize > 3)
                        newArr[i + 3] = 255;
                }
            }

            bitmap.UnlockBits(pixels);
            newBitmap.UnlockBits(newPixels);

            return newBitmap;
        }

        /// <summary>
        /// Compares two bitmaps by putting bitmap2 over bitmap1 at a defined position. 0 - every pixel is different, 1 - every pixel matches.
        /// Assumes black and white bitmaps.
        /// </summary>
        /// <returns>A value between 0 and 1</returns>
        public static double CompareBitmaps(Bitmap bitmap1, Bitmap bitmap2, Point pos)
        {
            BitmapData data1 = bitmap1.LockBits(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), ImageLockMode.ReadWrite, bitmap1.PixelFormat);
            BitmapData data2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadWrite, bitmap2.PixelFormat);

            int maxDiff = bitmap2.Width * bitmap2.Height;
            int diff = 0;

            unsafe
            {
                byte* arr1 = (byte*)data1.Scan0;
                byte* arr2 = (byte*)data2.Scan0;
                int pixelSize1 = Bitmap.GetPixelFormatSize(bitmap1.PixelFormat) / 8;
                int pixelSize2 = Bitmap.GetPixelFormatSize(bitmap2.PixelFormat) / 8;
                for (int i = 0; i < bitmap2.Height * bitmap2.Width; i++)
                {
                    int add2 = i * pixelSize2;
                    int x = i % bitmap2.Width;
                    int y = i / bitmap2.Width;

                    int x1 = x + pos.X;
                    int y1 = y + pos.Y;

                    if (x1 >= 0 && y1 >= 0 && x1 < bitmap1.Width && y1 < bitmap1.Height)
                    {
                        int add1 = (bitmap1.Width * y1 + x1) * pixelSize1;
                        if (arr1[add1] != arr2[add2])
                            diff++;
                    }
                    else
                    {
                        diff++;
                    }
                }
            }

            bitmap1.UnlockBits(data1);
            bitmap2.UnlockBits(data2);

            return 1.0 - (double)diff / maxDiff;
        }

        public static Bitmap CutOutPiece(Bitmap from, int x, int y, int w, int h)
        {
            Bitmap result = new Bitmap(w, h);
            Graphics graphics = Graphics.FromImage(result);
            graphics.DrawImageUnscaled(from, -x, -y);
            graphics.Dispose();
            return result;
        }
    }
}

