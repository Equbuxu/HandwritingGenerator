using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class TextRenderer
    {
        private List<fChar> text;
        private List<Sheet> notebook;
        private Font font;

        private List<Bitmap> renderedText = new List<Bitmap>();
        private bool done = false;

        public TextRenderer(List<fChar> text, List<Sheet> notebook, Font font)
        {
            this.text = text;
            this.notebook = notebook;
            this.font = font;
        }

        private void Render()
        {
            if (done)
                return;
            done = true;

        }

        public Bitmap GetPage(int number)
        {
            Render();
            if (number > renderedText.Count)
                return null;
            if (number < 0)
                return null;
            return renderedText[number];
        }
    }
}
