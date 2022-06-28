using Siren.Services;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class CheckPermissionViewModel : BaseViewModel
    {
        public CheckPermissionViewModel()
        {
            RequestPermissionCommand = new Command(async () => await Request());
            RefreshCommand = new Command(async () => await Refresh());

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Refresh();
            });
        }

        public Command RequestPermissionCommand { get; set; }
        public Command RefreshCommand { get; set; }

        private async Task Refresh()
        {
            IsBusy = true;

            IFileManager fileManager = DependencyService.Resolve<IFileManager>();
            bool allIsFine = await fileManager.TestFileManagerAsync();

            if (allIsFine)
            {
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }

            ShowMessage = !allIsFine;

            IsBusy = false;
        }

        private async Task Request()
        {
            IFileManager fileManager = DependencyService.Resolve<IFileManager>();

            await fileManager.RequestFileSystemPermissionAsync();
        }

        private bool _showMessage = false;
        public bool ShowMessage
        {
            get => _showMessage;
            set => SetProperty(ref _showMessage, value);
        }

        private bool _isRUS = false;
        public bool IsRUS
        {
            get => _isRUS;
            set => SetProperty(ref _isRUS, value);
        }

    }
}
