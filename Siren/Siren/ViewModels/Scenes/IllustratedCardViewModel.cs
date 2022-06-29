using Siren.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            set
            {
                SetProperty(ref _image, value);
                OnPropertyChanged(nameof(HasImage));
            }
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
                    Image = ImageSource.FromStream(async (token) => await GetStream(token, _imagePath));
                }
                OnPropertyChanged(nameof(Image));
            }
        }

        public bool HasImage => Image != null;

        public async Task DeleteImageFileAsync()
        {
            Image?.Cancel();
            Image = null;

            IFileManager fileManager = DependencyService.Resolve<IFileManager>();
            await fileManager.DeleteFileAsync(ImagePath);
        }

        private async Task<Stream> GetStream(CancellationToken cancelToken, string path)
        {
            IFileManager fileManager = DependencyService.Resolve<IFileManager>();

            return await fileManager.GetStreamToReadAsync(path);
        }
    }
}
