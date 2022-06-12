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

[assembly: Dependency(typeof(FileManager))]
namespace Siren.Droid.Services
{
    public class FileManager : IFileManager
    {
        public Task CreateFolderIfNotExists(string folderPath, string folderName)
        {
            return Task.Run(() => 
                Directory.CreateDirectory(Path.Combine(folderPath, folderName))
            );
        }

        public async ValueTask<Stream> GetStreamToReadAsync(string filePath)
        {
            return await Task.FromResult(new FileStream(filePath, FileMode.Open));
        }

        public async ValueTask<Stream> GetStreamToWriteAsync(string filePath)
        {
            return await Task.FromResult(new FileStream(filePath, FileMode.Create));
        }
    }
}