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

        public bool _isMusicEnabled;
        public bool IsMusicEnabled
        {
            get => _isMusicEnabled;
            set => SetProperty(ref _isMusicEnabled, value);
        }

        public bool _isMusicShuffled;
        public bool IsMusicShuffled
        {
            get => _isMusicShuffled;
            set => SetProperty(ref _isMusicShuffled, value);
        }

        public ObservableCollection<TrackSetupViewModel> Elements { get; set; } = new ObservableCollection<TrackSetupViewModel>();

        public void ReloadImage()
        {
            ImagePath = ImagePath;
            OnPropertyChanged(nameof(Image));
        }
    }
}
