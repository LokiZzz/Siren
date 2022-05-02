using Siren.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Essentials;
using Xamarin.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Siren.ViewModels
{
    public class AddOrEditComponentViewModel : BaseViewModel, IQueryAttributable
    {
        private SceneManager SceneManager { get; }

        public AddOrEditComponentViewModel()
        {
            SceneManager = DependencyService.Resolve<SceneManager>();

            SelectImageCommand = new Command(SelectImage);
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            PropertyChanged += (_, __) => SaveCommand.ChangeCanExecute();
        }

        public EAddOrEditIntent Intent { get; set; }
        public EComponentType ComponentType { get; set; }

        public Command SelectImageCommand { get; }
        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _imagePath;

        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value); 
        }

        private FileResult _imageFileResult;

        private async void SelectImage()
        {
            _imageFileResult = await FilePicker.PickAsync(PickOptions.Images);

            if (_imageFileResult != null)
            {
                _imagePath = _imageFileResult.FullPath;
                Stream stream = await _imageFileResult.OpenReadAsync();
                Image = ImageSource.FromStream(() => stream);
            }
        }

        private bool ValidateSave() => !string.IsNullOrWhiteSpace(_name);

        private async void OnCancel() => await Shell.Current.GoToAsync("..");

        private async void OnSave()
        {
            switch(Intent)
            {
                case EAddOrEditIntent.Add:
                    await Add();
                    break;
                case EAddOrEditIntent.Edit:
                    await Edit();
                    break;
            }

            await Shell.Current.GoToAsync("..");
        }

        private async Task Add()
        {
            string imagePath = string.Empty;

            if (Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(_imageFileResult.FullPath);
                imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
                Stream stream = await _imageFileResult.OpenReadAsync();
                using (FileStream fileStream = File.Create(imagePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
            }

            switch (ComponentType)
            {
                case EComponentType.Setting:
                    SceneManager.AddSetting(new SettingViewModel 
                    { 
                        Name = this.Name, 
                        ImagePath = imagePath,
                        Image = this.Image 
                    });
                    break;
                case EComponentType.Scene:
                    break;
            }
        }

        private async Task Edit()
        {
            switch (ComponentType)
            {
                case EComponentType.Setting:
                    break;
                case EComponentType.Scene:
                    break;
                case EComponentType.Element:
                    break;
                case EComponentType.Effect:
                    break;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            string intentString = HttpUtility.UrlDecode(query["intent"]);
            Enum.TryParse(intentString, out EAddOrEditIntent intent);
            Intent = intent;

            string componentString = HttpUtility.UrlDecode(query["component"]);
            Enum.TryParse(componentString, out EComponentType component);
            ComponentType = component;
        }
    }

    public enum EAddOrEditIntent
    {
        Add = 1,
        Edit = 2
    }

    public enum EComponentType
    {
        Setting = 1,
        Scene = 2,
        Element = 3,
        Effect = 4
    }
}
