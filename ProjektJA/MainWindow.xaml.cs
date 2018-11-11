using System.Drawing;
using System.Windows;
using CsImplementation;
using Microsoft.Win32;

namespace ProjektJA
{
    public partial class MainWindow : Window
    {
        private readonly IGaussBlurCs blurCsEngine;
        private readonly IPixelation pixelationCsEngine;
        private Bitmap after;
        private Bitmap source;

        public MainWindow()
        {
            blurCsEngine = new GaussBlurCs();
            pixelationCsEngine = new Pixelation();
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Bitmapy (*.bmp;)|*.bmp;";

            if (openFileDialog.ShowDialog() == true)
            {
                source = new Bitmap(openFileDialog.FileName);

                image.Source = source.ToBitmapImage();
            }
        }

        private async void DoOnClick(object sender, RoutedEventArgs e)
        {
            if (radioBlur.IsChecked.Value)
            {
                after = await blurCsEngine.Blur(source, slRadius.Value).ConfigureAwait(false);
            }

            if (radioPixel.IsChecked.Value)
            {
                after = await pixelationCsEngine.Pixelate(source, (int) slRadius.Value).ConfigureAwait(false);
            }

            image.Source = after.ToBitmapImage();
        }

        private void RadioBlur_OnChecked(object sender, RoutedEventArgs e)
        {
            slRadius.Minimum = 0.1;
            slRadius.Maximum = 100;
            slRadius.IsSnapToTickEnabled = false;
        }

        private void RadioPixel_OnChecked(object sender, RoutedEventArgs e)
        {
            slRadius.Minimum = 3;
            slRadius.Maximum = 101;
            slRadius.TickFrequency = 2;
            slRadius.IsSnapToTickEnabled = true;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();

            if (saveFileDialog.ShowDialog() == true)
            {
                after.Save(saveFileDialog.FileName);
            }
        }
    }
}