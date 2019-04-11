using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting_Generator
{
    public class TextConverter
    {
        private bool converted = false;

        private string origText;
        private List<FChar> convertedText = new List<FChar>();

        private static Dictionary<char, FChar> translationTable = new Dictionary<char, FChar>()
        {
            { '\x0410', FChar.rus_1_cap},
            { '\x0411', FChar.rus_2_cap},
            { '\x0412', FChar.rus_3_cap},
            { '\x0413', FChar.rus_4_cap},
            { '\x0414', FChar.rus_5_cap},
            { '\x0415', FChar.rus_6_cap},
            { '\x0401', FChar.rus_7_cap},
            { '\x0416', FChar.rus_8_cap},
            { '\x0417', FChar.rus_9_cap},
            { '\x0418', FChar.rus_10_cap},
            { '\x0419', FChar.rus_11_cap},
            { '\x041A', FChar.rus_12_cap},
            { '\x041B', FChar.rus_13_cap},
            { '\x041C', FChar.rus_14_cap},
            { '\x041D', FChar.rus_15_cap},
            { '\x041E', FChar.rus_16_cap},
            { '\x041F', FChar.rus_17_cap},
            { '\x0420', FChar.rus_18_cap},
            { '\x0421', FChar.rus_19_cap},
            { '\x0422', FChar.rus_20_cap},
            { '\x0423', FChar.rus_21_cap},
            { '\x0424', FChar.rus_22_cap},
            { '\x0425', FChar.rus_23_cap},
            { '\x0426', FChar.rus_24_cap},
            { '\x0427', FChar.rus_25_cap},
            { '\x0428', FChar.rus_26_cap},
            { '\x0429', FChar.rus_27_cap},
            { '\x042A', FChar.rus_28_cap},
            { '\x042B', FChar.rus_29_cap},
            { '\x042C', FChar.rus_30_cap},
            { '\x042D', FChar.rus_31_cap},
            { '\x042E', FChar.rus_32_cap},
            { '\x042F', FChar.rus_33_cap},

            { '\x0430', FChar.rus_1},
            { '\x0431', FChar.rus_2},
            { '\x0432', FChar.rus_3},
            { '\x0433', FChar.rus_4},
            { '\x0434', FChar.rus_5},
            { '\x0435', FChar.rus_6},
            { '\x0451', FChar.rus_7},
            { '\x0436', FChar.rus_8},
            { '\x0437', FChar.rus_9},
            { '\x0438', FChar.rus_10},
            { '\x0439', FChar.rus_11},
            { '\x043A', FChar.rus_12},
            { '\x043B', FChar.rus_13},
            { '\x043C', FChar.rus_14},
            { '\x043D', FChar.rus_15},
            { '\x043E', FChar.rus_16},
            { '\x043F', FChar.rus_17},
            { '\x0440', FChar.rus_18},
            { '\x0441', FChar.rus_19},
            { '\x0442', FChar.rus_20},
            { '\x0443', FChar.rus_21},
            { '\x0444', FChar.rus_22},
            { '\x0445', FChar.rus_23},
            { '\x0446', FChar.rus_24},
            { '\x0447', FChar.rus_25},
            { '\x0448', FChar.rus_26},
            { '\x0449', FChar.rus_27},
            { '\x044A', FChar.rus_28},
            { '\x044B', FChar.rus_29},
            { '\x044C', FChar.rus_30},
            { '\x044D', FChar.rus_31},
            { '\x044E', FChar.rus_32},
            { '\x044F', FChar.rus_33},

            { '.', FChar.period},
            { '!', FChar.exclamation_mark},
            { '?', FChar.question_mark},
            { '№', FChar.number},
            //{ '', FChar.lower_quote},
            { '"', FChar.upper_quote},
            { '-', FChar.minus},
            { '—', FChar.minus},
            { '+', FChar.plus},
            { '=', FChar.equals},
            { '/', FChar.slash},
            { '(', FChar.open_parenthesis},
            { ')', FChar.close_parenthesis},
            { ';', FChar.semicolon},
            { ':', FChar.colon},
            { '@', FChar.email},
            { '#', FChar.hash},
            { '$', FChar.dollar},
            { '&', FChar.ampersand},
            { '\\', FChar.backslash},
            { '[', FChar.open_square_bracket},
            { ']', FChar.close_square_bracket},
            { '{', FChar.open_curly_bracket},
            { '}', FChar.close_curly_bracket},
            { '<', FChar.less_than},
            { '>', FChar.greater_than},
            { '%', FChar.percent},
            { '~', FChar.tilde},
            { '_', FChar.underscore},
            { ',', FChar.comma},

            { '0', FChar.digit_0},
            { '1', FChar.digit_1},
            { '2', FChar.digit_2},
            { '3', FChar.digit_3},
            { '4', FChar.digit_4},
            { '5', FChar.digit_5},
            { '6', FChar.digit_6},
            { '7', FChar.digit_7},
            { '8', FChar.digit_8},
            { '9', FChar.digit_9},

            { ' ', FChar.space},
            { '\n', FChar.nextline},
            { '\t', FChar.tab},
        };

        private static List<FChar> vowels = new List<FChar>()
        {
            // а, о, э, и, у, ы, е, ё, ю, я.
            FChar.rus_1,
            FChar.rus_16,
            FChar.rus_31,
            FChar.rus_10,
            FChar.rus_21,
            FChar.rus_29,
            FChar.rus_6,
            FChar.rus_7,
            FChar.rus_32,
            FChar.rus_33,

            FChar.rus_1_cap,
            FChar.rus_16_cap,
            FChar.rus_31_cap,
            FChar.rus_10_cap,
            FChar.rus_21_cap,
            FChar.rus_29_cap,
            FChar.rus_6_cap,
            FChar.rus_7_cap,
            FChar.rus_32_cap,
            FChar.rus_33_cap,
        };

        private static List<FChar> soundCharacters = new List<FChar>()
        {
            // ъ,ь,й
            FChar.rus_30,
            FChar.rus_28,
            FChar.rus_11,

            FChar.rus_30_cap,
            FChar.rus_28_cap,
            FChar.rus_11_cap,
        };

        private static List<FChar> consonants = new List<FChar>()
        {
            // Б, В, Г, Д, Ж, З, Й, К, Л, М, Н, П, Р, С, Т, Ф, Х, Ц, Ч, Ш, Щ
            FChar.rus_2,
            FChar.rus_3,
            FChar.rus_4,
            FChar.rus_5,
            FChar.rus_8,
            FChar.rus_9,
            FChar.rus_11,
            FChar.rus_12,
            FChar.rus_13,
            FChar.rus_14,
            FChar.rus_15,
            FChar.rus_17,
            FChar.rus_18,
            FChar.rus_19,
            FChar.rus_20,
            FChar.rus_22,
            FChar.rus_23,
            FChar.rus_24,
            FChar.rus_25,
            FChar.rus_26,
            FChar.rus_27,

            FChar.rus_2_cap,
            FChar.rus_3_cap,
            FChar.rus_4_cap,
            FChar.rus_5_cap,
            FChar.rus_8_cap,
            FChar.rus_9_cap,
            FChar.rus_11_cap,
            FChar.rus_12_cap,
            FChar.rus_13_cap,
            FChar.rus_14_cap,
            FChar.rus_15_cap,
            FChar.rus_17_cap,
            FChar.rus_18_cap,
            FChar.rus_19_cap,
            FChar.rus_20_cap,
            FChar.rus_22_cap,
            FChar.rus_23_cap,
            FChar.rus_24_cap,
            FChar.rus_25_cap,
            FChar.rus_26_cap,
            FChar.rus_27_cap,
        };

        public TextConverter(string text)
        {
            origText = text;
        }

        public List<FChar> Convert()
        {
            if (converted)
                return convertedText;
            converted = true;

            TranslateToFChar();
            PairQuotes();
            SeparateSyllables();

            return convertedText;
        }

        private void SeparateSyllables()
        {
            bool pendingLineBreak = false;
            for (int i = 0; i < convertedText.Count; i++)
            {
                if (!vowels.Contains(convertedText[i]) && !consonants.Contains(convertedText[i]) && !soundCharacters.Contains(convertedText[i]))
                {
                    pendingLineBreak = false;
                    continue;
                }

                if (pendingLineBreak && !soundCharacters.Contains(convertedText[i]))
                {
                    pendingLineBreak = false;
                    convertedText.Insert(i, FChar.linebreak);
                    i++;
                }

                if (vowels.Contains(convertedText[i]))
                    pendingLineBreak = true;
            }
        }

        private void PairQuotes()
        {
            bool insideQuotes = false;

            for (int i = 0; i < convertedText.Count; i++)
            {
                if (convertedText[i] == FChar.upper_quote)
                {
                    insideQuotes = !insideQuotes;
                    if (!insideQuotes)
                        convertedText[i] = FChar.lower_quote;
                }
            }
        }

        private void TranslateToFChar()
        {
            for (int i = 0; i < origText.Length; i++)
            {
                //Test if it's a tag
                int skip;
                FChar tag = ConvertToTag(i, out skip);
                if (tag != FChar.empty)
                {
                    i += skip - 1;
                    convertedText.Add(tag);
                    continue;
                }

                //Translate as a regular character
                convertedText.Add(ConvertToFChar(origText[i]));
            }
        }

        private FChar ConvertToTag(int pos, out int skip)
        {
            string tag = "[center]";
            bool isTag = true;

            for (int i = 0; i < tag.Length; i++)
            {
                if (origText[pos + i] != tag[i])
                {
                    isTag = false;
                    break;
                }
            }
            skip = isTag ? tag.Length : 0;
            return isTag ? FChar.align_center : FChar.empty;
        }

        private FChar ConvertToFChar(char character)
        {
            if (translationTable.ContainsKey(character))
                return translationTable[character];
            return FChar.space;
        }
    }
}
