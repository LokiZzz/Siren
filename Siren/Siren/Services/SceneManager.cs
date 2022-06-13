using Newtonsoft.Json;
using Siren.Messaging;
using Siren.Models;
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
        public SettingViewModel SelectedSetting { get; set; }

        public SettingViewModel SettingToAdd { get; private set; }

        public void AddSetting(SettingViewModel setting)
        {
            SettingToAdd = setting;
            MessagingCenter.Send(this, Messages.SettingAdded);
        }

        public SceneViewModel SceneToAdd { get; private set; }

        public void AddScene(SceneViewModel scene)
        {
            SceneToAdd = scene;
            MessagingCenter.Send(this, Messages.SceneAdded);
        }

        public IllustratedCardViewModel IllustratedCardToEdit { get; set; }
        public SceneComponentViewModel ComponentToEdit { get; set; }

        private string _currentEnvironmentFileName = "current-env.json";

        public void SaveCurrentSettings(List<Setting> settings)
        {
            Bundle bundle = new Bundle { Settings = settings };
            string content = JsonConvert.SerializeObject(bundle);

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _currentEnvironmentFileName);
            File.WriteAllText(fileName, content);
        }

        public void SaveCurrentSettings(ObservableCollection<SettingViewModel> settings)
        {
            SaveCurrentSettings(settings.Select(x => x.ToModel()).ToList());
        }

        public Bundle GetCurrentAgregatedBundle()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _currentEnvironmentFileName);
            if (File.Exists(fileName))
            {
                string content = File.ReadAllText(fileName);

                return JsonConvert.DeserializeObject<Bundle>(content);
            }
            else
            {
                return null;
            }
        }

        public ObservableCollection<SettingViewModel> GetCurrentSettings()
        {
            Bundle bundle = GetCurrentAgregatedBundle();
            
            return bundle != null
                ? bundle.Settings.Select(x => x.ToVM()).ToObservableCollection()
                : new ObservableCollection<SettingViewModel>();
        }
    }

}
