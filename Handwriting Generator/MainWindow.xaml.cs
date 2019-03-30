using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Handwriting_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void CreateFont()
        {
            FontCreator fontCreator = new FontCreator();
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\0o.png");
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\1o.png");
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\2o.png");
            Font font = fontCreator.GetFont();
            font.Save("DebugOut/savedFontOleg.zip");
            Console.WriteLine("done");
        }

        private void GenerateText()
        {
            Font font = new Font("DebugOut/savedFontOleg.zip");

            TextConverter textConverter = new TextConverter(
                "[center]Почерк\n" +
                "Почерк — фиксируемая в рукописи, характерная для каждого пишущего и основанная на его письменно-двигательном навыке система движений, с помощью которой выполняются условные графические знаки.\n" +
                "На формирование почерка огромное влияние оказывают различные факторы как субъективного, так и объективного плана. Субъективные присущи конкретной личности пишущего, а объективные зависят от внешних условий, в которых проходит процесс письма.\n" +
                "Исследование рукописных текстов обычно происходит в рамках криминалистического исследования документов (почерковедение). Кроме того, почерк является предметом изучения графологии, однако результаты графологических исследований не всеми признаются в качестве научного факта.\n" +
                "В последние годы количество текстов, написанных от руки, сокращается (люди всё чаще начинают писать на клавиатуре и распечатывать на принтерах).\n" +
                "Почерк может быть «хорошим», то есть удобочитаемым, или же невнятным (даже для человека, написавшего письмо). Разборчивость почерка имеет большое значение для публикаторов рукописных материалов деятелей прошлого. В частности, хороший почерк в письмах и рукописях К.Д.Бальмонта и едва разборчивый у М.В.Добужинского."
                );

            TextRenderer renderer = new TextRenderer(textConverter.Convert(), new List<Sheet>() { Sheet.LeftLinedSheet(), Sheet.RightLinedSheet() }, font);

            int i = 0;
            while (true)
            {
                Bitmap page = renderer.GetPage(i);
                if (page == null)
                    break;
                page.Save("DebugOut/page" + i + ".png");
                i++;
            }
            Console.WriteLine("done");
        }

        public MainWindow()
        {
            InitializeComponent();

            //Thread thread = new Thread(CreateFont);
            Thread thread = new Thread(GenerateText);

            thread.Start();
        }
    }
}
