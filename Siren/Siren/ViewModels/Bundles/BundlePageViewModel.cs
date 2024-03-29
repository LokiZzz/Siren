﻿using Siren.Messaging;
using Siren.Models;
using Siren.Services;
using Siren.ViewModels.Bundles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
            BundleService.OnCreateProgressUpdate += UpdateCreateProgress;
            BundleService.OnInstallProgressUpdate += UpdateInstallProgress;

            Task.Run(async () => { await InitializeInstalledBundles(); }).Wait();

            IntializeCommands();
        }

        private async Task InitializeInstalledBundles()
        {
            Bundles = (await SceneManager.GetEnvironment())
                .Where(x => x.Id != Guid.Empty)
                .Select(x => new BundleViewModel(x))
                .ToObservableCollection();

            OnPropertyChanged(nameof(Bundles));
        }

        public ObservableCollection<BundleViewModel> Bundles { get; set; } = new ObservableCollection<BundleViewModel>();

        private string _newBundleName;
        public string NewBundleName
        {
            get { return _newBundleName; }
            set 
            { 
                SetProperty(ref _newBundleName, value);
                CreateCommand.ChangeCanExecute();
            }
        }

        CancellationTokenSource _creatingCancellationTokenSource;

        private async Task CreateBundle()
        {
            using (_creatingCancellationTokenSource = new CancellationTokenSource())
            {
                CreatingProgress = 0;
                BundleSystemState = EBundleSystemState.Creating;
                string bundleName = GetNewBundleFileName();

                Bundle bundle = new Bundle
                {
                    Name = NewBundleName,
                    Settings = await SceneManager.GetSettingsFromCurrentEnvironment()
                };

                bool created = await BundleService.SaveBundleAsync(bundle, bundleName, _creatingCancellationTokenSource.Token);

                if (created)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "Bundle creation complete",
                        $"The bundle has been successfully created. ",
                        "Nice!"
                    );
                }

                BundleSystemState = EBundleSystemState.Default;
            }
        }

        private CancellationTokenSource _installingCancellationTokenSource;

        private async Task InstallBundle()
        {
            using (_installingCancellationTokenSource = new CancellationTokenSource())
            {
                InstallProgress = 0;
                BundleSystemState = EBundleSystemState.Installing;
                Bundle unpackedBundle = null;

                try
                {
                    unpackedBundle = await BundleService.LoadBundleAsync(
                        Bundles.Select(x => x.Bundle.Id).ToList(),
                        _installingCancellationTokenSource.Token
                    );
                }
                catch (BundleAlreadyInstalledException)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "Bundle already installed",
                        $"Bundle you want to install is already installed.",
                        "Ok"
                    );

                    BundleSystemState = EBundleSystemState.Default;

                    return;
                }

                if (unpackedBundle != null)
                {
                    unpackedBundle.IsActivated = true;
                    Bundles.Add(new BundleViewModel(unpackedBundle));

                    List<Bundle> bundles = await SceneManager.GetEnvironment();
                    bundles.Add(unpackedBundle);
                    await SceneManager.SaveEnvironment(bundles);

                    MessagingCenter.Send(this, Messages.NeedToUpdateEnvironment);
                }

                BundleSystemState = EBundleSystemState.Default;
            }
        }

        private async Task ActivateDeactivateBundle(Guid bundleId)
        {
            List<Bundle> bundles = await SceneManager.GetEnvironment();
            Bundle bundleToActivate = bundles.FirstOrDefault(x => x.Id == bundleId);
            bundleToActivate.IsActivated = !bundleToActivate.IsActivated;
            await SceneManager.SaveEnvironment(bundles);

            await InitializeInstalledBundles();

            MessagingCenter.Send(this, Messages.NeedToUpdateEnvironment);
        }


        private async Task UninstallBundle(Guid bundleId)
        {
            IsBusy = true;

            List<Bundle> bundles = await SceneManager.GetEnvironment();
            Bundle bundleToRemove = bundles.FirstOrDefault(x => x.Id == bundleId);
            bundles.Remove(bundleToRemove);
            await SceneManager.SaveEnvironment(bundles);
            await BundleService.DeleteBundleFilesAsync(bundleId);
            Bundles.Remove(Bundles.First(x => x.Bundle.Id == bundleId));

            await InitializeInstalledBundles();
            MessagingCenter.Send(this, Messages.NeedToUpdateEnvironment);
            IsBusy = false;
        }

        private string GetNewBundleFileName()
        {
            Regex rgx = new Regex("[^a-zA-Zа-яА-Я0-9 -]");
            string name = rgx.Replace(NewBundleName, "");

            return $"{name}.siren";
        }

        private string _installProgressMessage = "Installing progress...";
        public string InstallProgressMessage
        {
            get => _installProgressMessage;
            set => SetProperty(ref _installProgressMessage, value);
        }

        private double _installProgress;
        public double InstallProgress
        {
            get => _installProgress;
            set => SetProperty(ref _installProgress, value);
        }

        private string _creatingProgressMessage = "Creating progress...";
        public string CreatingProgressMessage
        {
            get => _creatingProgressMessage;
            set => SetProperty(ref _creatingProgressMessage, value);
        }

        private double _creatingProgress;
        public double CreatingProgress
        {
            get => _creatingProgress;
            set => SetProperty(ref _creatingProgress, value);
        }

        private void UpdateInstallProgress(object sender, ProcessingProgress e)
        {
            InstallProgress = e.Progress;
            InstallProgressMessage = e.Message;
        }

        private void UpdateCreateProgress(object sender, ProcessingProgress e)
        {
            CreatingProgress = e.Progress;
            CreatingProgressMessage = e.Message;
        }

        private EBundleSystemState _bundleSystemState = EBundleSystemState.Default;
        public EBundleSystemState BundleSystemState
        {
            get => _bundleSystemState;
            set
            {
                SetProperty(ref _bundleSystemState, value);
                CreateCommand.ChangeCanExecute();
                CancelCreateCommand.ChangeCanExecute();
                InstallCommand.ChangeCanExecute();
                CancelInstallCommand.ChangeCanExecute();
                ShowCreateProgress = value == EBundleSystemState.Creating;
                ShowInstallProgress = value == EBundleSystemState.Installing;
            }
        }

        public Command CreateCommand { get; set; }
        public Command CancelCreateCommand { get; set; }
        public Command InstallCommand { get; set; }
        public Command CancelInstallCommand { get; set; }
        public Command ActivateDeactivateCommand { get; set; }
        public Command UninstallCommand { get; set; }

        private bool GetCreateCommandCanExecute()
        {
            return BundleSystemState == EBundleSystemState.Default && !string.IsNullOrWhiteSpace(NewBundleName);
        }

        private void IntializeCommands()
        {
             CreateCommand = new Command(async () => await CreateBundle(), () => GetCreateCommandCanExecute()); 
             CancelCreateCommand = new Command(() => _creatingCancellationTokenSource.Cancel(), () => BundleSystemState == EBundleSystemState.Creating); 
             InstallCommand = new Command(async () => await InstallBundle(), () => BundleSystemState == EBundleSystemState.Default); 
             CancelInstallCommand = new Command(() => _installingCancellationTokenSource.Cancel(), () => BundleSystemState == EBundleSystemState.Installing); 
             ActivateDeactivateCommand = new Command<Bundle>(async (bundle) => await ActivateDeactivateBundle(bundle.Id)); 
             UninstallCommand = new Command<Bundle>(async (bundle) => await UninstallBundle(bundle.Id)); 
        }

        private bool _showCreateProgress;
        public bool ShowCreateProgress
        {
            get => _showCreateProgress;
            set => SetProperty(ref _showCreateProgress, value);
        }

        private bool _showInstallProgress;
        public bool ShowInstallProgress
        {
            get => _showInstallProgress;
            set => SetProperty(ref _showInstallProgress, value);
        }
    }

    public enum EBundleSystemState
    {
        Default = 1,
        Installing = 2,
        Creating = 3
    }
}
