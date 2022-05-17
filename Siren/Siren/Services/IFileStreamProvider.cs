using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    public interface IFileStreamProvider
    {
        Task<Stream> GetFileStreamToRead(string filePath);
        Task<Stream> GetFileStreamToWrite(string filePath);
    }
}
