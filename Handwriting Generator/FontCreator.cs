using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    /// <summary>
    /// Makes a font from forms
    /// </summary>
    public class FontCreator
    {
        private Dictionary<FChar, List<Bitmap>> images = new Dictionary<FChar, List<Bitmap>>();
        public Dictionary<FChar, List<double>> leftMargins = new Dictionary<FChar, List<double>>();
        public Dictionary<FChar, List<double>> rightMargins = new Dictionary<FChar, List<double>>();

        List<FChar[]> formTranslationTables = new List<FChar[]>();
        List<double> lineHeights = new List<double>(); //from the top

        private const double borderCutThickness = 13.0 / 235.0;

        public FontCreator()
        {
            formTranslationTables.Add(new FChar[48]
            {
            FChar.rus_1_cap,FChar.rus_2_cap,FChar.rus_3_cap,FChar.rus_4_cap,FChar.rus_5_cap,FChar.rus_6_cap,FChar.rus_7_cap,FChar.rus_8_cap,
            FChar.rus_9_cap,FChar.rus_10_cap,FChar.rus_11_cap,FChar.rus_12_cap,FChar.rus_13_cap,FChar.rus_14_cap,FChar.rus_15_cap,FChar.rus_16_cap,

            FChar.rus_17_cap,FChar.rus_18_cap,FChar.rus_19_cap,FChar.rus_20_cap,FChar.rus_21_cap,FChar.rus_22_cap,FChar.rus_23_cap,FChar.rus_24_cap,
            FChar.rus_25_cap,FChar.rus_26_cap,FChar.rus_27_cap,FChar.rus_28_cap,FChar.rus_29_cap,FChar.rus_30_cap,FChar.rus_31_cap,FChar.rus_32_cap,

            FChar.rus_33_cap,FChar.period,FChar.exclamation_mark,FChar.question_mark,FChar.number,FChar.lower_quote,FChar.upper_quote,FChar.minus,
            FChar.plus,FChar.equals,FChar.slash,FChar.open_parenthesis,FChar.close_parenthesis,FChar.semicolon,FChar.email,FChar.hash,
            });

            formTranslationTables.Add(new FChar[48]
            {
            FChar.rus_1, FChar.rus_2,FChar.rus_3,FChar.rus_4,FChar.rus_5,FChar.rus_6,FChar.rus_7,FChar.rus_8,
            FChar.rus_9,FChar.rus_10,FChar.rus_11,FChar.rus_12,FChar.rus_13,FChar.rus_14,FChar.rus_15,FChar.rus_16,

            FChar.rus_17, FChar.rus_18,FChar.rus_19,FChar.rus_20,FChar.rus_21,FChar.rus_22,FChar.rus_23,FChar.rus_24,
            FChar.rus_25,FChar.rus_26,FChar.rus_27,FChar.rus_28,FChar.rus_29,FChar.rus_30,FChar.rus_31,FChar.rus_32,

            FChar.rus_33, FChar.dollar,FChar.ampersand,FChar.backslash,FChar.open_square_bracket,FChar.close_square_bracket,FChar.open_curly_bracket,FChar.close_curly_bracket,
            FChar.less_than,FChar.greater_than,FChar.percent,FChar.tilde,FChar.underscore,FChar.comma,FChar.space,FChar.space,
            });

            lineHeights.Add(2.0 / 3.0);
            lineHeights.Add(0.5);
        }

        /// <summary>
        /// Loads a font from unprepared form image
        /// </summary>
        public void Add(string path)
        {
            if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
                AddFromImage(path);
        }

        private int GetFormType(Bitmap prepForm)
        {
            const double markerDist = 10.0 / 165.0;
            const double xOff = 10.0 / 165.0;
            const double yOff = 2.5 / 255.0;

            for (int i = 0; i < 2; i++)
            {
                double x = (xOff + i * markerDist) * prepForm.Width;
                double y = yOff * prepForm.Height;
                if (BitmapUtils.GetGrayscale(prepForm.GetPixel((int)x, (int)y)) < 128)
                    return i;
            }

            return -1;
        }

        private void AddFromImage(string path)
        {
            FormPreparer generator = new FormPreparer(path);
            Bitmap prepared = generator.CreatePrepared();
            FormCutter cutter = new FormCutter(prepared);
            List<List<Bitmap>> letters = cutter.Cut();

            int formType = GetFormType(prepared);

            foreach (List<Bitmap> bitmaps in letters)
            {
                for (int i = 0; i < bitmaps.Count; i++)
                {
                    bitmaps[i] = PrepareLetterImage(bitmaps[i], formType);
                }
            }

            for (int i = 0; i < letters.Count; i++)
            {
                //add images of a letter
                FChar key = formTranslationTables[formType][i];
                if (!images.ContainsKey(key))
                    images.Add(key, new List<Bitmap>());
                images[key].AddRange(letters[i]);

                //add offsets
                if (!leftMargins.ContainsKey(key))
                    leftMargins.Add(key, new List<double>());

                if (!rightMargins.ContainsKey(key))
                    rightMargins.Add(key, new List<double>());

                for (int j = 0; j < letters[i].Count; j++)
                {
                    double leftMargin = FindLeftMargin(letters[i][j]);
                    double rightMargin = FindRightMargin(letters[i][j]);
                    leftMargins[key].Add(leftMargin);
                    rightMargins[key].Add(rightMargin);
                }
            }
        }

        private double FindLeftMargin(Bitmap image)
        {
            Bitmap downscaled = BitmapUtils.Resize(image, 80, 80);
            int pixelMargin = downscaled.Width - 1;

            for (int i = 0; i < downscaled.Width; i++)
            {
                for (int j = 0; j < downscaled.Height; j++)
                {
                    if (IsPartOfTheLetter(downscaled, i, j))
                    {
                        pixelMargin = i;
                        goto loopend;
                    }
                }
            }
        loopend:
            //image.SetPixel(pixelMargin * Font.imagePixelW / downscaled.Width, 220, Color.Red);
            double cmMargin = (pixelMargin * Font.imagePixelW / downscaled.Width) * Font.imageCmW / Font.imagePixelW;
            return cmMargin;
        }

        private double FindRightMargin(Bitmap image)
        {
            Bitmap downscaled = BitmapUtils.Resize(image, 80, 80);
            int pixelMargin = downscaled.Width - 1;

            for (int i = downscaled.Width - 1; i >= 0; i--)
            {
                for (int j = 0; j < downscaled.Height; j++)
                {
                    if (IsPartOfTheLetter(downscaled, i, j))
                    {
                        pixelMargin = i;
                        goto loopend;
                    }
                }
            }
        loopend:
            //image.SetPixel(pixelMargin * Font.imagePixelW / downscaled.Width, 220, Color.Blue);
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

        private Bitmap PrepareLetterImage(Bitmap image, int formType)
        {
            Bitmap standartImage = new Bitmap(Font.imagePixelW, Font.imagePixelH, PixelFormat.Format32bppArgb);

            //remove black borders
            using (Graphics gr = Graphics.FromImage(image))
            {
                int pixelThicknessH = (int)(borderCutThickness * image.Width);
                int pixelThicknessV = (int)(borderCutThickness * image.Height);
                gr.FillRectangle(Brushes.White, new Rectangle(0, 0, pixelThicknessH, image.Height)); //left
                gr.FillRectangle(Brushes.White, new Rectangle(0, 0, image.Width, pixelThicknessV)); //top
                gr.FillRectangle(Brushes.White, new Rectangle(image.Width - pixelThicknessH, 0, pixelThicknessH, image.Height)); //right
                gr.FillRectangle(Brushes.White, new Rectangle(0, image.Height - pixelThicknessV, image.Width, image.Height)); //bottom
            }

            //copy over the image
            using (Graphics gr = Graphics.FromImage(standartImage))
            {
                int topOffset = (int)(Font.imagePixelH / 2 - Font.imagePixelW * lineHeights[formType]);

                gr.Clear(Color.Transparent);
                gr.DrawImage(image, new Rectangle(0, topOffset, Font.imagePixelW, Font.imagePixelW));
            }

            //make white transparent
            BitmapData data = standartImage.LockBits(new Rectangle(0, 0, standartImage.Width, standartImage.Height), ImageLockMode.ReadWrite, standartImage.PixelFormat);
            unsafe
            {
                byte* arr = (byte*)data.Scan0;
                int channelCount = Image.GetPixelFormatSize(standartImage.PixelFormat) / 8;
                for (int i = 0; i < standartImage.Width * standartImage.Height * channelCount; i += channelCount)
                {
                    int grayscale = BitmapUtils.GetGrayscale(arr[i], arr[i + 1], arr[i + 2]);
                    if (grayscale > 200)
                        arr[i + 3] = (byte)(255 - grayscale);
                }
            }
            standartImage.UnlockBits(data);

            return standartImage;
        }

        public Font GetFont()
        {
            return new Font(images, leftMargins, rightMargins);
        }
    }
}
