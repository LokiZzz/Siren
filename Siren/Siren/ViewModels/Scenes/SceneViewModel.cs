using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class SceneViewModel : BaseViewModel
    {
        public bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Name { get; set; }

        public ImageSource Image { get; set; }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                Image = ImageSource.FromFile(value);
                OnPropertyChanged(nameof(Image));
            }
        }

        public ObservableCollection<TrackSetupViewModel> Elements { get; set; } = new ObservableCollection<TrackSetupViewModel>();
    }
}
