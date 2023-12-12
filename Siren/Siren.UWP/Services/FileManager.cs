using Microsoft.Win32.SafeHandles;
using Siren.Services;
using Siren.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

[assembly: Dependency(typeof(FileManager))]
namespace Siren.UWP.Services
{
    public class FileManager : IFileManager
    {
        public async Task<string> ChoosePlaceToSaveFileAsync(string fileName = null)
        {
            Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Siren file", new List<string>() { ".siren" });

            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = fileName;

            StorageFile result = await savePicker.PickSaveFileAsync();

            return result != null ? result.Path : null;
        }

        public async Task<string> ChooseAndCopyImageToAppData()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                StorageFolder dstFolder = ApplicationData.Current.LocalFolder;
                StorageFile copy = await file.CopyAsync(dstFolder, file.Name, NameCollisionOption.ReplaceExisting);

                return copy.Path;
            }

            return null;
        }

        public async Task<List<string>> ChooseAndCopySoundsToAppData()
        {
            List<string> copiedFiles = new List<string>();
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

            if (files.Count > 0)
            {
                foreach (StorageFile file in files)
                {
                    StorageFolder dstFolder = ApplicationData.Current.LocalFolder;
                    StorageFile copy = await file.CopyAsync(dstFolder, file.Name, NameCollisionOption.ReplaceExisting);
                    copiedFiles.Add(copy.Path);
                }
            }

            return copiedFiles;
        }

        public async Task CreateFolderIfNotExistsAsync(string folderPath, string folderName)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            await storageFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            IStorageItem item = await storageFolder.TryGetItemAsync(Path.GetFileName(filePath));

            if (item != null)
            {
                await item.DeleteAsync();
            }
        }

        public async Task DeleteFolderAsync(string folderPath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);

            await storageFolder.DeleteAsync();
        }

        public async ValueTask<Stream> GetStreamToReadAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            StorageFile file = await storageFolder.GetFileAsync(Path.GetFileName(filePath));

            return await file.OpenStreamForReadAsync();
        }

        public async ValueTask<Stream> GetStreamToWriteAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            StorageFile file = await storageFolder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.OpenIfExists);

            return await file.OpenStreamForWriteAsync();
        }

        public async Task OpenFolder(string folderPath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            await Launcher.LaunchFolderAsync(storageFolder);
        }

        public async Task RequestFileSystemPermissionAsync()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
        }

        public async Task<bool> TestFileManagerAsync()
        {
            try
            {
                string fileName = "permission_test.txt";
                string fileContent = "Swift as a shadow...";

                //Create
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                StorageFile createFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                //Write
                await FileIO.WriteTextAsync(createFile, fileContent);

                //Read
                StorageFile readFile = await storageFolder.GetFileAsync(fileName);
                string text = await Windows.Storage.FileIO.ReadTextAsync(readFile);
                if (!text.Equals(fileContent))
                {
                    throw new Exception("Test file content is not valid.");
                }

                //Delete
                StorageFile deleteFile = await storageFolder.GetFileAsync(fileName);
                await deleteFile.DeleteAsync();

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task DeleteFromAppData(string fileName)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            await file.DeleteAsync();
        }
    }
}
