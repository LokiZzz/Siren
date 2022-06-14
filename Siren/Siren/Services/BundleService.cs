using Siren.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;
using Newtonsoft.Json;
using Siren.Utility;

namespace Siren.Services
{
    public interface IBundleService
    {
        Task SaveBundleAsync(Bundle bundle, string filePath);
        Task<Bundle> LoadBundleAsync(string filePath);
    }

    public class BundleService : IBundleService
    {
        IFileManager _fileManager;

        const int _metadataFrameSize = 128 * 1024; //128kB
        string _sirenFilePath = string.Empty;

        string SirenTempFile => $"{_sirenFilePath}.temp";

        public async Task SaveBundleAsync(Bundle bundle, string filePath)
        {
            _fileManager = DependencyService.Resolve<IFileManager>();
            _sirenFilePath = filePath;

            await CreateBundleAsync(bundle);
        }

        public async Task<Bundle> LoadBundleAsync(string filePath)
        {
            _fileManager = DependencyService.Resolve<IFileManager>();
            _sirenFilePath = filePath;

            return await UnpackBundleAsync();
        }

        public async Task CreateBundleAsync(Bundle bundle)
        {
            //1. Create bundle model for metadata
            Bundle bundleCopy = bundle.GetDeepCopy();
            SirenFileMetaData metadata = new SirenFileMetaData
            {
                Bundle = bundleCopy,
                CompressedFiles = new List<CompressedFileInfo>()
            };
            List<string> filesToCompress = GetAllFilesFromBundle(bundleCopy);

            //2. Create a big melted file chunk to give it to the compressor (creating a *.siren.temp)

            //2.1 Create a metadata frame gap
            using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(SirenTempFile))
            {
                byte[] metadataFrame = new byte[_metadataFrameSize];
                byte[] metadataActualBytes = ConvertToByteArray(metadata);
                metadataActualBytes.CopyTo(metadataFrame, 0);
                await targetStream.WriteAsync(metadataFrame, 0, _metadataFrameSize);
            }

            //2.2 Add actual files 
            foreach (string file in filesToCompress)
            {
                using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(file))
                {
                    using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(SirenTempFile))
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }

