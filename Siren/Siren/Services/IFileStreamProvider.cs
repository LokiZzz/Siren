using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    public interface IFileStreamProvider
    {
        Stream GetFileStreamToRead(string filePath);
        Stream GetFileStreamToWrite(string filePath);
    }
}
