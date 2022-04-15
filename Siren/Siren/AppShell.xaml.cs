using Siren.Services;
using Siren.ViewModels;
using Siren.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Siren
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            //Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));

            Routing.RegisterRoute(nameof(AddOrEditSettingPage), typeof(AddOrEditSettingPage));
            DependencyService.RegisterSingleton(new SceneManager());
        }
    }
}
