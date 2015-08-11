using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Path = System.IO.Path;

namespace ImagePreparator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Color _color;
        public List<IconVm> Icons { get; set; }

        public List<ScaleVm> AvailableScales = new List<ScaleVm>()
        {
            new ScaleVm(100) {IsChecked = true},
            new ScaleVm(140) {IsChecked = true},
            new ScaleVm(240) {IsChecked = true},
            new ScaleVm(200),
            new ScaleVm(400),
            new ScaleVm(150),
            new ScaleVm(125),
        };

        private string _outputDir;

        public MainWindow()
        {
            InitializeComponent();
            UpdateIcons();

            Scales.ItemsSource = AvailableScales;
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

            Icons = LoadPresets();

            foreach (var iconVm in Icons)
            {
                iconVm.Image = IconPreview.Source;
                iconVm.Background = new SolidColorBrush(_color);
            }

            ResultImages.ItemsSource = Icons;
        }

        private List<IconVm> LoadPresets()
        {
            List<IconVm> result = null;

            var settingsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            //if (File.Exists(settingsPath))
            //{
            //    var settings = File.ReadAllText(settingsPath);
            //    result = JsonConvert.DeserializeObject<List<IconVm>>(settings);
            //}

            if (result == null)
            {
                result = new List<IconVm>();
                result.AddRange(GetIcons(150, 150, "Logo"));
                result.AddRange(GetIcons(310, 150, "WideLogo"));
                result.AddRange(GetIcons(310, 310, "BigSquareLogo"));
                result.AddRange(GetIcons(44, 44, "SmallLogo"));
                result.AddRange(GetIcons(71, 71, "SquareLogo71"));
                result.AddRange(GetIcons(50, 50, "StoreLogo"));
                result.AddRange(GetIcons(24, 24, "BadgeLogo", true));

//                var settings = JsonConvert.SerializeObject(result, Formatting.Indented);
//                File.WriteAllText(settingsPath, settings);
            }
            return result;
        }

        private IEnumerable<IconVm> GetIcons(int width, int height, string name, bool isBadge = false)
        {
            var scales = AvailableScales.Where(x=>x.IsChecked).Select(x=>x.Scale);
            return scales.Select(scale => new IconVm(width, height, scale, name, isBadge));
        }

        public void CreateIcons()
        {
            foreach (var icon in Icons)
            {
                var elemnet = GetRendreable(icon);
                CreateIcon(elemnet, icon.Width, icon.Height, System.IO.Path.Combine(_outputDir, icon.FileName));
            }

            Process.Start(_outputDir);
        }

        private FrameworkElement GetRendreable(IconVm iconVm)
        {
            var border = new Border()
            {
                Background = new SolidColorBrush(_color),
                Width = iconVm.Width,
                Height = iconVm.Height
            };
            var image = new Image()
            {
                Stretch = Stretch.Uniform,
                Source = IconPreview.Source,
                Width = iconVm.ImageWidth,
                Height = iconVm.ImageHeight,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };


            border.Child = image;

            border.Measure(new Size(border.Width, border.Height));
            border.Arrange(new Rect(new Size(border.Width, border.Height)));
            border.UpdateLayout();
            return border;
        }

        public void CreateIcon(FrameworkElement element, int width,int height,string path)
        {
            try
            {
                RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(element);

                string file = path;

                string Extension = Path.GetExtension(file).ToLower();

                BitmapEncoder encoder;
                if (Extension == ".gif")
                    encoder = new GifBitmapEncoder();
                else if (Extension == ".png")
                    encoder = new PngBitmapEncoder();
                else if (Extension == ".jpg")
                    encoder = new JpegBitmapEncoder();
                else
                    return;

                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using (Stream stm = File.Create(file))
                {
                    encoder.Save(stm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                _color = wb.GetPixel((int) pos.X, (int) pos.Y);

                ColorPicker1.SelectedColor = _color;
            }
            catch (Exception ex)
            {
            }
        }

        private void ColorPicker1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            _color = ColorPicker1.SelectedColor ?? Colors.Transparent;
            UpdateIcons();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var availableScale in AvailableScales)
            {
                availableScale.IsChecked = false;
            }

            foreach (var scaleVm in AvailableScales.Where(x => (new[] { 100, 140, 240 }).Contains(x.Scale)).ToArray())
            {
                scaleVm.IsChecked = true;
            }
            UpdateIcons();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            foreach (var availableScale in AvailableScales)
            {
                availableScale.IsChecked = false;
            }

            foreach (var scaleVm in AvailableScales.Where(x => (new[] {100, 200, 400, 125, 150}).Contains(x.Scale)).ToArray())
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
