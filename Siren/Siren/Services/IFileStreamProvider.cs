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
        Task CreateFolderIfNotExistsAsync(string folderPath, string folderName);
        Task DeleteFileAsync(string filePath);
        Task DeleteFolderAsync(string folderPath);
        Task<string> ChoosePlaceToSaveFileAsync(string fileName = null);
        
        Task OpenFolder(string folderPath);
        Task<List<string>> ChooseAndCopyToAppData(string prefix = null);
        Task DeleteFromAppData(string fileName);
    }
}
