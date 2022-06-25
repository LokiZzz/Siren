using Siren.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Siren.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckPermissionPage : ContentPage
    {
        public CheckPermissionPage()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}