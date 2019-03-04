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

            //Font font = new Font();
            //font.LoadFromForm("Resources/testing/3.jpg");
            FormPreparer generator = new FormPreparer("Resources/testing/0.jpg");
            Bitmap prepared = generator.CreatePrepared();
            FormCutter cutter = new FormCutter(prepared);
            List<List<Bitmap>> letters = cutter.Cut();

            for (int i = 0; i < letters.Count; i++)
            {
                Directory.CreateDirectory("DebugOut/" + i.ToString());
                int name = 0;
                foreach (Bitmap image in letters[i])
                {
                    name++;
                    image.Save("DebugOut/" + i.ToString() + "/" + name.ToString() + ".png");
                }
            }
        }
    }
}