                    metadata.CompressedFiles.Add(new CompressedFileInfo
                    {
                        Name = Path.GetFileName(file),
                        SizeBytesBefore = sourceStream.Length,
                    });
                }
            }

            //3. Create a compressed bundle (*.siren) file
            using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(_sirenFilePath))
            {
                using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                {
                    using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(SirenTempFile))
                    {
                        await sourceStream.CopyToAsync(compressionStream);
                    }
                }
            }

            //4. Delete temp file
            await _fileManager.DeleteFileAsync(SirenTempFile);
        }

        //public async Task CreateBundleAsync(Bundle bundle)
        //{
        //    Bundle bundleCopy = bundle.GetDeepCopy();
        //    SirenFileMetaData metadata = new SirenFileMetaData
        //    {
        //        Bundle = bundleCopy,
        //        CompressedFiles = new List<CompressedFileInfo>()
        //    };
        //    List<string> filesToCompress = GetAllFilesFromBundle(bundleCopy);

        //    //Create a gap for metadata frame and write compressed data;
        //    using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(_sirenFilePath))
        //    {
        //        await targetStream.WriteAsync(new byte[_metadataFrameSize], 0, _metadataFrameSize); //Create a gap

        //        using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
        //        {
        //            foreach (string file in filesToCompress)
        //            {
        //                using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(file))
        //                {
        //                    long sizeBefore = targetStream.Length;
        //                    await sourceStream.CopyToAsync(compressionStream);
        //                    long sizeAfter = targetStream.Length;

        //                    metadata.CompressedFiles.Add(new CompressedFileInfo
        //                    {
        //                        Name = Path.GetFileName(file),
        //                        SizeBytesBefore = sourceStream.Length,
        //                        SizeBytesAfter = sizeAfter - sizeBefore,
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    //Write the metadata into frame to the begining of file;
        //    using (FileStream targetStream = new FileStream(_sirenFilePath, FileMode.Open))
        //    {
        //        targetStream.Position = 0; //Maybe delete this
        //        byte[] metadataFrame = new byte[_metadataFrameSize];
        //        byte[] metadataActualBytes = ConvertToByteArray(metadata);
        //        metadataActualBytes.CopyTo(metadataFrame, 0);
        //        await targetStream.WriteAsync(metadataFrame, 0, _metadataFrameSize);
        //    }
        //}

        public async Task<Bundle> UnpackBundleAsync()
        {
            SirenFileMetaData metadata = null;

            //1. Unpack big melted chunk (*.siren.temp)
            using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(_sirenFilePath))
            {
                using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
{
                    using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(SirenTempFile))
                    {
                        await decompressionStream.CopyToAsync(targetStream);
                    }
                }
            }

            //2. Divide big chank to separate files and put it to the Bundle folder at LocalApp directory
            using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(SirenTempFile))
            {
                //2.1 Get the metadata from metadata frame
                metadata = await GetMetadataFromSirenFile(sourceStream);

                //2.2 Divide file
                foreach (CompressedFileInfo file in metadata.CompressedFiles)
                {
                    string path = GetLocalAppDataBundleFilePath(file.Name, metadata.Bundle.Name);

                    using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(path))
                    {
                        byte[] buffer = new byte[file.SizeBytesBefore];
                        await sourceStream.ReadAsync(buffer, 0, buffer.Length);
                        await targetStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            }

            //3. Delete temp file
            await _fileManager.DeleteFileAsync(SirenTempFile);

            //4. Bind bundle to brand new unpacked files
            SetFilePathsToLocalAppData(metadata);

            return metadata.Bundle;
        }

        private async Task<SirenFileMetaData> GetMetadataFromSirenFile(Stream sourceStream)
        {
            byte[] metadataFrame = new byte[_metadataFrameSize];
            await sourceStream.ReadAsync(metadataFrame, 0, _metadataFrameSize);
            SirenFileMetaData metadata = ConvertToObject<SirenFileMetaData>(metadataFrame);
            metadata.Bundle.Name = string.IsNullOrEmpty(metadata.Bundle.Name)
                ? Path.GetFileNameWithoutExtension(_sirenFilePath)
                : metadata.Bundle.Name;

            return metadata;
        }

        //public async Task<Bundle> UnpackBundleAsync()
        //{
        //    //Read metadata file and decompress data;
        //    using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(_sirenFilePath))
        //    {
        //        byte[] metadataFrame = new byte[_metadataFrameSize];
        //        await sourceStream.ReadAsync(metadataFrame, 0, _metadataFrameSize);
        //        SirenFileMetaData metadata = ConvertToObject<SirenFileMetaData>(metadataFrame);
        //        metadata.Bundle.Name = string.IsNullOrEmpty(metadata.Bundle.Name)
        //            ? Path.GetFileNameWithoutExtension(_sirenFilePath)
        //            : metadata.Bundle.Name;
        //        sourceStream.Position = _metadataFrameSize;
        //        await _fileManager.CreateFolderIfNotExists(
        //            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //            metadata.Bundle.Name
        //        );

        //        byte[] firstFileCompressed = await GetBytes(sourceStream, _metadataFrameSize, 5);

        //        using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
        //        {
        //            foreach (CompressedFileInfo file in metadata.CompressedFiles)
        //            {
        //                string path = GetLocalAppDataBundleFilePath(file.Name, metadata.Bundle.Name);

        //                using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(path))
        //                {
        //                    byte[] singleFileChunk = new byte[file.SizeBytes];
        //                    await decompressionStream.ReadAsync(singleFileChunk, 0, singleFileChunk.Length);
        //                    await targetStream.WriteAsync(singleFileChunk, 0, singleFileChunk.Length);

        //                    //Посмотреть хвост файла!!!
        //                    byte[] fullFile = await GetBytes(targetStream, 0, (int)sourceStream.Length);
        //                    string fileProba = await GetProba(targetStream, 0, 10);
        //                    string fileProba2 = await GetProba(targetStream, (int)(sourceStream.Length - 10), 10);
        //                }
        //            }
        //        }

        //        //Bind bundle to brand new unpacked files
        //        SetFilePathsToLocalAppData(metadata);

        //        return metadata.Bundle;
        //    }
        //}

        private void SetFilePathsToLocalAppData(SirenFileMetaData metadata)
        {
            string bundleName = metadata.Bundle.Name;

            foreach (Setting setting in metadata.Bundle.Settings)
            {
                setting.ImagePath = GetLocalAppDataBundleFilePath(Path.GetFileName(setting.ImagePath), bundleName);

                foreach (Scene scene in setting.Scenes)
                {
                    scene.ImagePath = GetLocalAppDataBundleFilePath(Path.GetFileName(scene.ImagePath), bundleName);
                    scene.ElementsSetup.ForEach(element => {
                        element.FilePath = GetLocalAppDataBundleFilePath(Path.GetFileName(setting.ImagePath), bundleName);
                    });
                }

                setting.Elements.ForEach(element => {
                    element.FilePath = GetLocalAppDataBundleFilePath(Path.GetFileName(setting.ImagePath), bundleName);
                });

                setting.Effects.ForEach(effect => {
                    effect.FilePath = GetLocalAppDataBundleFilePath(Path.GetFileName(setting.ImagePath), bundleName);
                });
            }
        }

        private string GetLocalAppDataBundleFilePath(string fileName, string bundleName)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                bundleName,
                fileName
            );
        }

        private byte[] ConvertToByteArray<T>(T objectToSerialize)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToSerialize));
        }

        private T ConvertToObject<T>(byte[] bytesToDeserialize)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytesToDeserialize));
        }

        private List<string> GetAllFilesFromBundle(Bundle bundle)
        {
            IEnumerable<string> elements = bundle.Settings.SelectMany(x => x.Elements).Select(x => x.FilePath);
            IEnumerable<string> effects = bundle.Settings.SelectMany(x => x.Effects).Select(x => x.FilePath);
            IEnumerable<string> settingsImages = bundle.Settings.Select(x => x.ImagePath)
                .Where(x => !string.IsNullOrEmpty(x));
            IEnumerable<string> scenesImages = bundle.Settings.SelectMany(x => x.Scenes).Select(x => x.ImagePath)
                .Where(x => !string.IsNullOrEmpty(x)); ;

            return elements.Union(effects).Union(settingsImages).Union(scenesImages).ToList();
        }

        private async Task<string> GetProba(Stream stream, int from, int size)
        {
            byte[] bytes = await GetBytes(stream, from, size);
            string output = string.Empty;

            foreach(byte b in bytes)
            {
                output += b.ToString() + "\t";
            }

            return output;
        }

        private async Task<byte[]> GetBytes(Stream stream, int from, int size)
        {
            long initialPosition = stream.Position;

            byte[] output = new byte[size];
            stream.Position = from;
            await stream.ReadAsync(output, 0, size);

            stream.Position = initialPosition;

            return output;
        }
    }

    public class SirenFileMetaData
    {
        public Bundle Bundle { get; set; }
        public List<CompressedFileInfo> CompressedFiles { get; set; }
    }

    public class CompressedFileInfo
    {
        public string Name { get; set; }
        public long SizeBytesBefore { get; set; }
        //public long SizeBytesAfter { get; set; }
    }
}