using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\0.jpg");
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\1.jpg");
            Font font = fontCreator.GetFont();
            font.Save("DebugOut/savedFont.zip");
        }

        public MainWindow()
        {
            InitializeComponent();

            //CreateFont();

            Font font = new Font("DebugOut/savedFont.zip");

            TextConverter textConverter = new TextConverter("На днях компания Microsoft открыла исходный код калькулятора. Это приложение входило во все версии операционной системы Windows. Исходный код разных проектов Microsoft достаточно часто открывался за последние годы, но новость о калькуляторе в первый же день просочилась даже в нетехнологические средства массовой информации. Что ж, это популярная, но очень маленькая программа на языке C++, тем не менее, статический анализ кода с помощью PVS-Studio выявил подозрительные места в коде.");


            TextRenderer renderer = new TextRenderer(textConverter.Convert(), new List<Sheet>() { Sheet.LeftLinedSheet() }, font);

            int i = 0;
            while (true)
            {
                Bitmap page = renderer.GetPage(i);
                if (page == null)
                    break;
                page.Save("DebugOut/page" + i + ".png");
                i++;
            }
        }
    }
}
