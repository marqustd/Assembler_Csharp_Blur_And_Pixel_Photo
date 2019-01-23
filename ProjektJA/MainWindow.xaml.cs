using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using ImageFilters.GaussBlur;
using ImageFilters.Pixelate;
using Microsoft.Win32;
using ProjektJA.Model.Data;

namespace ProjektJA
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new DataContext();
            InitializeComponent();

            btnFilter.IsEnabled = false;

            slMask.Minimum = 3;
            slMask.Maximum = 101;
            slMask.TickFrequency = 2;
            slMask.IsSnapToTickEnabled = true;

            slRadius.Minimum = 0.1;
            slRadius.Maximum = 100;
            slRadius.IsSnapToTickEnabled = false;

            ProgressLabel.Visibility = Visibility.Hidden;
        }

        private void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            var context = DataContext as DataContext;
            btnFilter.IsEnabled = false;

            openFileDialog.Filter = "Obrazy (*.bmp;*.jpg;)|*.bmp;*.jpg;";

            if (openFileDialog.ShowDialog() == true)
            {
                context.Source = new Bitmap(openFileDialog.FileName);
                context.ImageSource = context.Source.ToBitmapImage();
                btnFilter.IsEnabled = true;
            }
        }

        private async void DoOnClick(object sender, RoutedEventArgs e)
        {
            var context = DataContext as DataContext;
            var source = new Bitmap(context.Source);
            var maskSize = (int) slMask.Value;
            var radius = slRadius.Value;

            var stopWatch = Stopwatch.StartNew();
            ProgressLabel.Visibility = Visibility.Visible;
            if (radioAssembler.IsChecked.Value) //assembler
            {
                if (radioBlur.IsChecked.Value) //asembler blur
                {
                    await Task.Run(() =>
                    {
                        var engine = new GaussBlurrAsm(maskSize, radius);
                        context.After = engine.FilterUnsafe(source);
                    });
                }
                else //assembler pixel
                {
                    await Task.Run(() =>
                    {
                        var engine = new PixelationAsm(maskSize);
                        context.After = engine.FilterUnsafe(source);
                    });
                }
            }
            else //Csharp
            {
                if (radioBlur.IsChecked.Value) //Csharp blur
                {
                    await Task.Run(() =>
                    {
                        var engine = new GaussBlurCsharp(maskSize, radius);
                        context.After = engine.FilterUnsafe(source);
                    });
                }
                else //Csharp pixel
                {
                    await Task.Run(() =>
                    {
                        var engine = new PixelationCsharp(maskSize);
                        context.After = engine.FilterUnsafe(source);
                    });
                }
            }

            stopWatch.Stop();
            ProgressLabel.Visibility = Visibility.Hidden;
            timeLabel.Content = $"Czas[ms]: {stopWatch.ElapsedMilliseconds}";
            context.ImageSource = context.After.ToBitmapImage();
        }

        private void RadioBlur_OnChecked(object sender, RoutedEventArgs e)
        {
            slRadius.IsEnabled = true;
            slRadius.Visibility = Visibility.Visible;
            boxRadius.Visibility = Visibility.Visible;
            labelRadius.Visibility = Visibility.Visible;
        }

        private void RadioPixel_OnChecked(object sender, RoutedEventArgs e)
        {
            slRadius.IsEnabled = false;
            slRadius.Visibility = Visibility.Hidden;
            boxRadius.Visibility = Visibility.Hidden;
            labelRadius.Visibility = Visibility.Hidden;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            var context = DataContext as DataContext;
            saveFileDialog.Filter = "Obrazy (*.bmp;)|*.bmp;";

            if (saveFileDialog.ShowDialog() == true)
            {
                context.After.Save(saveFileDialog.FileName);
            }
        }
    }
}