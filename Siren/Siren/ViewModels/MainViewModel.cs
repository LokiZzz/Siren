using Siren.Models;
using Siren.Services;
using Siren.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            Settings = new ObservableCollection<Scene>
            {
                new Scene { Name = "First scene" },
                new Scene { Name = "Second scene" },
                new Scene { Name = "Third scene" }
            };

            AddSettingCommand = new Command(async () => await AddSetting());
        }

        public Command AddSettingCommand { get; }

        public ObservableCollection<Scene> Settings { get; set; }

        private async Task AddSetting()
        {
            await Shell.Current.GoToAsync(nameof(AddOrEditSettingPage));
        }
    }
}