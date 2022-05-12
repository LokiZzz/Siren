using FFImageLoading.Forms;
using Siren.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Siren.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SceneCardView : ContentView
    {
        public SceneCardView()
        {
            InitializeComponent();
        }

        private void ContentView_LayoutChanged(object sender, EventArgs e)
        {

        }
    }
}