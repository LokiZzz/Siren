﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    public interface IFileStreamProvider
    {
        ValueTask<Stream> GetStreamToReadAsync(string filePath);
        ValueTask<Stream> GetStreamToWriteAsync(string filePath);
        Task CreateFolderIfNotExists(string folderPath, string folderName);
    }
}
