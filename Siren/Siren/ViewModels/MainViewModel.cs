﻿using Siren.Messaging;
using Siren.Models;
using Siren.Services;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Siren.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private SceneManager SceneManager { get; }

        public MainViewModel()
        {
            InitializeMessagingCenter();
            SceneManager = DependencyService.Get<SceneManager>();
            IntializeCollections();
        }

        private async Task GoToAddSetting()
        {
            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Add}&component={EComponentType.Setting}"
            );
        }

        private void AddSetting(SceneManager manager)
        {
            SettingViewModel newSetting = new SettingViewModel
            {
                Name = SceneManager.SettingToAdd.Name,
                ImagePath = SceneManager.SettingToAdd.ImagePath,
            };
            Settings.Add(newSetting);
            SelectedSetting = newSetting;
        }

        private async Task GoToEditSetting(SettingViewModel setting)
        {
            SceneManager.IllustratedCardToEdit = setting;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Setting}"
            );
        }

        private void SelectSetting(SettingViewModel setting)
        {
            SelectedSetting = setting;
            SceneManager.SelectedSetting = SelectedSetting;
            SelectedSetting.Scenes.ForEach(x => x.ReloadImage());
        }

        private async Task DeleteSetting(SettingViewModel setting)
        {
            bool wantToDelete = await App.Current.MainPage.DisplayAlert(
                "Warning",
                $"Are you really want to delete \"{setting.Name}\" setting?",
                "Yes", "No"
            );

            if (wantToDelete)
            {
                //If other settings does not use Image, then:
                setting.DeleteImageFile();

                //If other scenes of other settings does not use Image, then:
                setting.Scenes.ForEach(x => x.DeleteImageFile());

                setting.Scenes.Clear();
                setting.Elements.ForEach(x => x.Dispose());
                setting.Elements.Clear();
                setting.Effects.ForEach(x => x.Dispose());
                setting.Effects.Clear();
                Settings.Remove(setting);
            }
        }

        private async Task GoToAddScene()
        {
            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Add}&component={EComponentType.Scene}"
            );
        }

        private void AddScene(SceneManager manager)
        {
            SceneViewModel newScene = new SceneViewModel
            {
                Name = SceneManager.SceneToAdd.Name,
                ImagePath = SceneManager.SceneToAdd.ImagePath,
            };
            SelectedSetting.Scenes.Add(newScene);
        }

        private async Task GoToEditScene(SceneViewModel scene)
        {
            SceneManager.IllustratedCardToEdit = scene;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Scene}"
            );
        }

        private async Task SelectScene(SceneViewModel scene)
        {
            Settings.SelectMany(x => x.Scenes).ForEach(x => x.IsSelected = false);
            SelectedScene = scene;

            foreach (SceneComponentViewModel element in Settings.SelectMany(x => x.Elements))
            {
                TrackSetupViewModel existingElement = SelectedScene.Elements
                    .FirstOrDefault(x => x.FilePath.Equals(element.FilePath));

                if (existingElement != null)
                {
                    if (!element.IsPlaying || element.Volume != existingElement.Volume)
                    {
                        await element.SmoothPlay(targetVolume: existingElement.Volume);
                    }
                }
                else
                {
                    if (element.IsPlaying)
                    {
                        element.SmoothStop();
                    }
                }
            }

            OnPropertyChanged(nameof(IsScenePlaying));
            OnPropertyChanged(nameof(CurrentSceneText));
            Settings.ForEach(x => x.UpdateHasSelectedScene());
        }

        private async Task DeleteScene(SceneViewModel scene)
        {
            bool wantToDelete = await App.Current.MainPage.DisplayAlert(
                "Warning",
                $"Are you really want to delete \"{scene.Name}\" scene?",
                "Yes", "No"
            );

            if (wantToDelete)
            {
                SelectedSetting.Scenes.Remove(scene);
            }
        }

        private void SaveScene(string name)
        {
            var playingElements = SelectedSetting.Elements
                .Where(x => x.IsPlaying)
                .Select(x => new TrackSetupViewModel
                {
                    FilePath = x.FilePath,
                    Volume = x.Volume,
                })
                .ToList();

            SelectedScene.Elements = new ObservableCollection<TrackSetupViewModel>(playingElements);

            SaveCurrentBundle();
        }

        private async Task AddElements()
        {
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            foreach(FileResult element in result)
            {
                if (!SelectedSetting.Elements.Any(x => x.FilePath.Equals(element.FullPath)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel { Loop = true, FilePath = element.FullPath };
                    SelectedSetting.Elements.Add(player);
                }
            }
        }

        private async Task GoToEditElement(SceneComponentViewModel element)
        {
            SceneManager.ComponentToEdit = element;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Element}"
            );
        }

        private void DeleteElement(SceneComponentViewModel component)
        {
            SelectedSetting.Elements.Remove(component);
            component.Dispose();
        }

        private async Task AddEffects()
        {
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            foreach (FileResult element in result)
            {
                if (!SelectedSetting.Effects.Any(x => x.FilePath.Equals(element.FullPath)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel { FilePath = element.FullPath };
                    SelectedSetting.Effects.Add(player);
                }
            }
        }

        private async Task GoToEditEffect(SceneComponentViewModel effect)
        {
            SceneManager.ComponentToEdit = effect;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Effect}"
            );
        }

        private void DeleteEffect(SceneComponentViewModel component)
        {
            SelectedSetting.Effects.Remove(component);
            component.Dispose();
        }

        private async Task GlobalPlayStop()
        {
            if(IsScenePlaying)
            {
                Settings.SelectMany(x => x.Elements)
                    .Where(x => x.IsPlaying)
                    .ForEach(x => x.SmoothStop());
            }
            else
            {
                if (SelectedScene != null)
                {
                    await SelectScene(SelectedScene);
                }
            }
            OnPropertyChanged(nameof(IsScenePlaying));
        }

        private void SaveCurrentBundle()
        {
            SceneManager.SaveCurrentSettings(Settings);
        }

        private void IntializeCollections()
        {
            Settings = SceneManager.GetCurrentSettings();
            SelectedSetting = Settings.FirstOrDefault();

            Settings.CollectionChanged += BindNewSettingEvents;
            Settings.CollectionChanged += SaveCurrentBundle;
            Settings.ForEach(x => x.SettingChanged += SaveCurrentBundle);
        }

        private void BindNewSettingEvents(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(SettingViewModel item in e.NewItems)
                {
                    item.SettingChanged += SaveCurrentBundle;
                }
            }
        }

        private void SaveCurrentBundle(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveCurrentBundle();
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<SceneComponentViewModel>(this, Messages.ElementPlayingStatusChanged, vm => OnPropertyChanged(nameof(IsScenePlaying)));
            MessagingCenter.Subscribe<SceneManager>(this, Messages.SettingAdded, AddSetting);
            MessagingCenter.Subscribe<SceneManager>(this, Messages.SceneAdded, AddScene);
            MessagingCenter.Subscribe<AddOrEditComponentViewModel>(this, Messages.IllustratedCardEdited, (manager) => SaveCurrentBundle());
            MessagingCenter.Subscribe<BundlePageViewModel>(this, Messages.NeedToUpdateEnvironment, (manager) => IntializeCollections());
        }

        public Command AddSettingCommand { get => new Command(async () => await GoToAddSetting()); }
        public Command EditSettingCommand { get => new Command<SettingViewModel>(async (setting) => await GoToEditSetting(setting)); }
        public Command SelectSettingCommand { get => new Command<SettingViewModel>(SelectSetting); }
        public Command DeleteSettingCommand { get => new Command<SettingViewModel>(async (scetting) => await DeleteSetting(scetting)); }
        public Command AddSceneCommand { get => new Command(async () => await GoToAddScene()); }
        public Command EditSceneCommand { get => new Command<SceneViewModel>(async (scene) => await GoToEditScene(scene)); }
        public Command SelectSceneCommand { get => new Command<SceneViewModel>(async (scene) => await SelectScene(scene)); }
        public Command DeleteSceneCommand { get => new Command<SceneViewModel>(async (scene) => await DeleteScene(scene)); }
        public Command AddElementsCommand { get => new Command(async () => await AddElements()); }
        public Command EditElementCommand { get => new Command<SceneComponentViewModel>(async (element) => await GoToEditElement(element)); }
        public Command DeleteElementCommand { get => new Command<SceneComponentViewModel>(DeleteElement); }
        public Command AddEffectsCommand { get => new Command(async () => await AddEffects()); }
        public Command EditEffectCommand { get => new Command<SceneComponentViewModel>(async (effect) => await GoToEditEffect(effect)); }
        public Command DeleteEffectCommand { get => new Command<SceneComponentViewModel>(DeleteEffect); }
        public Command SaveSceneCommand { get => new Command<string>(SaveScene); }
        public Command GlobalPlayCommand { get => new Command(async () => await GlobalPlayStop()); }

        public ObservableCollection<SettingViewModel> Settings { get; set; } = new ObservableCollection<SettingViewModel>();

        private SettingViewModel _selectedSetting;
        public SettingViewModel SelectedSetting
        {
            get => _selectedSetting;
            set
            {
                if (value == null)
                {
                    return;
                }

                SetProperty(ref _selectedSetting, value);
                foreach (SettingViewModel item in Settings)
                {
                    item.IsSelected = false;
                }
                SelectedSetting.IsSelected = true;
            }
        }

        private SceneViewModel _selectedScene;
        public SceneViewModel SelectedScene
        {
            get => _selectedScene;
            set
            {
                SetProperty(ref _selectedScene, value);
                foreach (SceneViewModel item in SelectedSetting.Scenes)
                {
                    item.IsSelected = false;
                }
                SelectedScene.IsSelected = true;
            }
        }

        public bool IsScenePlaying => Settings.SelectMany(x => x.Elements).Any(x => x.IsPlaying) == true;

        public string CurrentSceneText
        {
            get
            {
                string current;

                if(SelectedSetting == null || SelectedScene == null)
                {
                    current = "still does not selected...";
                }
                else
                {
                    string setting = Settings.FirstOrDefault(x => x.Scenes.Contains(SelectedScene)).Name;
                    current = $"{setting} — { SelectedScene.Name}";
                }

                return $"Current scene: {current}";
            }
        }

    }
}