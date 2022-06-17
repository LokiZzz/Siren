using Newtonsoft.Json;
using Siren.Messaging;
using Siren.Models;
using Siren.Utility;
using Siren.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.Services
{
    public class SceneManager
    {
        private string _currentEnvironmentFileName = "current-env.json";

        public IllustratedCardViewModel IllustratedCardToEdit { get; set; }
        public SceneComponentViewModel ComponentToEdit { get; set; }
        public SettingViewModel SettingToAdd { get; private set; }
        public SettingViewModel SelectedSetting { get; set; }
        public SceneViewModel SceneToAdd { get; private set; }

        public void AddSetting(SettingViewModel setting)
        {
            SettingToAdd = setting;
            MessagingCenter.Send(this, Messages.SettingAdded);
        }

        public void AddScene(SceneViewModel scene)
        {
            SceneToAdd = scene;
            MessagingCenter.Send(this, Messages.SceneAdded);
        }

        public async Task SaveEnvironment(List<Bundle> bundles)
        {
            await LocalDataHelper.WriteToTheLocalAppFile(bundles, _currentEnvironmentFileName);
        }

        public async Task<List<Bundle>> GetEnvironment()
        {
            List<Bundle> environment = await LocalDataHelper.GetObjectFromLocalAppFile<List<Bundle>>(_currentEnvironmentFileName);
            
            return environment ?? new List<Bundle>();
        }

        public async Task<List<Setting>> GetSettingsFromCurrentEnvironment(bool onlyActivatedBundles = true)
        {
            List<Setting> currentEnvironment = new List<Setting>();
            List<Bundle> bundles = await GetEnvironment();

            if (bundles != null && bundles.Any())
            {
                if (onlyActivatedBundles)
                {
                    bundles = bundles.Where(x => x.IsActivated).ToList();
                }

                currentEnvironment = bundles.SelectMany(x => x.Settings).ToList();
            }

            return currentEnvironment;
        }

        public async Task<ObservableCollection<SettingViewModel>> GetVMFromCurrentEnvironment(bool onlyActivatedBundles = true)
        {
            ObservableCollection<SettingViewModel> currentEnvironment = new ObservableCollection<SettingViewModel>();
            List<Setting> settings = await GetSettingsFromCurrentEnvironment(onlyActivatedBundles);
            
            if(settings != null && settings.Any())
            {
                currentEnvironment = settings.Select(x => x.ToVM()).ToObservableCollection();
            }

            return currentEnvironment;
        }

        public async Task SaveCurrentEnvironment(ObservableCollection<SettingViewModel> settings)
        {
            List<Bundle> environment = await GetEnvironment();

            if(!environment.Any(x => x.Id == Guid.Empty))
            {
                environment.Add(new Bundle 
                {
                    Id = Guid.Empty,
                    Name = "DefaultBundle",
                    Settings = new List<Setting>(),
                    IsActivated = true
                });
            }

            //Rewrite all settings to specific bundles (including DefaultBundle)
            foreach (Bundle bundle in environment)
            {
                bundle.Settings = settings.Where(x => x.BundleId == bundle.Id).Select(x => x.ToModel()).ToList();
            }

            await SaveEnvironment(environment);
        }
    }
}
