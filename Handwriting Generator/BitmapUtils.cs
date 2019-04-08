using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    static class BitmapUtils
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static System.Windows.Media.ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public static Bitmap LoadBitmap(string path)
        {
            Bitmap bitmap = new Bitmap(path);
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(newBitmap))
                    gr.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Dispose();
                return newBitmap;
            }
            return bitmap;
        }

        public static Bitmap MakeGrayscale(Bitmap bitmap)
        {
            Debug.Assert(bitmap.PixelFormat == PixelFormat.Format32bppArgb);

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
                    byte avg = arr[i + 3] == 0 ? (byte)255 : (byte)GetGrayscale(arr[i], arr[i + 1], arr[i + 2]);
                    newArr[i] = newArr[i + 1] = newArr[i + 2] = avg;
                    newArr[i + 3] = 255; //ignore alpha
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
            Debug.Assert(orig.PixelFormat == PixelFormat.Format32bppArgb);

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
        public static void Autocontrast(Bitmap bitmap, double cutoffRatio = 0.0005)
        {
            //Generate histograms
            //bitmap = MakeGrayscale(bitmap);
            Debug.Assert(bitmap.PixelFormat == PixelFormat.Format32bppArgb);
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

        /// <summary>
        /// Operates on the original image
        /// </summary>
        public static void ChangeBrightness(Bitmap bitmap, double brightness)
        {
            Debug.Assert(bitmap.PixelFormat == PixelFormat.Format32bppArgb);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            unsafe
            {
                byte* arr = (byte*)data.Scan0;
                int channelCount = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                for (int i = 0; i < bitmap.Width * bitmap.Height * channelCount; i += channelCount)
                {
                    arr[i] = (byte)MathUtils.Constrain((int)(arr[i] * brightness), 0, 255);
                    arr[i + 1] = (byte)MathUtils.Constrain((int)(arr[i + 1] * brightness), 0, 255);
                    arr[i + 2] = (byte)MathUtils.Constrain((int)(arr[i + 2] * brightness), 0, 255);
                }
            }

            bitmap.UnlockBits(data);
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

        public static bool IsBelowThreshold(Bitmap map, int x, int y, int threshold)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return false;
            return GetGrayscale(map.GetPixel(x, y).R, map.GetPixel(x, y).G, map.GetPixel(x, y).B) < threshold;
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
        public static int GetMinMaxMiddleColorThreshold(Bitmap bitmap, double cutoffRatio = 0.001)
        {
            Debug.Assert(bitmap.PixelFormat == PixelFormat.Format32bppArgb);

            int[] histogram = new int[256];

            BitmapData pixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            unsafe
            {
                byte* arr = (byte*)pixels.Scan0;
                int pixelSize = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;

                for (int i = 0; i < bitmap.Height * bitmap.Width * pixelSize; i += pixelSize)
                {
                    int grayscale = GetGrayscale(arr[i], arr[i + 1], arr[i + 2]);
                    histogram[grayscale]++;
                }
            }

            bitmap.UnlockBits(pixels);

            int[] cumulativeHistogram = new int[256];
            cumulativeHistogram[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cumulativeHistogram[i] = histogram[i] + cumulativeHistogram[i - 1];
            }

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


            return (leftCutoffBoundary + rightCutoffBoundary) / 2;
        }

        /// <summary>
        /// Takes full color image, brightness less than threshold = black, otherwise white
        /// </summary>
        public static Bitmap MakeBlackAndWhite(Bitmap bitmap, int threshold)
        {
            Debug.Assert(bitmap.PixelFormat == PixelFormat.Format32bppArgb);

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
                    byte newColor = GetGrayscale(arr[i], arr[i + 1], arr[i + 2]) < threshold ? (byte)0 : (byte)255;
                    newArr[i] = newArr[i + 1] = newArr[i + 2] = newColor;
                    if (pixelSize > 3)
                        newArr[i + 3] = 255;
                }
            }

            bitmap.UnlockBits(pixels);
            newBitmap.UnlockBits(newPixels);

            return newBitmap;
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

