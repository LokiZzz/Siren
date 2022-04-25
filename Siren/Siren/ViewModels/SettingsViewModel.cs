using Siren.Services;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Title = "Settings";

            ChooseMusicPathCommand = new Command(async () => await ChooseMusicPath());
            ChooseElementsPathCommand = new Command(async () => await ChooseElementsPath());
            ChooseEffectsPathCommand = new Command(async () => await ChooseEffectsPath());

        }

        private async Task ChooseEffectsPath()
        {
        }

        private async Task ChooseElementsPath()
        {
        }

        private async Task ChooseMusicPath()
        {
        }

        public Command ChooseMusicPathCommand { get; }
        public Command ChooseElementsPathCommand { get; }
        public Command ChooseEffectsPathCommand { get; }


        private string _musicPath;
        public string MusicPath
        {
            get => _musicPath;
            set => SetProperty(ref _musicPath, value);
        }

        private string _elementsPath;
        public string ElementsPath
        {
            get => _elementsPath;
            set => SetProperty(ref _elementsPath, value);
        }

        private string _effectsPath;
        public string EffectsPath
        {
            get => _effectsPath;
            set => SetProperty(ref _effectsPath, value);
        }
    }
}