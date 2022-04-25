﻿using Siren.Services;
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

            Routing.RegisterRoute(nameof(AddOrEditSettingPage), typeof(AddOrEditSettingPage));
            Routing.RegisterRoute(nameof(AddOrEditScenePage), typeof(AddOrEditScenePage));
        }
    }
}