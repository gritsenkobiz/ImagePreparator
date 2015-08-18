using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageGenerator
{
    public class IconsGenerator
    {
        public Color Color { get; set; }
        public List<IconVm> Icons { get; set; }

        public bool IsWindows81 { get; set; }

        public List<ScaleVm> AvailableScales = new List<ScaleVm>()
        {
            new ScaleVm(80),
            new ScaleVm(100) {IsChecked = true},
            new ScaleVm(140) {IsChecked = true},
            new ScaleVm(180),
            new ScaleVm(240) {IsChecked = true},
            new ScaleVm(200),
            new ScaleVm(400),
            new ScaleVm(150),
            new ScaleVm(125),
        };

        private string _outputPath;

        public ImageSource SourceImage { get; set; }

        public IEnumerable<IconVm> UpdateIcons()
        {
            Icons = LoadPresets();

            foreach (var iconVm in Icons)
            {
                iconVm.Image = SourceImage;
                iconVm.Background = new SolidColorBrush(Color);
            }
            return Icons;
        }

        private List<IconVm> LoadPresets()
        {
            List<IconVm> result = null;

            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

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

                if(IsWindows81)
                {
                    result.AddRange(GetIcons(70, 70, "SquareLogo70"));
                    result.AddRange(GetIcons(30, 30, "SquareLogo30"));
                }

                //                var settings = JsonConvert.SerializeObject(result, Formatting.Indented);
                //                File.WriteAllText(settingsPath, settings);
            }
            return result;
        }


        public static void CreateIcon(FrameworkElement element, int width, int height, string path)
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

        private class MegaState
        {
            public string InputPath;
            public string OutputPath;

            public MegaState(string inputPath, string outputPath)
            {
                InputPath = inputPath;
                OutputPath = outputPath;
            }
        }

        public async Task CreateIcons(string outputPath, string inputPath = null)
        {
            Thread t = new Thread(RunCreateIcons);
            t.SetApartmentState(ApartmentState.STA);

            t.Start(new MegaState(inputPath,outputPath));

            while (t.IsAlive)
            {
                await Task.Delay(1000);
            }
        }

        private void RunCreateIcons(object state)
        {
            var p = state as MegaState;
            var outputPath = p.OutputPath;
            var inputPath = p.InputPath;

            if (inputPath != null)
            {
                var bm = new BitmapImage();
                bm.BeginInit();
                bm.StreamSource = File.OpenRead(inputPath);
                bm.EndInit();
                SourceImage = bm;
            }

            CreateIcons(Icons, SourceImage, Color, outputPath);
        }


        public static void CreateIcons(IEnumerable<IconVm> Icons, ImageSource imageSource, Color bgColor, string outputDir)
        {
            foreach (var icon in Icons)
            {
                var elemnet = GetRendreable(icon, imageSource, bgColor);
                CreateIcon(elemnet, icon.Width, icon.Height, Path.Combine(outputDir, icon.FileName));
            }
        }

        public static FrameworkElement GetRendreable(IconVm iconVm, ImageSource sourceIcon, Color color)
        {
            var border = new Border()
            {
                Background = new SolidColorBrush(color),
                Width = iconVm.Width,
                Height = iconVm.Height
            };
            var image = new Image()
            {
                Stretch = Stretch.Uniform,
                Source = sourceIcon,
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

        private IEnumerable<IconVm> GetIcons(int width, int height, string name, bool isBadge = false)
        {
            var scales = AvailableScales.Where(x => x.IsChecked).Select(x => x.Scale);
            return scales.Select(scale => new IconVm(width, height, scale, name, isBadge));
        }

    }
}