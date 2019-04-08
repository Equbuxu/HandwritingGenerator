using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Handwriting_Generator
{
    /// <summary>
    /// Interaction logic for FontEditor.xaml
    /// </summary>
    public partial class FontEditor : Window
    {
        Font loadedFont = null;

        public FontEditor()
        {
            InitializeComponent();
        }

        private void UpdateCharacterList()
        {
            var selected = CharacterList.SelectedItem;

            CharacterList.Items.Clear();

            if (loadedFont == null)
                return;
            foreach (FChar item in loadedFont.images.Keys)
            {
                CharacterList.Items.Add(item.ToString());
            }

            if (selected != null)
                CharacterList.SelectedItem = selected;
        }

        private void LoadHfsFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Handwritted font samples|*.hfs";
            if (dialog.ShowDialog() != true)
                return;

            if (loadedFont == null)
                loadedFont = new Font(dialog.FileName);
            else
                loadedFont.AddFromOther(new Font(dialog.FileName));

            UpdateCharacterList();
            UpdateSampleList(null, null);
        }

        private void UpdateSampleList(object sender, SelectionChangedEventArgs e)
        {
            SampleList.Children.Clear();
            if (CharacterList.SelectedItem == null)
                return;

            FChar selectedItem = (FChar)Enum.Parse(typeof(FChar), CharacterList.SelectedItem.ToString());

            for (int i = 0; i < loadedFont.images[selectedItem].Count; i++)
            {
                Bitmap image = loadedFont.images[selectedItem][i];

                int width = 100;
                int height = (int)(width * (image.Height / (double)image.Width));

                var canvas = new Canvas();
                canvas.Width = width;
                canvas.Height = height;
                int id = i;
                canvas.MouseLeftButtonDown += (a, b) => OpenSampleSettings(selectedItem, id);

                var control = new System.Windows.Controls.Image();
                control.Source = BitmapUtils.ImageSourceForBitmap(image);
                control.Width = width;
                control.Height = height;

                var rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Stroke = System.Windows.Media.Brushes.Red;
                rectangle.Fill = System.Windows.Media.Brushes.Transparent;
                rectangle.StrokeThickness = 2.0;
                rectangle.Width = (loadedFont.rightMargins[selectedItem][i] - loadedFont.leftMargins[selectedItem][i]) / Font.imageCmW * width;
                rectangle.Height = height;

                canvas.Children.Add(control);
                canvas.Children.Add(rectangle);
                Canvas.SetLeft(rectangle, loadedFont.leftMargins[selectedItem][i] / Font.imageCmW * width);

                var border = new Border();
                border.BorderBrush = System.Windows.Media.Brushes.Gray;
                border.BorderThickness = new Thickness(1.0);
                border.Child = canvas;
                border.Margin = new Thickness(5.0);
                SampleList.Children.Add(border);
            }
        }

        private void LoadFormImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files |*.jpg;*.jpeg;*.png";
            if (dialog.ShowDialog() != true)
                return;

            Font font;
            try
            {
                FontCreator fontCreator = new FontCreator();
                fontCreator.Add(dialog.FileName);
                font = fontCreator.GetFont();
            }
            catch (FormException exc)
            {
                string message = "Exception while processing the form. ";
                if (exc.InnerException != null)
                {
                    message += exc.InnerException.Message;
                }
                MessageBox.Show(message);
                return;
            }

            if (loadedFont == null)
                loadedFont = font;
            else
                loadedFont.AddFromOther(font);

            UpdateCharacterList();
            UpdateSampleList(null, null);
        }

        private void Autoalign(object sender, RoutedEventArgs e)
        {
            FontAligner aligner;
            try
            {
                aligner = new FontAligner(loadedFont);
            }
            catch (FormatException)
            {
                MessageBox.Show("Font must contain small russian letters in order to perform automatic border calculation.");
                return;
            }
            aligner.Align();

            UpdateSampleList(null, null);
        }

        private void SaveHfs(object sender, RoutedEventArgs e)
        {
            if (loadedFont == null)
            {
                MessageBox.Show("Nothing to export!");
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Handwritted font samples|*.hfs";
            if (dialog.ShowDialog() != true)
                return;
            loadedFont.Save(dialog.FileName);
        }

        private void ClearList(object sender, RoutedEventArgs e)
        {
            loadedFont = null;
            UpdateCharacterList();
            UpdateSampleList(null, null);
        }

        private void OpenSampleSettings(FChar character, int id)
        {
            SampleSettings sampleSettings = new SampleSettings(loadedFont, character, id);
            sampleSettings.ShowDialog();

            UpdateCharacterList();
            UpdateSampleList(null, null);
        }
    }
}
