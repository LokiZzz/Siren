using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class SceneViewModel : IllustratedCardViewModel
    {
        public bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ObservableCollection<TrackSetupViewModel> Elements { get; set; } = new ObservableCollection<TrackSetupViewModel>();

        public void ReloadImage()
        {
            ImagePath = ImagePath;
            OnPropertyChanged(nameof(Image));
        }
    }
}
