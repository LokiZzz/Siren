using Siren.Models;
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
        public MainViewModel()
        {
            InitializeMessagingCenter();

            AddSettingCommand = new Command(async () => await AddSetting());
        }

        public Command AddSettingCommand { get; }

        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        private void UpdateSettings(SceneManager manager)
        {
            foreach(Setting setting in manager.Settings)
            {
                if(!Settings.Any(x => x.Name == setting.Name))
                {
                    Settings.Add(setting);
                }
            }
        }

        private async Task AddSetting()
        {
            await Shell.Current.GoToAsync(nameof(AddOrEditSettingPage));
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<SceneManager>(this, SceneManagerMessages.SettingAdded, UpdateSettings);
        }
    }
}