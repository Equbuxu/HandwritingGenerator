using System;
using System.Collections.Generic;
using System.Drawing;
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
        public Dictionary<fChar, List<Bitmap>> images;
        public Dictionary<fChar, List<double>> leftMargins;
        public Dictionary<fChar, List<double>> rightMargins;

        public Font(Dictionary<fChar, List<Bitmap>> images, Dictionary<fChar, List<double>> leftMargins, Dictionary<fChar, List<double>> rightMargins)
        {
            this.images = images;
            this.leftMargins = leftMargins;
            this.rightMargins = rightMargins;
        }

        public Font(string path)
        {
            images = new Dictionary<fChar, List<Bitmap>>();
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

            //Load margins
            XElement file = XElement.Load("TempFontStorage/margins.xml");
            var leftPairs = file.Descendants().Where((x) => x.Name == "leftMargins").First().Descendants().Where((elem) => elem.Name == "pair");
            leftMargins = new Dictionary<fChar, List<double>>();
            foreach (XElement pair in leftPairs)
            {
                fChar key = (fChar)Enum.Parse(typeof(fChar), pair.Attribute("key").Value);
                if (!leftMargins.ContainsKey(key))
                    leftMargins.Add(key, new List<double>());
                leftMargins[key].AddRange(pair.Descendants().Select((item) => double.Parse(item.Attribute("value").Value)));
            }

            var rightPairs = file.Descendants().Where((x) => x.Name == "rightMargins").First().Descendants().Where((elem) => elem.Name == "pair");
            rightMargins = new Dictionary<fChar, List<double>>();
            foreach (XElement pair in rightPairs)
            {
                fChar key = (fChar)Enum.Parse(typeof(fChar), pair.Attribute("key").Value);
                if (!rightMargins.ContainsKey(key))
                    rightMargins.Add(key, new List<double>());
                rightMargins[key].AddRange(pair.Descendants().Select((item) => double.Parse(item.Attribute("value").Value)));
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
