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
        ValueTask<Stream> GetStreamToRead(string filePath);
        ValueTask<Stream> PickAndGetStreamToRead();
        ValueTask<Stream> GetStreamToReadFromAppData(string fileName);

        ValueTask<Stream> GetStreamToWrite(string filePath);
        ValueTask<Stream> PickAndGetStreamToWrite(string suggestedFileName);
        ValueTask<Stream> GetStreamToWriteToBundleFolder(Guid bundleId, string fileName);

        Task DeleteFileAsync(string filePath);
        Task DeleteFolderAsync(string folderPath);
        Task DeleteFromAppData(string fileName);

        Task OpenFolder(string folderPath);

        Task CreateFolderForBundleFiles(Guid bundleId);

        Task<List<string>> ChooseAndCopySoundsToAppData();

        Task<string> ChooseAndCopyImageToAppData();
    }
}
