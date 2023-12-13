using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Siren.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThanksPage : ContentPage
    {
        public ThanksPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Browser.OpenAsync(
                "https://pay.cloudtips.ru/p/a00a6a33", 
                BrowserLaunchMode.SystemPreferred
            );
        }
    }
}