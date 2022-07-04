using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    public interface IFileManager
    {
        ValueTask<Stream> GetStreamToReadAsync(string filePath);
        ValueTask<Stream> GetStreamToWriteAsync(string filePath);
        Task CreateFolderIfNotExistsAsync(string folderPath, string folderName);
        Task DeleteFileAsync(string filePath);
        Task DeleteFolderAsync(string folderPath);
        Task<bool> TestFileManagerAsync();
        Task RequestFileSystemPermissionAsync();
        Task<string> ChoosePlaceToSaveFileAsync(string fileName = null);
    }
}
