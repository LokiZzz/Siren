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
            CheckCommand = new Command(async () => await TryCheck());
            //Task.Run(async () => await Check());
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Check();
            });
        }

        public Command CheckCommand { get; set; }

        private async Task TryCheck()
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

        private async Task Check()
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
