using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImagePreparator.Annotations;

namespace ImagePreparator
{
    public class ScaleVm: INotifyPropertyChanged
    {
        private bool _isChecked;
        public string Text => Scale.ToString();
        public int Scale { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value; 
                OnPropertyChanged();
            }
        }

        public ScaleVm(int scale)
        {
            Scale = scale;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}