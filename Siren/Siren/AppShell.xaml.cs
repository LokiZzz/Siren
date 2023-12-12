using Siren.Services;
using Siren.ViewModels;
using Siren.Views;
using Siren.Views.Help;
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

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AddOrEditComponentPage), typeof(AddOrEditComponentPage));
            Routing.RegisterRoute(nameof(BundlePage), typeof(BundlePage));
            Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
        }
    }
}
