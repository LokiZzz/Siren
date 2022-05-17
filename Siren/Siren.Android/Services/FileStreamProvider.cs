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
        public Task<Stream> GetFileStreamToRead(string filePath)
        {
            return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open));
        }

        public Task<Stream> GetFileStreamToWrite(string filePath)
        {
            return Task.FromResult<Stream>(File.Create(filePath));
        }
    }
}