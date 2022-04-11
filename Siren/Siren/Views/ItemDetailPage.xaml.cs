using Siren.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Siren.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}