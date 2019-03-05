using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class FontCreator
    {
        private Dictionary<string, List<Bitmap>> images = new Dictionary<string, List<Bitmap>>();

        private static string[] form0TranslationTable = new string[48]
        {
            "А", "Б","В","Г","Д","Е","Ё","Ж","З","И","Й","К","Л","М","Н","О",
            "П","Р","С","Т","У","Ф","Х","Ц","Ч","Ш","Щ","Ъ","Ы","Ь","Э","Ю",
            "Я", ".","!","?","№","loquote","upquote","-","+","=","/","(",")",";","@","#",
        };

        private static string[] form1TranslationTable = new string[48]
        {
            "а", "б","в","г","д","е","ё","ж","з","и","й","к","л","м","н","о",
            "п","р","с","т","у","ф","х","ц","ч","ш","щ","ъ","ы","ь","э","ю",
            "я", "$","&","\\","[","]","{","}","<",">","%","~","_","","","",
        };

        public FontCreator()
        {

        }

        /// <summary>
        /// Loads a font from unprepared form image or prev. exported file
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

            for (int i = 0; i < letters.Count; i++)
            {
                string key = formType == 0 ? form0TranslationTable[i] : form1TranslationTable[i];
                if (!images.ContainsKey(key))
                    images.Add(key, new List<Bitmap>());
                images[key].AddRange(letters[i]);
            }
        }

        public Font GetFont()
        {
            images["з"][0].Save("DebugOut/z.png");

            return new Font();
        }
    }
}
