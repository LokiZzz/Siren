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
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.Services
{
    public class SceneManager
    {
        public SettingViewModel SelectedSetting { get; set; }

        public SettingViewModel SettingToAdd { get; private set; }

        public SettingViewModel SettingToEdit { get; private set; }

        public void AddSetting(SettingViewModel setting)
        {
            SettingToAdd = setting;
            MessagingCenter.Send(this, Messages.SettingAdded);
        }

        public SceneViewModel SceneToAdd { get; private set; }

        public SceneViewModel SceneToEdit { get; private set; }

        public void AddScene(SceneViewModel scene)
        {
            SceneToAdd = scene;
            MessagingCenter.Send(this, Messages.SceneAdded);
        }

        public SceneComponentViewModel SceneComponentToEdit { get; set; }

        private string _bundleFileName = "current-bundle.json";

        public void SaveCurrentBundle(ObservableCollection<SettingViewModel> settings)
        {
            Bundle bundle = new Bundle { Settings = settings.Select(x => x.ToModel()).ToList() };
            string content = JsonConvert.SerializeObject(bundle);

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _bundleFileName);
            File.WriteAllText(fileName, content);
        }

        public ObservableCollection<SettingViewModel> GetCurrentBundle()
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
    }

}
