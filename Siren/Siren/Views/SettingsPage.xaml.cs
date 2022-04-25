using Siren.ViewModels;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Siren.Views
{
    public partial class SettingsPage : ContentPage
    {
        SettingsViewModel _viewModel;

        public SettingsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new SettingsViewModel();
        }
    }
}