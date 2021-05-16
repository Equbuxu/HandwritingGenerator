using Handwriting_Generator.Helpers;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Handwriting_Generator.UI
{
    class BindSample : INotifyPropertyChanged
    {
        public Bitmap Image { get; }
        private BitmapSource imageSource = null;
        public BitmapSource ImageSource
        {
            get
            {
                if (imageSource == null)
                    imageSource = Image.CreateBitmapSource();
                return imageSource;
            }
        }
        private double leftBorderX;
        public double LeftBorderX
        {
            get => leftBorderX;
            set
            {
                leftBorderX = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftBorderX)));
            }
        }

        private double rightBorderX;
        public double RightBorderX
        {
            get => rightBorderX;
            set
            {
                rightBorderX = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RightBorderX)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BindSample(Bitmap image)
        {
            Image = image;
        }
    }
}
