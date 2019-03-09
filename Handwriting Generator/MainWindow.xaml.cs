﻿using System;
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
            font.Save("DebugOut/savedFont.zip");

            List<fChar> text = new List<fChar>()
            {
                fChar.rus_17_cap,
                fChar.rus_18,
                fChar.rus_10,
                fChar.rus_3,
                fChar.rus_6,
                fChar.rus_20,
            };

            TextRenderer renderer = new TextRenderer(text, new List<Sheet>() { Sheet.LeftLinedSheet() }, font);
            renderer.GetPage(0).Save("DebugOut/page.png");
        }
    }
}
