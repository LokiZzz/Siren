using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Siren.Services
{
    public interface IFileManager
    {
        ValueTask<Stream> GetStreamToReadAsync(string filePath);
        ValueTask<Stream> GetStreamToWriteAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        Task DeleteFolderAsync(string folderPath);
        Task<string> ChoosePlaceToSaveFileAsync(string fileName = null);

        Task CreateFolderForBundleFiles(Guid bundleId);
        Task OpenFolder(string folderPath);
        Task<List<string>> ChooseAndCopySoundsToAppData();
        Task<string> ChooseAndCopyImageToAppData();
        Task DeleteFromAppData(string fileName);
        ValueTask<Stream> PickFolderAndGetStreamToWrite(string suggestedFileName);
        ValueTask<Stream> GetStreamToReadFromAppDataAsync(string fileName);
        ValueTask<Stream> PickAndGetStreamToRead();
        ValueTask<Stream> GetStreamToWriteFileIntoBundleFolder(Guid bundleId, string fileName);
    }
}
