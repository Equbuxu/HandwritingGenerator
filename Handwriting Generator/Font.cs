using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class Font
    {
        public Dictionary<fChar, List<Bitmap>> images;

        public Font(Dictionary<fChar, List<Bitmap>> images)
        {
            this.images = images;
        }

        public Font(string path)
        {
            images = new Dictionary<fChar, List<Bitmap>>();
            Load(path);
        }

        private void Load(string path)
        {
            if (Directory.Exists("TempFontStorage"))
                Directory.Delete("TempFontStorage", true);

            Directory.CreateDirectory("TempFontStorage");
            ZipFile.ExtractToDirectory(path, "TempFontStorage");

            string[] folders = Directory.GetDirectories("TempFontStorage");

            foreach (string folder in folders)
            {
                fChar key = (fChar)int.Parse(Path.GetFileName(folder));
                List<Bitmap> value = new List<Bitmap>();
                string[] imagePaths = Directory.GetFiles(folder);
                foreach (string imagePath in imagePaths)
                {
                    Bitmap loadedBitmap;
                    using (Bitmap bitmap = new Bitmap(imagePath))
                    {
                        loadedBitmap = new Bitmap(bitmap);
                    }
                    value.Add(loadedBitmap);
                }
                images.Add(key, value);
            }

            if (Directory.Exists("TempFontStorage"))
                Directory.Delete("TempFontStorage", true);
        }

        public void Save(string path)
        {
            if (Directory.Exists("TempFontStorage"))
                Directory.Delete("TempFontStorage", true);

            Directory.CreateDirectory("TempFontStorage");
            foreach (KeyValuePair<fChar, List<Bitmap>> bitmaps in images)
            {
                int i = 0;
                Directory.CreateDirectory("TempFontStorage/" + (int)bitmaps.Key);
                foreach (Bitmap image in bitmaps.Value)
                {
                    image.Save("TempFontStorage/" + (int)bitmaps.Key + "/" + i.ToString() + ".png");
                    i++;
                }
            }

            if (File.Exists(path))
                File.Delete(path);
            ZipFile.CreateFromDirectory("TempFontStorage", path);

            Directory.Delete("TempFontStorage", true);
        }
    }
}
