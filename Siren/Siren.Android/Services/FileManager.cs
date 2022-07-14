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
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileManager))]
namespace Siren.Droid.Services
{
    public class FileManager : IFileManager
    {
        public async Task<string> ChoosePlaceToSaveFileAsync(string fileName = null)
        {
            return await Task.FromResult(
                Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), fileName)
            );
        }

        public void ClearAccessList() { }

        public Task CreateFolderIfNotExistsAsync(string folderPath, string folderName)
        {
            return Task.Run(() => 
                Directory.CreateDirectory(Path.Combine(folderPath, folderName))
            );
        }

        public Task DeleteFileAsync(string filePath)
        {
            return Task.Run(() =>
                File.Delete(filePath)
            );
        }

        public Task DeleteFolderAsync(string folderPath)
        {
            return Task.Run(() =>
                Directory.Delete(folderPath)
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

        public async Task RequestFileSystemPermissionAsync()
        {
            await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        public Task<bool> TestFileManagerAsync()
        {
            return Task.Run(() => true);
        }
    }
}