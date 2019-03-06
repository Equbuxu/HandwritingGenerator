using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    class FormCutter
    {
        private Bitmap form;
        bool done = false;

        const double realWidth = 165.0; //mm
        const double realHeight = 255.0; //mm

        //first column offsets
        const double xOffset = 2.5 / realWidth;
        const double yOffset = 7.5 / realHeight;
        const double columnDist = 55.0 / realWidth; //distance between columns

        const double cellWidth = 15.0 / realWidth;
        const double cellHeight = 15.0 / realHeight;
        const int sampleCount = 3; //the amount of samples of each character
        const int rowCount = 16;
        const int columnCount = 3;

        List<List<Bitmap>> result;

        public FormCutter(Bitmap preparedForm)
        {
            form = preparedForm;
        }

        public List<List<Bitmap>> Cut()
        {
            if (done)
                return result;
            done = true;

            result = new List<List<Bitmap>>();

            for (int i = 0; i < columnCount; i++)
            {
                result.AddRange(CutColumn(i));
            }

            return result;
        }

        private List<List<Bitmap>> CutColumn(int index)
        {
            double columnXOffset = index * columnDist + xOffset;

            List<List<Bitmap>> images = new List<List<Bitmap>>();

            for (int i = 0; i < rowCount; i++)
            {
                images.Add(new List<Bitmap>());
            }

            int w = (int)(form.Width * cellWidth);
            int h = (int)(form.Width * cellWidth);

            for (int j = 0; j < rowCount; j++)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    int x = (int)((cellWidth * i + columnXOffset) * form.Width);
                    int y = (int)((cellHeight * j + yOffset) * form.Height);
                    images[j].Add(BitmapUtils.CutOutPiece(form, x, y, w, h));
                }
            }
            return images;
        }

    }
}
