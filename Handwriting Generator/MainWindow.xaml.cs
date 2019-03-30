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
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\0.jpg");
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\1.jpg");
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\2.jpg");
            Font font = fontCreator.GetFont();
            font.Save("DebugOut/savedFont.zip");
            Console.WriteLine("done");
        }

        private void GenerateText()
        {
            Font font = new Font("DebugOut/savedFont.zip");

            TextConverter textConverter = new TextConverter("Встретил мужика, начали сожительствовать. Не как друзья или собутыльники, а как мужчина с женщиной. Амбал покупал ему кучу всякого, курточку там какую-то купил за 30 000 руб. (а это было более 10 лет назад!) а этот противный... эта неблагодарная скотина - взял и изменил! Причем, подумать только - с кем! С женщиной!");
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
