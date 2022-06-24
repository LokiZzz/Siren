﻿using Siren.Messaging;
using Siren.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
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
                    Add();
                    break;
                case EAddOrEditIntent.Edit:
                    Edit();
                    MessagingCenter.Send(this, Messages.IllustratedCardEdited);
                    break;
            }

            await Shell.Current.GoToAsync("..");
        }

        private void Add()
        {
            switch (ComponentType)
            {
                case EComponentType.Setting:
                    SceneManager.AddSetting(new SettingViewModel { Name = this.Name, ImagePath = _imagePath,Image = this.Image });
                    break;
                case EComponentType.Scene:
                    SceneManager.AddScene(new SceneViewModel { Name = this.Name, ImagePath = _imagePath, Image = this.Image });
                    break;
            }
        }

        private void Edit()
        {
            switch(ComponentType)
            {
                case EComponentType.Setting:
                case EComponentType.Scene:
                    EditIllustratedCard();
                    break;
                case EComponentType.Element:
                case EComponentType.Effect:
                    EditSceneComponent();
                    break;
            }
        }

        private void EditIllustratedCard()
        {
            SceneManager.IllustratedCardToEdit.Name = Name;
            SceneManager.IllustratedCardToEdit.ImagePath = _imagePath;
        }

        private void EditSceneComponent()
        {
            SceneManager.ComponentToEdit.Alias = Name;
        }

        public void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            string intentString = HttpUtility.UrlDecode(query["intent"]);
            Enum.TryParse(intentString, out EAddOrEditIntent intent);
            Intent = intent;

            string componentString = HttpUtility.UrlDecode(query["component"]);
            Enum.TryParse(componentString, out EComponentType component);
            ComponentType = component;

            if(intent == EAddOrEditIntent.Edit)
            {
                if (component == EComponentType.Element || component == EComponentType.Effect)
                {
                    Name = string.IsNullOrEmpty(SceneManager.ComponentToEdit.Alias)
                        ? SceneManager.ComponentToEdit.Name
                        : SceneManager.ComponentToEdit.Alias;
                }
                if (component == EComponentType.Setting || component == EComponentType.Scene)
                {
                    Name = SceneManager.IllustratedCardToEdit.Name;
                    _imagePath = SceneManager.IllustratedCardToEdit.ImagePath;
                    if (!string.IsNullOrEmpty(_imagePath))
                    {
                        Image = ImageSource.FromStream(async (token) => await GetStream(token, _imagePath));
                    }
                    OnPropertyChanged(nameof(Image));
                }
            }

            InitializeVisibilityProperties();
        }

        private async Task<Stream> GetStream(CancellationToken cancelToken, string path)
        {
            IFileManager fileManager = DependencyService.Resolve<IFileManager>();

            return await fileManager.GetStreamToReadAsync(path);
        }

        #region Controls visibility

        private bool _isImagePickerVisible;
        public bool IsImagePickerVisible
        {
            get => _isImagePickerVisible;
            set => SetProperty(ref _isImagePickerVisible, value);
        }

        private void InitializeVisibilityProperties()
        {
            IsImagePickerVisible = ComponentType == EComponentType.Setting
                || ComponentType == EComponentType.Scene;
        }

        #endregion
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
