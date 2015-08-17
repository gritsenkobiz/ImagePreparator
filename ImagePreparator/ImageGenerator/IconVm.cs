using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Newtonsoft.Json;

namespace ImageGenerator
{
    [JsonObject]
    public class IconVm : INotifyPropertyChanged
    {
        private SolidColorBrush _background;

        [JsonProperty]
        public int ImageWidth { get; set; }
        [JsonProperty]
        public int ImageHeight { get; set; }
        [JsonProperty]
        public int Width { get; set; }
        [JsonProperty]
        public int Height { get; set; }

        [JsonProperty]
        public string FileName { get; set; }

        [JsonProperty]
        public int Scale { get; set; }

        [JsonProperty]
        public bool IsBadge { get; set; }

        [JsonIgnore]
        public ImageSource Image { get; set; }

        [JsonIgnore]
        public SolidColorBrush Background
        {
            get { return _background; }
            set
            {
                if (IsBadge)
                    return;

                _background = value; 
                OnPropertyChanged();
            }
        }

        public IconVm(int width, int height, int scale, string fileName = null, bool isBadge = false)
        {
            IsBadge = isBadge;

            Scale = scale;
            Width = (int) Math.Round(width * Scale *0.01, MidpointRounding.AwayFromZero);
            Height = (int) Math.Round(height*Scale*0.01, MidpointRounding.AwayFromZero);
            if (isBadge)
            {
                ImageWidth = (int)(Width);
                ImageHeight = (int)(Height);
            }
            else
            {
                ImageWidth = (int)(Width * 0.5);
                ImageHeight = (int)(Height * 0.5);
            }

            FileName = $"{fileName}_{width}x{height}_Scale-{Scale}({Width}x{Height}).png";
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}