using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class SettingViewModel : BaseViewModel
    {
        public SettingViewModel()
        {
            Scenes = new ObservableCollection<SceneViewModel>();
            Elements = new ObservableCollection<SceneComponentViewModel>();
            Effects = new ObservableCollection<SceneComponentViewModel>();
        }

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
            if(File.Exists(ImagePath))
            {
                File.Delete(ImagePath);
            }
        }

        private ObservableCollection<SceneViewModel> _scenes;
        public ObservableCollection<SceneViewModel> Scenes
        {
            get => _scenes;
            set
            {
                SetProperty(ref _scenes, value);
                _scenes.CollectionChanged += FireSettingChanged;
            }
        }

        private ObservableCollection<SceneComponentViewModel> _elements;
        public ObservableCollection<SceneComponentViewModel> Elements
        {
            get => _elements;
            set
            {
                SetProperty(ref _elements, value);
                _elements.CollectionChanged += FireSettingChanged;
            }
        }

        private ObservableCollection<SceneComponentViewModel> _effects;
        public ObservableCollection<SceneComponentViewModel> Effects
        {
            get => _effects;
            set
            {
                SetProperty(ref _effects, value);
                _effects.CollectionChanged += FireSettingChanged;
            }
        }


        public event NotifyCollectionChangedEventHandler SettingChanged;
        private void FireSettingChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SettingChanged(sender, e);
        }
    }
}
