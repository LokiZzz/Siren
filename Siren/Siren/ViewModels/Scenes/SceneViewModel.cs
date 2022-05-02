using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                if (!string.IsNullOrEmpty(_imagePath))
                {
                    Stream stream = File.OpenRead(_imagePath);
                    Image = ImageSource.FromStream(() => stream);
                }
                OnPropertyChanged(nameof(Image));
            }
        }

        public void DeleteImageFile()
        {
            if (File.Exists(ImagePath))
            {
                File.Delete(ImagePath);
            }
        }

        public ObservableCollection<TrackSetupViewModel> Elements { get; set; } = new ObservableCollection<TrackSetupViewModel>();
    }
}
