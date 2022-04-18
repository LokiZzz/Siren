﻿using Siren.Models;
using Siren.Services;
using Siren.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private SceneManager SceneManager { get; }

        public MainViewModel()
        {
            InitializeMessagingCenter();
            InitializeCommands();
            InitializeAudioEnvironment();

            SceneManager = DependencyService.Get<SceneManager>();
        }

        public Command AddSettingCommand { get; set; }
        public Command AddSceneCommand { get; set; }

        private Setting _selectedSetting;
        public Setting SelectedSetting
        {
            get 
            {
                return SceneManager.SelectedSetting;
            }
            set
            {
                SceneManager.SelectedSetting = value;
                SetProperty(ref _selectedSetting, value);
            }
        }

        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        private void InitializeAudioEnvironment()
        {
            if (SceneManager?.Settings != null)
            {
                Settings = new ObservableCollection<Setting>(SceneManager.Settings);
            }
        }

        private void UpdateSettings(SceneManager manager)
        {
            foreach (Setting setting in manager.Settings)
            {
                if (!Settings.Any(x => x.Name == setting.Name))
                {
                    Settings.Add(setting);
                }
            }
        }

        private async Task AddSetting()
        {
            await Shell.Current.GoToAsync(nameof(AddOrEditSettingPage));
        }

        private async Task AddScene()
        {
            await Shell.Current.GoToAsync(nameof(AddOrEditScenePage));
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<SceneManager>(this, SceneManagerMessages.SettingAdded, UpdateSettings);
        }

        private void InitializeCommands()
        {
            AddSettingCommand = new Command(async () => await AddSetting());
            AddSceneCommand = new Command(async () => await AddScene());
        }
    }
}