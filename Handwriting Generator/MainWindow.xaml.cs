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
        public MainWindow()
        {
            InitializeComponent();

            /*FontCreator fontCreator = new FontCreator();
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\0.jpg");
            fontCreator.Add(@"C:\Users\egor0\source\repos\Handwriting Generator\Handwriting Generator\Resources\testing\1.jpg");
            Font font = fontCreator.GetFont();*/
            Font font = new Font("DebugOut/savedFont.zip");
            //font.Save("DebugOut/savedFont.zip");

            List<FChar> text = new List<FChar>()
            {
                FChar.rus_17_cap,
                FChar.rus_18,
                FChar.rus_10,
                FChar.rus_3,
                FChar.rus_6,
                FChar.rus_20,
                FChar.align_center,
                FChar.rus_1,
                FChar.rus_2,
                FChar.nextline,
                FChar.rus_3,
                FChar.align_center,
                FChar.nextline,
                FChar.rus_5,
                FChar.rus_33_cap,
            };

            TextRenderer renderer = new TextRenderer(text, new List<Sheet>() { Sheet.LeftLinedSheet() }, font);
            renderer.GetPage(0).Save("DebugOut/page.png");
        }
    }
}
