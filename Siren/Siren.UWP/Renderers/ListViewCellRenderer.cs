using Siren.UWP;

[assembly: Xamarin.Forms.Platform.UWP.ExportRenderer(
    typeof(DLToolkit.Forms.Controls.FlowListView), 
    typeof(CustomListViewRenderer)
)]

namespace Siren.UWP
{
    using System;
    using Windows.UI.Xaml.Input;
    using Xamarin.Forms.Platform.UWP;

    public class CustomListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            if(List != null)
            {
                List.IsItemClickEnabled = false;
            }
        }
    }
}