using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siren.Droid.Services
{
    public class ActivityResultMessage
    {
        public static string Key = "arm";

        public int RequestCode { get; set; }

        public object ResultCode { get; set; }

        public object Data { get; set; }
    }
}