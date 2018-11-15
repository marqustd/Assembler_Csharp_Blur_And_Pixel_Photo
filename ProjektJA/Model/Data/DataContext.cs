using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using ProjektJA.Annotations;

namespace ProjektJA.Model.Data
{
    internal class DataContext : INotifyPropertyChanged
    {
        private Bitmap after;
        private BitmapImage imageSource;
        private Bitmap source;

        public BitmapImage ImageSource
        {
            get => imageSource;
            set
            {
                imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        public Bitmap Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public Bitmap After
        {
            get => after;
            set
            {
                after = value;
                OnPropertyChanged(nameof(After));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}