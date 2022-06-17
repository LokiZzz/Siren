using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class IllustratedCardViewModel : BaseViewModel
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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
                    Image = ImageSource.FromStream(() => File.OpenRead(_imagePath));
                }
                OnPropertyChanged(nameof(Image));
            }
        }

        public void DeleteImageFile()
        {
            Image?.Cancel();
            Image = null;

            if (File.Exists(ImagePath))
            {
                File.Delete(ImagePath);
            }
        }
    }
}
