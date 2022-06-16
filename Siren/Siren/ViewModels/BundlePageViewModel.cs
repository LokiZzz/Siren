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
        private SceneManager SceneManager { get; }
        private IBundleService BundleService { get; }

        public BundlePageViewModel()
        {
            SceneManager = DependencyService.Get<SceneManager>();
            BundleService = DependencyService.Get<IBundleService>();

            Bundles = SceneManager.GetEnvironment()
                .Where(x => x.Id != Guid.Empty)
                .Select(x => new BundleViewModel(x))
                .ToObservableCollection();
        }

        public ObservableCollection<BundleViewModel> Bundles { get; set; } = new ObservableCollection<BundleViewModel>();

        private string _newBundleName;
        public string NewBundleName
        {
            get { return _newBundleName; }
            set { _newBundleName = value; }
        }

        public Command CreateCommand { get => new Command(async () => await CreateBundle()); }
        public Command InstallCommand { get => new Command(async () => await InstallBundle()); }
        public Command ActivateDeactivateCommand { get => new Command<Bundle>((bundle) => ActivateDeactivateBundle(bundle.Id)); }
        public Command UninstallCommand { get => new Command<Bundle>(async (bundle) => await UninstallBundle(bundle.Id)); }

        private async Task CreateBundle()
        {
            Bundle bundle = new Bundle 
            {
                Name = _newBundleName,
                Settings = SceneManager.GetSettingsFromCurrentEnvironment() 
            };

            string fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                GetNewBundleFileName()
            );

            await BundleService.SaveBundleAsync(bundle, fileName);
        }

        private async Task InstallBundle()
        {
            FileResult result = await FilePicker.PickAsync(GetSirenFilePickOption());

            if (result != null)
            {
                Bundle unpackedBundle = await BundleService.LoadBundleAsync(result.FullPath);
                Bundles.Add(new BundleViewModel(unpackedBundle));

                List<Bundle> bundles = SceneManager.GetEnvironment();
                bundles.Add(unpackedBundle);
                SceneManager.SaveEnvironment(bundles);

                ActivateDeactivateBundle(unpackedBundle.Id);
            }
        }

        private void ActivateDeactivateBundle(Guid bundleId)
        {
            List<Bundle> bundles = SceneManager.GetEnvironment();
            Bundle bundleToActivate = bundles.FirstOrDefault(x => x.Id == bundleId);
            bundleToActivate.IsActivated = !bundleToActivate.IsActivated;
            SceneManager.SaveEnvironment(bundles);

            MessagingCenter.Send(this, Messages.NeedToUpdateEnvironment);
        }


        private async Task UninstallBundle(Guid bundleId)
        {
            //Delete from environment
            List<Bundle> bundles = SceneManager.GetEnvironment();
            Bundle bundleToRemove = bundles.FirstOrDefault(x => x.Id == bundleId);
            bundles.Remove(bundleToRemove);
            SceneManager.SaveEnvironment(bundles);

            //Update MainViewModel
            MessagingCenter.Send(this, Messages.NeedToUpdateEnvironment);

            //Delete files
            await BundleService.DeleteBundleFilesAsync(bundleId);

            //Delete local VM
            Bundles.Remove(Bundles.First(x => x.Bundle.Id == bundleId));
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

    public class BundleViewModel : BaseViewModel
    {
        public BundleViewModel(Bundle bundle)
        {
            Bundle = bundle;
        }

        public Bundle Bundle { get; set; }

        private bool _isActivated;
        public bool IsActivated 
        { 
            get => Bundle.IsActivated;
            set
            {
                SetProperty(ref _isActivated, value);
                Bundle.IsActivated = value;
            }
        }
    }
}
