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
        const double realHeight = 250.0; //mm

        const double xOffset1 = 2.5 / realWidth; //first column
        const double xOffset2 = 57.5 / realWidth; //second column
        const double xOffset3 = 112.5 / realWidth; //third column
        const double yOffset = 7.5 / realHeight;

        const double cellWidth = 15.0 / realWidth;
        const double cellHeight = 15.0 / realHeight;
        const int sampleCount = 3; //the amount of samples of each character
        const int charCount = 24 * 4;

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
            for (int i = 0; i < charCount; i++)
                result.Add(new List<Bitmap>());

            for (int i = 0; i < matrixW; i++)
            {
                if (i % 4 == 3) //skip cells with printed labels
                    continue;
                for (int j = 0; j < matrixH; j++)
                {
                    int characterId = j + (i / 4) * matrixH;
                    int x = (int)(form.Width * (topLeftX + i * cellWidth));
                    int y = (int)(form.Height * (topLeftY + j * cellHeight));
                    int w = (int)(form.Width * cellWidth);
                    int h = (int)(form.Width * cellWidth);
                    result[characterId].Add(BitmapUtils.CutOutPiece(form, x, y, w, h));
                }
            }

            return result;
        }


    }
}
