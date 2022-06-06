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

        private string _bundleFileName = "current-bundle.json";

        public void SaveCurrentSettings(ObservableCollection<SettingViewModel> settings)
        {
            Bundle bundle = new Bundle { Settings = settings.Select(x => x.ToModel()).ToList() };
            string content = JsonConvert.SerializeObject(bundle);

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _bundleFileName);
            File.WriteAllText(fileName, content);
        }

        public async Task<ObservableCollection<SettingViewModel>> GetCurrentSettings()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _bundleFileName);
            if (File.Exists(fileName))
            {
                string content = File.ReadAllText(fileName);
                Bundle bundle = JsonConvert.DeserializeObject<Bundle>(content);

                return bundle.Settings.Select(x => x.ToVM()).ToObservableCollection();
            }
            else
            {
                return new ObservableCollection<SettingViewModel>();
            }
        }

        public async Task<Bundle> GetCurrentBundle()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _bundleFileName);
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
    }

}
