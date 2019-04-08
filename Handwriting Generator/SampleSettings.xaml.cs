using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
using System.Windows.Shapes;

namespace Handwriting_Generator
{
    /// <summary>
    /// Interaction logic for SampleSettings.xaml
    /// </summary>
    public partial class SampleSettings : Window
    {
        public SampleSettings(Font loadedFont, FChar character, int id)
        {
            InitializeComponent();

            this.character = character;
            this.id = id;
            this.loadedFont = loadedFont;

            Bitmap image = loadedFont.images[character][id];
            height = 200;
            width = (int)(height * (image.Width / (double)image.Height));

            ImageCanvas.Width = width;
            ImageCanvas.Height = height;

            Preview.Width = width;
            Preview.Height = height;

            Preview.Source = BitmapUtils.ImageSourceForBitmap(image);

            leftBorderPos = loadedFont.leftMargins[character][id];
            rightBorderPos = loadedFont.rightMargins[character][id];

            LeftBorder.Text = leftBorderPos.ToString("G4", CultureInfo.InvariantCulture);
            RightBorder.Text = rightBorderPos.ToString("G4", CultureInfo.InvariantCulture);

            UpdateRectangle();
        }

        double leftBorderPos;
        double rightBorderPos;
        int width;
        int height;
        Font loadedFont;
        FChar character;
        int id;

        private void ParseInput(object sender, RoutedEventArgs e)
        {
            double newLeft = 0;
            if (double.TryParse(LeftBorder.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out newLeft))
            {
                if (newLeft >= 0 && newLeft < Font.imageCmW && newLeft < rightBorderPos)
                    leftBorderPos = newLeft;
            }

            double newRight = 0;
            if (double.TryParse(RightBorder.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out newRight))
            {
                if (newRight >= 0 && newRight < Font.imageCmW && newRight > leftBorderPos)
                    rightBorderPos = newRight;
            }

            UpdateRectangle();
        }

        private void UpdateRectangle()
        {
            Borders.Width = (-leftBorderPos + rightBorderPos) / Font.imageCmW * width;
            Borders.Height = height;
            Canvas.SetLeft(Borders, leftBorderPos / Font.imageCmW * width);
        }

        private void Submid(object sender, RoutedEventArgs e)
        {
            loadedFont.leftMargins[character][id] = leftBorderPos;
            loadedFont.rightMargins[character][id] = rightBorderPos;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", "", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
                return;

            loadedFont.leftMargins[character].RemoveAt(id);
            loadedFont.rightMargins[character].RemoveAt(id);
            loadedFont.images[character].RemoveAt(id);

            if (loadedFont.leftMargins[character].Count == 0)
            {
                loadedFont.leftMargins.Remove(character);
                loadedFont.rightMargins.Remove(character);
                loadedFont.images.Remove(character);
            }

            Close();
        }
    }
}
