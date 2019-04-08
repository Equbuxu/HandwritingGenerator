using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for TextRenderingWindow.xaml
    /// </summary>
    public partial class TextRenderingWindow : Window
    {
        Font selectedFont = null;
        TextRenderer renderer = null;
        int curPreviewPage = 0;

        public TextRenderingWindow()
        {
            InitializeComponent();
            UpdatePreview();
            PresetsComboBox.SelectedIndex = 0;
        }

        private void SelectPreset(object sender, RoutedEventArgs e)
        {
            Sheet sheet;
            switch (PresetsComboBox.SelectedIndex)
            {
                default://lined 
                    sheet = Sheet.LeftLinedSheet();
                    break;
                case 1: //checkered
                    sheet = Sheet.LeftCheckeredSheet();
                    break;
                case 2: //a4
                    sheet = Sheet.A4Sheet();
                    break;
            }

            WidthTextBox.Text = sheet.Width.ToString("G4", CultureInfo.InvariantCulture);
            HeightTextBox.Text = sheet.Height.ToString("G4", CultureInfo.InvariantCulture);
            LeftMarginTextBox.Text = sheet.LeftMargin.ToString("G4", CultureInfo.InvariantCulture);
            RightMarginTextBox.Text = sheet.RightMargin.ToString("G4", CultureInfo.InvariantCulture);
            LineHeightTextBox.Text = sheet.FirstLineHeight.ToString("G4", CultureInfo.InvariantCulture);
            LineDistanceTextBox.Text = sheet.DistBetweenLines.ToString("G4", CultureInfo.InvariantCulture);
            LineCountTextBox.Text = sheet.LineCount.ToString("G4", CultureInfo.InvariantCulture);
        }

        private void FilterNumbers(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!Char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private List<Sheet> CreateSheetSequence()
        {
            Sheet defined = new Sheet();
            int pageCount;
            try
            {
                defined.Width = double.Parse(WidthTextBox.Text, CultureInfo.InvariantCulture);
                defined.Height = double.Parse(HeightTextBox.Text, CultureInfo.InvariantCulture);
                defined.LeftMargin = double.Parse(LeftMarginTextBox.Text, CultureInfo.InvariantCulture);
                defined.RightMargin = double.Parse(RightMarginTextBox.Text, CultureInfo.InvariantCulture);
                defined.FirstLineHeight = double.Parse(LineHeightTextBox.Text, CultureInfo.InvariantCulture);
                defined.DistBetweenLines = double.Parse(LineDistanceTextBox.Text, CultureInfo.InvariantCulture);
                defined.LineCount = int.Parse(LineCountTextBox.Text, CultureInfo.InvariantCulture);
                pageCount = int.Parse(PageCountTextBox.Text, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return null;
            }

            Sheet inverted = new Sheet();
            inverted.Width = defined.Width;
            inverted.Height = defined.Height;
            inverted.LeftMargin = defined.RightMargin;
            inverted.RightMargin = defined.LeftMargin;
            inverted.FirstLineHeight = defined.FirstLineHeight;
            inverted.DistBetweenLines = defined.DistBetweenLines;
            inverted.LineCount = defined.LineCount;

            List<Sheet> sheets = new List<Sheet>();
            int startInverted = InvertCheckBox.IsChecked == true ? 1 : 0;

            for (int i = 0; i < pageCount; i++)
            {
                if (i % 2 == startInverted)
                    sheets.Add(defined.Copy());
                else
                    sheets.Add(inverted.Copy());
            }

            return sheets;
        }

        private void UpdatePreview()
        {
            if (renderer == null)
            {
                PreviousPageButton.IsEnabled = false;
                NextPageButton.IsEnabled = false;
                return;
            }

            PreviousPageButton.IsEnabled = curPreviewPage != 0;
            NextPageButton.IsEnabled = curPreviewPage != renderer.GetPageCount() - 1;

            PreviewImage.Source = BitmapUtils.ImageSourceForBitmap(renderer.GetPage(curPreviewPage));
        }

        private void Render(object sender, RoutedEventArgs e)
        {
            if (selectedFont == null)
            {
                MessageBox.Show("Font is not selected");
                return;
            }
            List<Sheet> sheets = CreateSheetSequence();
            if (sheets == null)
            {
                MessageBox.Show("Invalid paper settings");
                return;
            }

            string text = TextToRender.Text;
            TextConverter textConverter = new TextConverter(text);
            renderer = new TextRenderer(textConverter.Convert(), sheets, selectedFont);

            UpdatePreview();
        }

        private void SelectFont(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Handwritten font samples|*.hfs";
            if (dialog.ShowDialog() != true)
                return;

            string path = dialog.FileName;

            try
            {
                selectedFont = new Font(path);
            }
            catch (FontLoadingException exc)
            {
                selectedFont = null;
                MessageBox.Show("Exception while loading font file: " + exc.InnerException != null ? exc.InnerException.Message : "");
                return;
            }

            FontFileName.Content = dialog.SafeFileName;
        }

        private void ChangePreview(object sender, RoutedEventArgs e)
        {
            if (sender == PreviousPageButton)
                curPreviewPage--;
            else if (sender == NextPageButton)
                curPreviewPage++;

            Debug.Assert(renderer == null ? curPreviewPage == 0 : curPreviewPage >= 0 && curPreviewPage < renderer.GetPageCount());

            UpdatePreview();
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            if (renderer == null)
            {
                MessageBox.Show("Nothing to export: there are no rendered pages");
                return;
            }

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            int i = 0;
            while (true)
            {
                System.Drawing.Bitmap page = renderer.GetPage(i);
                if (page == null)
                    break;
                page.SetResolution((float)(Font.pixelsPerCmH * 2.54), (float)(Font.pixelsPerCmV * 2.54));
                page.Save(dialog.SelectedPath + "/" + i + ".png");
                i++;
            }
        }
    }
}
