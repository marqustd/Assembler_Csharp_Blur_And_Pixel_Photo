using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using CsImplementation;
using Microsoft.Win32;
using ProjektJA.Model.Data;

namespace ProjektJA
{
    public partial class MainWindow : Window
    {
        private readonly IGaussBlurCs blurCsEngine;
        private readonly IPixelation pixelationCsEngine;

        public MainWindow()
        {
            DataContext = new DataContext();
            var x = sum(5, 10);
            blurCsEngine = new GaussBlurCs();
            pixelationCsEngine = new Pixelation();
            InitializeComponent();
        }

        [DllImport("CppImplementation.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern double sum(double x, double y);

        private void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            var context = DataContext as DataContext;

            openFileDialog.Filter = "Obrazy (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                context.Source = new Bitmap(openFileDialog.FileName);
                context.ImageSource = context.Source.ToBitmapImage();
            }
        }

        private async void DoOnClick(object sender, RoutedEventArgs e)
        {
            var context = DataContext as DataContext;
            var source = new Bitmap(context.Source);

            if (radioBlur.IsChecked.Value)
                context.After = await blurCsEngine.Blur(source, slRadius.Value).ConfigureAwait(false);

            if (radioPixel.IsChecked.Value)
                context.After = await pixelationCsEngine.PixelateAsync(source, (int) slRadius.Value);

            context.ImageSource = context.After.ToBitmapImage();
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
            var context = DataContext as DataContext;


            if (saveFileDialog.ShowDialog() == true) context.After.Save(saveFileDialog.FileName);
        }
    }
}