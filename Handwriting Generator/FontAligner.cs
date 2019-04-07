using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    class FontAligner
    {
        private static readonly List<FChar> smallLetters = new List<FChar>()
            {
                FChar.rus_1,
                FChar.rus_4,
                FChar.rus_6,
                FChar.rus_7,
                FChar.rus_8,
                FChar.rus_10,
                FChar.rus_11,
                FChar.rus_12,
                FChar.rus_13,
                FChar.rus_14,
                FChar.rus_15,
                FChar.rus_16,
                FChar.rus_17,
                FChar.rus_18,
                FChar.rus_19,
                FChar.rus_20,
                FChar.rus_22,
                FChar.rus_23,
                FChar.rus_24,
                FChar.rus_25,
                FChar.rus_26,
                FChar.rus_27,
                FChar.rus_28,
                FChar.rus_29,
                FChar.rus_30,
                FChar.rus_31,
                FChar.rus_32,
                FChar.rus_33,
            };


        Font font;
        Font alignedFont;
        bool aligned = false;

        double coreRegionTop = 0;
        double coreRegionBottom = Font.imageCmH;

        public FontAligner(Font font)
        {
            foreach (FChar letter in smallLetters)
            {
                if (!font.images.Keys.Contains(letter))
                    throw new FormatException("Font must contain small russian letters for alignment");
            }
            this.font = font;
        }

        /// <summary>
        /// Recalculates margins in font. Operates on passed font.
        /// </summary>
        public void Align()
        {
            if (aligned)
                return;
            aligned = true;

            FindCoreRegion();

            foreach (KeyValuePair<FChar, List<Bitmap>> item in font.images)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    double leftMargin = FindMargin(item.Value[i], false, false);
                    double rightMargin = FindMargin(item.Value[i], true, false);
                    if (leftMargin == -1)
                        leftMargin = FindMargin(item.Value[i], false, true);
                    if (rightMargin == -1)
                        rightMargin = FindMargin(item.Value[i], true, true);

                    font.leftMargins[item.Key][i] = leftMargin;
                    font.rightMargins[item.Key][i] = rightMargin;
                }
            }
        }

        private void FindCoreRegion()
        {
            int[] histogram = new int[Font.imagePixelH];

            foreach (FChar letter in smallLetters)
            {
                for (int i = 0; i < font.images[letter].Count; i++)
                {
                    Bitmap image = font.images[letter][i];
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

                    unsafe
                    {
                        byte* arr = (byte*)data.Scan0;
                        int channelCount = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;

                        for (int j = 0; j < image.Width * image.Height * channelCount; j += channelCount)
                        {
                            if (BitmapUtils.GetGrayscale(arr[j], arr[j + 1], arr[j + 2]) < 128 && arr[j + 3] > 240)
                                histogram[j / channelCount / image.Width]++;
                        }
                    }

                    image.UnlockBits(data);
                }
            }

            int[] cumulativeHistogram = new int[Font.imagePixelH];
            cumulativeHistogram[0] = histogram[0];

            for (int i = 1; i < cumulativeHistogram.Length; i++)
            {
                cumulativeHistogram[i] = cumulativeHistogram[i - 1] + histogram[i];
            }

            double cutoffRatio = 0.05;
            int total = cumulativeHistogram.Last();

            int leftBorder = 0;
            for (int i = 0; i < cumulativeHistogram.Length; i++)
            {
                if ((double)cumulativeHistogram[i] / total > cutoffRatio)
                {
                    leftBorder = i;
                    break;
                }
            }

            int rightBorder = cumulativeHistogram.Length;
            for (int i = cumulativeHistogram.Length - 1; i >= 0; i--)
            {
                if ((double)cumulativeHistogram[i] / total < (1.0 - cutoffRatio))
                {
                    rightBorder = i;
                    break;
                }
            }

            coreRegionTop = leftBorder * Font.cmPerPixelV;
            coreRegionBottom = rightBorder * Font.cmPerPixelV;

        }

        /// <summary>
        /// Finds a left or a right margin depending on the second argument
        /// </summary>
        private double FindMargin(Bitmap image, bool right, bool useFullImage)
        {
            Bitmap downscaled = useFullImage ? BitmapUtils.Resize(image, 80, 80) : image;
            int pixelMargin = downscaled.Width - 1;
            bool found = false;

            int iStartValue = right ? downscaled.Width - 1 : 0;
            int increment = right ? -1 : 1;
            int iEndValue = right ? -1 : downscaled.Width;

            int jStartValue = useFullImage ? 0 : (int)(coreRegionTop * Font.pixelsPerCmV);
            int jEndValue = useFullImage ? downscaled.Height : (int)(coreRegionBottom * Font.pixelsPerCmV);

            for (int i = iStartValue; i != iEndValue; i += increment)
            {
                for (int j = jStartValue; j < jEndValue; j++)
                {
                    if (IsPartOfTheLetter(downscaled, i, j))
                    {
                        pixelMargin = i;
                        found = true;
                        goto loopend;
                    }
                }
            }
        loopend:
            //image.SetPixel(pixelMargin * Font.imagePixelW / downscaled.Width, 220, Color.Red);
            if (!found)
                return -1;
            double cmMargin = (pixelMargin * Font.imagePixelW / downscaled.Width) * Font.imageCmW / Font.imagePixelW;
            return cmMargin;
        }

        private static bool IsPartOfTheLetter(Bitmap downscaled, int x, int y)
        {
            const int marginsBlackThreshold = 200;
            const int marginsTransparentThreshold = 240;

            int suit = 0;
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (i < 0 || j < 0 || i >= downscaled.Width || j >= downscaled.Height)
                        continue;
                    Color pixel = downscaled.GetPixel(i, j);
                    if (pixel.A > marginsBlackThreshold && BitmapUtils.GetGrayscale(pixel) < marginsBlackThreshold)
                        suit++;
                }
            }

            return suit > 2;
        }
    }
}
