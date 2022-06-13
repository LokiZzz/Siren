using Siren.Messaging;
using Siren.Models;
using Siren.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class BundlePageViewModel : BaseViewModel
    {
        public Command TestCommand { get => new Command(async () => await Test()); }
        private async Task Test()
        {
            await BundleService.Test();
        }

        private SceneManager SceneManager { get; }
        private IBundleService BundleService { get; }

        public BundlePageViewModel()
        {
            SceneManager = DependencyService.Get<SceneManager>();
            BundleService = DependencyService.Get<IBundleService>();
        }

        public ObservableCollection<Bundle> Bundles { get; set; }

        private string _newBundleName;
        public string NewBundleName
        {
            get { return _newBundleName; }
            set { _newBundleName = value; }
        }

        public Command CreateCommand { get => new Command(async () => await CreateBundle()); }
        public Command InstallCommand { get => new Command(async () => await InstallBundle()); }
        public Command ActivateCommand { get => new Command<Bundle>((bundle) => ActivateBundle(bundle)); }
        public Command DeactivateCommand { get => new Command<Bundle>(async (bundle) => await DeactivateBundle(bundle)); }
        public Command UninstallCommand { get => new Command<Bundle>(async (bundle) => await UninstallBundle(bundle)); }

        private async Task CreateBundle()
        {
            Bundle bundle = SceneManager.GetCurrentAgregatedBundle();
            bundle.Name = _newBundleName;
            string fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                GetNewBundleFileName()
            );

            await BundleService.SaveBundleAsync(bundle, fileName);
        }

        private async Task InstallBundle()
        {
            FileResult result = await FilePicker.PickAsync(GetSirenFilePickOption());
            Bundle unpackedBundle = await BundleService.LoadBundleAsync(result.FullPath);
            //ActivateBundle(unpackedBundle);
        }

        private void ActivateBundle(Bundle newBundle)
        {
            Bundle currentEnv = SceneManager.GetCurrentAgregatedBundle();

            if (currentEnv != null)
            {
                currentEnv.Settings.AddRange(newBundle.Settings);
            }
            else
            {
                currentEnv = newBundle;
            }

            SceneManager.SaveCurrentSettings(currentEnv.Settings);
            MessagingCenter.Send(this, Messages.NeedToUpdateEnvironment);
        }

        private Task DeactivateBundle(Bundle bundle)
        {
            throw new NotImplementedException();
        }

        private Task UninstallBundle(Bundle bundle)
        {
            throw new NotImplementedException();
        }

        private async Task LoadBundle()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TestBundle.siren");
            Bundle bundle = await BundleService.LoadBundleAsync(fileName);

            await BundleService.SaveBundleAsync(bundle, fileName);
        }

        private PickOptions GetSirenFilePickOption()
        {
            FilePickerFileType customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { ".siren" } },
                    { DevicePlatform.UWP, new[] { ".siren" } },
                }
            );

            return new PickOptions
            {
                PickerTitle = "Please select a Siren file",
                FileTypes = customFileType,
            };
        }

        private string GetNewBundleFileName()
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string name = rgx.Replace(NewBundleName, "");

            return $"{name}.siren";
        }
    }
}
