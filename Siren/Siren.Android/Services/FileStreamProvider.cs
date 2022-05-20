using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Siren.Droid.Services;
using Siren.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileStreamProvider))]
namespace Siren.Droid.Services
{
    public class FileStreamProvider : IFileStreamProvider
    {
        public Stream GetFileStreamToRead(string filePath)
        {
            return new FileStream(filePath, FileMode.Open);
        }

        public Stream GetFileStreamToWrite(string filePath)
        {
            return new FileStream(filePath, FileMode.Create);
        }
    }
}