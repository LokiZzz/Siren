using Siren.Models;
using Siren.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class AddOrEditSceneViewModel : BaseViewModel
    {
        private SceneManager SceneManager { get; }

        public AddOrEditSceneViewModel()
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
            Scene scene = new Scene { Name = Name };
            SceneManager.AddScene(scene);

            await Shell.Current.GoToAsync("..");
        }
    }
}
