using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageGenerator;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImagePreparator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly IconsGenerator _iconsGenerator = new IconsGenerator();
        private string _outputDir;

        public MainWindow()
        {
            InitializeComponent();
            UpdateIcons();

            Scales.ItemsSource = _iconsGenerator.AvailableScales;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog(this) == true)
            {
                var bm = new BitmapImage();
                bm.BeginInit();
                bm.StreamSource = File.OpenRead(ofd.FileName);
                bm.EndInit();
                IconPreview.Source = bm;
            }

            UpdateIcons();
        }

        private void UpdateIcons()
        {
            if (ResultImages == null)
                return;


            ResultImages.ItemsSource = _iconsGenerator.UpdateIcons();
        }

        public void CreateIcons()
        {
            foreach (var icon in _iconsGenerator.Icons)
            {
                var elemnet = IconsGenerator.GetRendreable(icon, IconPreview.Source, _iconsGenerator.Color);
                IconsGenerator.CreateIcon(elemnet, icon.Width, icon.Height, Path.Combine(_outputDir, icon.FileName));
            }

            Process.Start(_outputDir);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                _outputDir = dialog.FileName;
                CreateIcons();
            }
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int Height = (int)IconPreview.ActualHeight;
                int Width = (int)IconPreview.ActualWidth;


                RenderTargetBitmap bmp = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(this.IconPreview);

                var wb = new WriteableBitmap(bmp);
                var pos = e.GetPosition(sender as FrameworkElement);
                _iconsGenerator.Color = wb.GetPixel((int) pos.X, (int) pos.Y);

                ColorPicker1.SelectedColor = _iconsGenerator.Color;
            }
            catch (Exception ex)
            {
            }
        }

        private void ColorPicker1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            _iconsGenerator.Color = ColorPicker1.SelectedColor ?? Colors.Transparent;
            UpdateIcons();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var availableScale in _iconsGenerator.AvailableScales)
            {
                availableScale.IsChecked = false;
            }

            foreach (var scaleVm in _iconsGenerator.AvailableScales.Where(x => (new[] { 100, 140, 240 }).Contains(x.Scale)).ToArray())
            {
                scaleVm.IsChecked = true;
            }
            UpdateIcons();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            foreach (var availableScale in _iconsGenerator.AvailableScales)
            {
                availableScale.IsChecked = false;
            }

            foreach (var scaleVm in _iconsGenerator.AvailableScales.Where(x => (new[] {100, 200, 400, 125, 150}).Contains(x.Scale)).ToArray())
            {
                scaleVm.IsChecked = true;
            }
            UpdateIcons();
        }

        private void Scales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateIcons();
        }
    }
}
