using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using GaussBlur;
using Microsoft.Win32;

namespace ProjektJA
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IGaussBlurCs blurCsEngine;
        private Bitmap after;
        private BitmapImage sourceBitMapImage;

        public MainWindow()
        {
            blurCsEngine = new GaussBlurCs();
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Obrazy (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                sourceBitMapImage = new BitmapImage();
                sourceBitMapImage.BeginInit();
                sourceBitMapImage.UriSource = new Uri(openFileDialog.FileName);
                sourceBitMapImage.EndInit();

                image.Source = sourceBitMapImage;
            }
        }

        private void DoSomething(object sender, RoutedEventArgs e)
        {
            after = blurCsEngine.Blur(sourceBitMapImage.ToBitmap(), slRadius.Value);

            image.Source = after.ToBitmapImage();
        }
    }
}