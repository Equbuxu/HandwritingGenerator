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
        private Dictionary<string, List<Bitmap>> images = new Dictionary<string, List<Bitmap>>();

        private static string[] form0TranslationTable = new string[48]
        {
            "а_capital", "б_capital","в_capital","г_capital","д_capital","е_capital","ё_capital","ж_capital","з_capital","и_capital","й_capital","к_capital","л_capital","м_capital","н_capital","о_capital",
            "п_capital","р_capital","с_capital","т_capital","у_capital","ф_capital","х_capital","ц_capital","ч_capital","ш_capital","щ_capital","ъ_capital","ы_capital","ь_capital","э_capital","ю_capital",
            "я_capital", "period","exclamation_mark","question_mark","number","lower_quote","upper_quote","minus","plus","equals","slash","open_parenthesis","close_parenthesis","semicolon","email","hash",
        };

        private static string[] form1TranslationTable = new string[48]
        {
            "а", "б","в","г","д","е","ё","ж","з","и","й","к","л","м","н","о",
            "п","р","с","т","у","ф","х","ц","ч","ш","щ","ъ","ы","ь","э","ю",
            "я", "dollar","ampersand","backslash","open_square_bracket","close_square_bracket","open_curly_bracket","close_curly_bracket","less_than","greater_than","percent","tilde","underscore","null","null","null",
        };

        private const double form0LineHeight = 2.0 / 3.0; //from the top
        private const double form1LineHeight = 0.5; //from the top
        private const double borderCutThickness = 13.0 / 235.0;

        private const int standartLetterPixelW = 220;
        private const int standartLetterPixelH = 440;

        public FontCreator()
        {

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
                string key = formType == 0 ? form0TranslationTable[i] : form1TranslationTable[i];
                if (!images.ContainsKey(key))
                    images.Add(key, new List<Bitmap>());
                images[key].AddRange(letters[i]);
            }
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
                int topOffset = (int)(standartLetterPixelH / 2 - standartLetterPixelW * form0LineHeight); //form 0
                if (formType == 1)
                    topOffset = (int)(standartLetterPixelH / 2 - standartLetterPixelW * form1LineHeight); //form 1

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
            return new Font(images);
        }
    }
}
