using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace Handwriting_Generator.UI
{
    class FontEditorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BindCharacter> LoadedFont { get; private set; }
        private BindSample selectedSample;
        public BindSample SelectedSample
        {
            get => selectedSample;
            set
            {
                selectedSample = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSample)));
            }
        }

        public CommandHandler SelectSampleCommand { get; }

        public FontEditorViewModel()
        {
            LoadedFont = new ObservableCollection<BindCharacter>();
            SelectSampleCommand = new CommandHandler((sender) =>
            {
                SelectedSample = (BindSample)sender;
            });

            var char1 = new BindCharacter(FChar.rus_1);
            Bitmap bitmap = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillEllipse(Brushes.Red, new RectangleF(0, 0, 100, 100));
            }
            Bitmap bitmap1 = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bitmap1))
            {
                g.FillEllipse(Brushes.Green, new RectangleF(0, 0, 100, 100));
            }
            var sample1 = new BindSample(bitmap);
            sample1.LeftBorderX = 7;
            sample1.RightBorderX = 25;
            char1.Samples.Add(sample1);
            var sample2 = new BindSample(bitmap1);
            sample2.LeftBorderX = 1;
            sample2.RightBorderX = 10;
            char1.Samples.Add(sample2);


            LoadedFont.Add(char1);

            var char2 = new BindCharacter(FChar.rus_2);
            LoadedFont.Add(char2);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
