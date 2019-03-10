using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Handwriting_Generator
{
    public class Font
    {
        public Dictionary<FChar, List<Bitmap>> images;
        public Dictionary<FChar, List<double>> leftMargins;
        public Dictionary<FChar, List<double>> rightMargins;

        public const int imagePixelW = 220;
        public const int imagePixelH = 440;

        public const double imageCmW = 1.5;
        public const double imageCmH = 3;
        public const double lineHeight = 1.5;

        public const double pixelsPerCmH = imagePixelW / imageCmW;
        public const double pixelsPerCmV = imagePixelH / imageCmH;

        public const double cmPerPixelH = imageCmW / imagePixelW;
        public const double cmPerPixelV = imageCmH / imagePixelH;

        public Font(Dictionary<FChar, List<Bitmap>> images, Dictionary<FChar, List<double>> leftMargins, Dictionary<FChar, List<double>> rightMargins)
        {
            this.images = images;
            this.leftMargins = leftMargins;
            this.rightMargins = rightMargins;
        }

        public Font(string path)
        {
            images = new Dictionary<FChar, List<Bitmap>>();
            Load(path);
        }

        private void Load(string path)
        {
            //Create temp directory
            if (Directory.Exists("TempFontStorage"))
                Directory.Delete("TempFontStorage", true);

            Directory.CreateDirectory("TempFontStorage");

            //Extract zip
            ZipFile.ExtractToDirectory(path, "TempFontStorage");

            //Load images
            string[] folders = Directory.GetDirectories("TempFontStorage");

            foreach (string folder in folders)
            {
                FChar key = (FChar)int.Parse(Path.GetFileName(folder));
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

            //Load margins
            XElement file = XElement.Load("TempFontStorage/margins.xml");
            var leftPairs = file.Descendants().Where((x) => x.Name == "leftMargins").First().Descendants().Where((elem) => elem.Name == "pair");
            leftMargins = new Dictionary<FChar, List<double>>();
            foreach (XElement pair in leftPairs)
            {
                FChar key = (FChar)Enum.Parse(typeof(FChar), pair.Attribute("key").Value);
                if (!leftMargins.ContainsKey(key))
                    leftMargins.Add(key, new List<double>());
                leftMargins[key].AddRange(pair.Descendants().Select((item) => double.Parse(item.Attribute("value").Value, CultureInfo.InvariantCulture)));
            }

            var rightPairs = file.Descendants().Where((x) => x.Name == "rightMargins").First().Descendants().Where((elem) => elem.Name == "pair");
            rightMargins = new Dictionary<FChar, List<double>>();
            foreach (XElement pair in rightPairs)
            {
                FChar key = (FChar)Enum.Parse(typeof(FChar), pair.Attribute("key").Value);
                if (!rightMargins.ContainsKey(key))
                    rightMargins.Add(key, new List<double>());
                rightMargins[key].AddRange(pair.Descendants().Select((item) => double.Parse(item.Attribute("value").Value, CultureInfo.InvariantCulture)));
            }

            //Delete temp directory
            if (Directory.Exists("TempFontStorage"))
                Directory.Delete("TempFontStorage", true);
        }

        public void Save(string path)
        {


            //Make temp directory
            if (Directory.Exists("TempFontStorage"))
                Directory.Delete("TempFontStorage", true);

            Directory.CreateDirectory("TempFontStorage");

            //Save images
            foreach (KeyValuePair<FChar, List<Bitmap>> bitmaps in images)
            {
                int i = 0;
                Directory.CreateDirectory("TempFontStorage/" + (int)bitmaps.Key);
                foreach (Bitmap image in bitmaps.Value)
                {
                    image.Save("TempFontStorage/" + (int)bitmaps.Key + "/" + i.ToString() + ".png");
                    i++;
                }
            }

            //Save margins

            XElement leftMarginsXml = new XElement(
                "leftMargins", leftMargins.Select((x) => new XElement(
                    "pair", x.Value.Select((item) => new XElement(
                        "item", new XAttribute("value", item)
                        )), new XAttribute("key", x.Key))));

            XElement rightMarginsXml = new XElement(
                "rightMargins", rightMargins.Select((x) => new XElement(
                    "pair", x.Value.Select((item) => new XElement(
                        "item", new XAttribute("value", item)
                        )), new XAttribute("key", x.Key))));

            XElement file = new XElement("margins", leftMarginsXml, rightMarginsXml);
            file.Save("TempFontStorage/margins.xml");

            //Compress
            if (File.Exists(path))
                File.Delete(path);
            ZipFile.CreateFromDirectory("TempFontStorage", path);

            //Delete temp directory
            Directory.Delete("TempFontStorage", true);
        }
    }
}
