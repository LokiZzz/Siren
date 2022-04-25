using Siren.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class AddOrEditSettingViewModel : BaseViewModel
    {
        private SceneManager SceneManager { get; }

        public AddOrEditSettingViewModel()
        {
            SceneManager = DependencyService.Resolve<SceneManager>();

            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged += (_, __) => SaveCommand.ChangeCanExecute();
        }

        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(_name);
        }

        private async void OnCancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSave()
        {
            Setting newSetting = new Setting { Name = Name };
            SceneManager.AddSetting(newSetting);

            await Shell.Current.GoToAsync("..");
        }
    }
}
