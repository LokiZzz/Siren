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
                    IFileManager fileManager = DependencyService.Resolve<IFileManager>();
                    Image = ImageSource.FromStream(async (_) => await GetStream(_imagePath)); ;
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
            ImagePath = null;
        }

        private async Task<Stream> GetStream(string path)
        {
            IFileManager fileManager = DependencyService.Resolve<IFileManager>();
            MemoryStream memoryStream = new MemoryStream();

            Stream sourceStream = await fileManager.GetStreamToRead(path);
            sourceStream.CopyTo(memoryStream);
            sourceStream.Close();
            sourceStream.Dispose();
            memoryStream.Position = 0;
            
            return memoryStream;
        }
    }
}
