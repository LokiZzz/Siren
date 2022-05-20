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
        public FileStream GetFileStreamToRead(string filePath)
        {
            return new FileStream(filePath, FileMode.Open);
        }

        public FileStream GetFileStreamToWrite(string filePath)
        {
            return new FileStream(filePath, FileMode.Create);
        }

        public Stream GetStreamToRead(string filePath)
        {
            throw new NotImplementedException();
        }

        public Stream GetStreamToWrite(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}