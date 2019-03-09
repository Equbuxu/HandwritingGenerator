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
        private Dictionary<fChar, List<Bitmap>> images = new Dictionary<fChar, List<Bitmap>>();
        public Dictionary<fChar, List<double>> leftMargins = new Dictionary<fChar, List<double>>();
        public Dictionary<fChar, List<double>> rightMargins = new Dictionary<fChar, List<double>>();

        List<fChar[]> formTranslationTables = new List<fChar[]>();
        List<double> lineHeights = new List<double>(); //from the top

        private const double borderCutThickness = 13.0 / 235.0;

        private const int standartLetterPixelW = 220;
        private const int standartLetterPixelH = 440;

        private const double standartLetterCmW = 1.5;
        private const double standartLetterCmH = 3;

        public FontCreator()
        {
            formTranslationTables.Add(new fChar[48]
            {
            fChar.rus_1_cap,fChar.rus_2_cap,fChar.rus_3_cap,fChar.rus_4_cap,fChar.rus_5_cap,fChar.rus_6_cap,fChar.rus_7_cap,fChar.rus_8_cap,
            fChar.rus_9_cap,fChar.rus_10_cap,fChar.rus_11_cap,fChar.rus_12_cap,fChar.rus_13_cap,fChar.rus_14_cap,fChar.rus_15_cap,fChar.rus_16_cap,

            fChar.rus_17_cap,fChar.rus_18_cap,fChar.rus_19_cap,fChar.rus_20_cap,fChar.rus_21_cap,fChar.rus_22_cap,fChar.rus_23_cap,fChar.rus_24_cap,
            fChar.rus_25_cap,fChar.rus_26_cap,fChar.rus_27_cap,fChar.rus_28_cap,fChar.rus_29_cap,fChar.rus_30_cap,fChar.rus_31_cap,fChar.rus_32_cap,

            fChar.rus_33_cap,fChar.period,fChar.exclamation_mark,fChar.question_mark,fChar.number,fChar.lower_quote,fChar.upper_quote,fChar.minus,
            fChar.plus,fChar.equals,fChar.slash,fChar.open_parenthesis,fChar.close_parenthesis,fChar.semicolon,fChar.email,fChar.hash,
            });

            formTranslationTables.Add(new fChar[48]
            {
            fChar.rus_1, fChar.rus_2,fChar.rus_3,fChar.rus_4,fChar.rus_5,fChar.rus_6,fChar.rus_7,fChar.rus_8,
            fChar.rus_9,fChar.rus_10,fChar.rus_11,fChar.rus_12,fChar.rus_13,fChar.rus_14,fChar.rus_15,fChar.rus_16,

            fChar.rus_17, fChar.rus_18,fChar.rus_19,fChar.rus_20,fChar.rus_21,fChar.rus_22,fChar.rus_23,fChar.rus_24,
            fChar.rus_25,fChar.rus_26,fChar.rus_27,fChar.rus_28,fChar.rus_29,fChar.rus_30,fChar.rus_31,fChar.rus_32,

            fChar.rus_33, fChar.dollar,fChar.ampersand,fChar.backslash,fChar.open_square_bracket,fChar.close_square_bracket,fChar.open_curly_bracket,fChar.close_curly_bracket,
            fChar.less_than,fChar.greater_than,fChar.percent,fChar.tilde,fChar.underscore,fChar.space,fChar.space,fChar.space,
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
                fChar key = formTranslationTables[formType][i];
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
            BitmapData data = downscaled.LockBits(new Rectangle(0, 0, downscaled.Width, downscaled.Height), ImageLockMode.ReadWrite, downscaled.PixelFormat);
            int pixelMargin = -1;
            unsafe
            {
                byte* arr = (byte*)data.Scan0;
                int channelCount = Bitmap.GetPixelFormatSize(downscaled.PixelFormat) / 8;
                for (int i = 0; i < downscaled.Width; i++)
                {
                    for (int j = 0; j < downscaled.Height; j++)
                    {
                        int pos = j * downscaled.Width * channelCount + i * channelCount;
                        if (BitmapUtils.GetGrayscale(arr[pos], arr[pos + 1], arr[pos + 2]) < 15 && arr[pos + 3] > 240)
                        {
                            pixelMargin = i;
                            goto loopend;
                        }
                    }
                }
            loopend:;
            }
            downscaled.UnlockBits(data);
            double cmMargin = (pixelMargin * standartLetterPixelW / downscaled.Width) * standartLetterCmW / standartLetterPixelW;
            return cmMargin;
        }

        private double FindRightMargin(Bitmap image)
        {
            Bitmap downscaled = BitmapUtils.Resize(image, 80, 80);
            BitmapData data = downscaled.LockBits(new Rectangle(0, 0, downscaled.Width, downscaled.Height), ImageLockMode.ReadWrite, downscaled.PixelFormat);
            int pixelMargin = -1;
            unsafe
            {
                byte* arr = (byte*)data.Scan0;
                int channelCount = Bitmap.GetPixelFormatSize(downscaled.PixelFormat) / 8;
                for (int i = downscaled.Width - 1; i >= 0; i--)
                {
                    for (int j = 0; j < downscaled.Height; j++)
                    {
                        int pos = j * downscaled.Width * channelCount + i * channelCount;
                        if (BitmapUtils.GetGrayscale(arr[pos], arr[pos + 1], arr[pos + 2]) < 15 && arr[pos + 3] > 240)
                        {
                            pixelMargin = i;
                            goto loopend;
                        }
                    }
                }
            loopend:;
            }
            downscaled.UnlockBits(data);
            double cmMargin = (pixelMargin * standartLetterPixelW / downscaled.Width) * standartLetterCmW / standartLetterPixelW;
            return cmMargin;
        }

        private Bitmap PrepareLetterImage(Bitmap image, int formType)
        {
            Bitmap standartImage = new Bitmap(standartLetterPixelW, standartLetterPixelH, PixelFormat.Format32bppArgb);

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
                int topOffset = (int)(standartLetterPixelH / 2 - standartLetterPixelW * lineHeights[formType]);

                gr.Clear(Color.Transparent);
                gr.DrawImage(image, new Rectangle(0, topOffset, standartLetterPixelW, standartLetterPixelW));
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
