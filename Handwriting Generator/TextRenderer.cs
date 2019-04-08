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
        class RenderUnit
        {
            public FChar corrCharacter;
            public Bitmap image;
            public int x;
            public int rightBorderX;
        }

        class Paragraph
        {
            public List<FChar> text = new List<FChar>();
            public bool centered = false;
        }

        private const double letterDistance = 0;
        private const double spaceSize = 1;
        private const double tabSize = 2;

        private List<FChar> text;
        private List<Sheet> notebook;
        private Font font;
        private Random random = new Random();

        private List<RenderUnit> renderSequence = new List<RenderUnit>();

        private List<Bitmap> renderedText = new List<Bitmap>();
        private bool done = false;

        public TextRenderer(List<FChar> text, List<Sheet> notebook, Font font)
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

            CreateSheetImages();

            List<Paragraph> paragraphs = SplitIntoParagraphs(text);

            int curSheet = 0;
            int curLine = 0;
            foreach (Paragraph paragraph in paragraphs)
            {
                int pos = 0;
                while (pos < paragraph.text.Count)
                {
                    int endPos;
                    int lineWidth = (int)((notebook[curSheet].Width - notebook[curSheet].LeftMargin - notebook[curSheet].RightMargin) * Font.pixelsPerCmH);
                    List<RenderUnit> line = MakeLineRenderSequence(paragraph, font, random, pos, out endPos, lineWidth);
                    pos = endPos + 1;

                    RenderLine(line, curSheet, curLine);
                    curLine++;

                    int totalLines = notebook[curSheet].LineCount;
                    if (curLine >= totalLines)
                    {
                        curSheet++;
                        curLine = 0;
                    }

                    if (curSheet >= notebook.Count)
                        goto renderend;
                }
            }
        renderend:;
        }

        private void CreateSheetImages()
        {
            foreach (Sheet sheet in notebook)
            {
                Bitmap image = new Bitmap((int)(sheet.Width * Font.pixelsPerCmH), (int)(sheet.Height * Font.pixelsPerCmV));
                using (Graphics gr = Graphics.FromImage(image))
                {
                    gr.Clear(Color.Transparent);
                }
                renderedText.Add(image);
            }
        }

        private void RenderLine(List<RenderUnit> line, int sheetNumber, int lineNumber)
        {
            int xOff = (int)(notebook[sheetNumber].LeftMargin * Font.pixelsPerCmH);
            int yOff = (int)((notebook[sheetNumber].FirstLineHeight + notebook[sheetNumber].DistBetweenLines * lineNumber - Font.lineHeight) * Font.pixelsPerCmV);

            using (Graphics gr = Graphics.FromImage(renderedText[sheetNumber]))
            {
                foreach (RenderUnit unit in line)
                {
                    if (unit.image != null)
                        gr.DrawImageUnscaled(unit.image, new Point(unit.x + xOff, yOff));
                    /*if (unit.corrCharacter == FChar.linebreak)
                        gr.FillRectangle(Brushes.Red, unit.x + xOff, yOff + (int)(Font.lineHeight * Font.pixelsPerCmH - 100), 4, 100);*/
                }
            }
        }

        private static List<Paragraph> SplitIntoParagraphs(List<FChar> text)
        {
            List<Paragraph> result = new List<Paragraph>();

            int pos = 0;
            int endPos;
            while (pos < text.Count)
            {
                Paragraph paragraph = FillOneParagraph(text, pos, out endPos);
                result.Add(paragraph);
                pos = endPos + 1;
            }

            return result;
        }

        /// <summary>
        /// Isolates one paragraph from text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="startPos">first character to process</param>
        /// <param name="endPos">last processed character</param>
        /// <returns></returns>
        private static Paragraph FillOneParagraph(List<FChar> text, int startPos, out int endPos)
        {
            Paragraph paragraph = new Paragraph();

            while (text[startPos] == FChar.nextline || text[startPos] == FChar.align_center)
            {
                if (text[startPos] == FChar.align_center)
                    paragraph.centered = true;

                startPos++;
            }
            endPos = startPos - 1;
            for (int i = startPos; i < text.Count; i++)
            {
                if (text[i] == FChar.nextline || text[i] == FChar.align_center)
                {
                    break;
                }
                paragraph.text.Add(text[i]);
                endPos = i;
            }

            return paragraph;
        }

        /// <param name="startFrom">First char to process</param>
        /// <param name="end">Last processed character</param>
        /// <param name="lineWidth">in pixels</param>
        private static List<RenderUnit> MakeLineRenderSequence(Paragraph text, Font font, Random random, int startFrom, out int end, int lineWidth)
        {
            int fullWidth = lineWidth;
            if (text.centered)
                lineWidth = lineWidth * 2 / 3;

            List<RenderUnit> generatedLine = new List<RenderUnit>();
            end = startFrom - 1;
            int x = 0;
            for (int i = startFrom; i < text.text.Count; i++)
            {
                FChar character = text.text[i];

                switch (character)
                {
                    case FChar.space:
                        if (i == startFrom)
                            break;
                        RenderUnit space = new RenderUnit();
                        space.corrCharacter = FChar.space;
                        space.image = null;
                        space.x = x;
                        x += (int)(spaceSize * Font.pixelsPerCmH);
                        space.rightBorderX = x;
                        generatedLine.Add(space);
                        break;
                    case FChar.tab:
                        x += (int)(tabSize * Font.pixelsPerCmH);
                        break;
                    case FChar.linebreak:
                        RenderUnit linebreak = new RenderUnit();
                        linebreak.corrCharacter = FChar.linebreak;
                        linebreak.x = x;
                        linebreak.image = null;
                        generatedLine.Add(linebreak);
                        break;
                    default:
                        InsertRenderUnit(generatedLine, character, random, x, out x, font);
                        break;
                }
                end = i;

                if (generatedLine.Count > 0 && generatedLine.Last().rightBorderX >= lineWidth)
                {
                    while (generatedLine.Last().rightBorderX >= lineWidth)
                    {
                        bool needsWrapping;
                        int breakPos = FindBreakPos(generatedLine, out needsWrapping); //index of the last character on a line

                        if (breakPos == -1) //proper wrapping is not possible
                        {
                            for (int j = generatedLine.Count - 1; j >= 0; j--)
                            {
                                if (generatedLine[j].rightBorderX < lineWidth)
                                {
                                    breakPos = j;
                                    needsWrapping = false;
                                    break;
                                }
                            }
                        }

                        int count = generatedLine.Count - breakPos - 1;
                        generatedLine.RemoveRange(breakPos + 1, count);
                        end -= count;

                        int uselessX;
                        if (needsWrapping)
                            InsertRenderUnit(generatedLine, FChar.minus, random, generatedLine.Last().rightBorderX, out uselessX, font);
                    }
                    break;
                }
            }

            if (text.centered)
            {
                int rightDist = fullWidth - generatedLine.Last().rightBorderX;
                foreach (RenderUnit unit in generatedLine)
                {
                    unit.x += rightDist / 2;
                }
            }

            return generatedLine;
        }

        private static void InsertRenderUnit(List<RenderUnit> list, FChar character, Random random, int x, out int newX, Font font)
        {
            RenderUnit unit = new RenderUnit();

            int selectedImageIndex = random.Next(font.images[character].Count());

            unit.x = (int)(x - font.leftMargins[character][selectedImageIndex] * Font.pixelsPerCmV);
            unit.image = font.images[character][selectedImageIndex];
            unit.corrCharacter = character;
            unit.rightBorderX = unit.x + (int)(font.rightMargins[character][selectedImageIndex] * Font.pixelsPerCmH);
            list.Add(unit);

            newX = x + (int)((font.rightMargins[character][selectedImageIndex] - font.leftMargins[character][selectedImageIndex] + letterDistance) * Font.pixelsPerCmV);
        }

        /// <summary>
        /// Finds a place where a line could be broken, starting from the end.
        /// Returns an index of a last character that should stay in the fist part of a line.
        /// </summary>
        private static int FindBreakPos(List<RenderUnit> line, out bool needsWrapping)
        {
            int breakPos = -1; //index of the last character on a line
            needsWrapping = false;
            for (int j = line.Count - 1; j >= 0; j--)
            {
                if (line[j].corrCharacter == FChar.space)
                {
                    breakPos = j - 1;
                    break;
                }
                else if (line[j].corrCharacter == FChar.linebreak)
                {
                    breakPos = j - 1;
                    needsWrapping = true;
                    break;
                }
            }
            return breakPos;
        }

        public Bitmap GetPage(int number)
        {
            Render();
            if (number >= renderedText.Count)
                return null;
            if (number < 0)
                return null;
            return renderedText[number];
        }

        public int GetPageCount()
        {
            Render();
            return renderedText.Count;
        }
    }
}
