using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    public interface IFileStreamProvider
    {
        FileStream GetFileStreamToRead(string filePath);
        FileStream GetFileStreamToWrite(string filePath);
        Stream GetStreamToRead(string filePath);
        Stream GetStreamToWrite(string filePath);
    }
}
