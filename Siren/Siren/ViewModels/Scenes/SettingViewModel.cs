using Siren.ViewModels.Players;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class SettingViewModel : IllustratedCardViewModel
    {
        public Guid BundleId { get; set; }

        public bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public void UpdateHasSelectedScene() => OnPropertyChanged(nameof(HasSelectedScene));
        public bool HasSelectedScene => Scenes?.Any(x => x.IsSelected) == true;

        private ObservableCollection<SceneViewModel> _scenes = new ObservableCollection<SceneViewModel>();
        public ObservableCollection<SceneViewModel> Scenes
        {
            get => _scenes;
            set
            {
                SetProperty(ref _scenes, value);
                _scenes.CollectionChanged += FireSettingChanged;
            }
        }


        private ObservableCollection<SceneComponentViewModel> _elements = new ObservableCollection<SceneComponentViewModel>();
        public ObservableCollection<SceneComponentViewModel> Elements
        {
            get => _elements;
            set
            {
                SetProperty(ref _elements, value);
                _elements.CollectionChanged += FireSettingChanged;
            }
        }

        private ObservableCollection<SceneComponentViewModel> _effects = new ObservableCollection<SceneComponentViewModel>();
        public ObservableCollection<SceneComponentViewModel> Effects
        {
            get => _effects;
            set
            {
                SetProperty(ref _effects, value);
                _effects.CollectionChanged += FireSettingChanged;
            }
        }

        private ObservableCollection<SceneComponentViewModel> _music = new ObservableCollection<SceneComponentViewModel>();
        public ObservableCollection<SceneComponentViewModel> Music
        {
            get => _music;
            set
            {
                SetProperty(ref _music, value);
                _music.CollectionChanged += FireSettingChanged;
            }
        }


        public event NotifyCollectionChangedEventHandler SettingChanged;
        private void FireSettingChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SettingChanged != null)
            {
                SettingChanged(sender, e);
            }
        }

        private SceneMusicPlayerViewModel _musicPlayer = new SceneMusicPlayerViewModel();
        public SceneMusicPlayerViewModel MusicPlayer
        {
            get => _musicPlayer;
            set => SetProperty(ref _musicPlayer, value);
        }
    }
}
