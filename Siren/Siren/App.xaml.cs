using Siren.Services;
using Siren.Views;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Siren
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            RegisterServices();

            MainPage = new AppShell();
        }

        private static void RegisterServices()
        {
            DependencyService.RegisterSingleton(new SceneManager());
            DependencyService.RegisterSingleton<IBundleService>(new BundleService());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
