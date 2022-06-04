using Siren.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;
using Newtonsoft.Json;
using Microsoft.Win32.SafeHandles;
using static System.Net.WebRequestMethods;

namespace Siren.Services
{
    public interface IBundleService
    {
        Task SaveBundleAsync(Bundle bundle, string filePath);
        Task<Bundle> LoadBundleAsync(string filePath);
        Task Test();
    }

    public class BundleService : IBundleService
    {
        public async Task Test()
        {
            _fileStreamProvider = DependencyService.Resolve<IFileStreamProvider>();

            string source = @"F:\Temp\hxeo3tbudtk61.jpg";
            string targetCompressed = @"F:\Temp\hxeo3tbudtk61.gzip";
            string targetDecompressed = @"F:\Temp\hxeo3tbudtk61_Uncompressed.jpg";

            //Compress

            using (Stream fsTarget = _fileStreamProvider.GetStreamToWrite(targetCompressed)) 
            {
                using (GZipStream gzsCompress = new GZipStream(fsTarget, CompressionMode.Compress))
                {
                    using (Stream fsSource = _fileStreamProvider.GetStreamToRead(source))
                    {
                        fsSource.CopyTo(gzsCompress);
                    }
                }
            }


            //Decompress

            using (Stream fsSource = _fileStreamProvider.GetStreamToRead(targetCompressed))
            {
                using (GZipStream gzsCompress = new GZipStream(fsSource, CompressionMode.Decompress))
                {
                    using (Stream fsTarget = _fileStreamProvider.GetStreamToWrite(targetDecompressed))
                    {
                        gzsCompress.CopyTo(fsTarget);
                    }
                }
            }
        }

        IFileStreamProvider _fileStreamProvider;

        const int _metadataFrameSize = 128 * 1024; //128kB
        string _sirenFilePath = string.Empty;

        public async Task SaveBundleAsync(Bundle bundle, string filePath)
        {
            _fileStreamProvider = DependencyService.Resolve<IFileStreamProvider>();
            _sirenFilePath = filePath;

            await CreateBundleAsync(bundle);
        }

        public async Task<Bundle> LoadBundleAsync(string filePath)
        {
            _fileStreamProvider = DependencyService.Resolve<IFileStreamProvider>();
            _sirenFilePath = filePath;

            return await UnpackBundleAsync();
        }

        public async Task CreateBundleAsync(Bundle bundle)
        {
            SirenFileMetaData metadata = new SirenFileMetaData
            {
                Bundle = bundle,
                CompressedFiles = new List<CompressedFileInfo>()
            };
            List<string> filesToCompress = GetAllFileFromBundle(bundle);

            //Create a gap for metadata frame and write compressed data;
            using (Stream targetStream = await _fileStreamProvider.GetStreamToWriteAsync(_sirenFilePath))
            {
                await targetStream.WriteAsync(new byte[_metadataFrameSize], 0, _metadataFrameSize); //Create a gap
                targetStream.Position = _metadataFrameSize; //Maybe delete this

                using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                {
                    foreach (string file in filesToCompress)
                    {
                        using (Stream sourceStream = await _fileStreamProvider.GetStreamToReadAsync(file))
                        {
                            await sourceStream.CopyToAsync(compressionStream);

                            metadata.CompressedFiles.Add(new CompressedFileInfo
                            {
                                Name = Path.GetFileName(file),
                                CompressedSizeBytes = sourceStream.Length,
                            });
                        }
                    }
                }
            }

            //Write the metadata into frame to the begining of file;
            using (FileStream targetStream = new FileStream(_sirenFilePath, FileMode.Open))
            {
                targetStream.Position = 0; //Maybe delete this
                byte[] metadataFrame = new byte[_metadataFrameSize];
                byte[] metadataActualBytes = ConvertToByteArray(metadata);
                metadataActualBytes.CopyTo(metadataFrame, 0);
                await targetStream.WriteAsync(metadataFrame, 0, _metadataFrameSize);
            }
        }

        public async Task<Bundle> UnpackBundleAsync()
        {
            //Read metadata file and decompress data;
            using (FileStream sourceStream = new FileStream(_sirenFilePath, FileMode.Open))
            {
                byte[] metadataFrame = new byte[_metadataFrameSize];
                await sourceStream.ReadAsync(metadataFrame, 0, _metadataFrameSize);
                SirenFileMetaData metadata = ConvertToObject<SirenFileMetaData>(metadataFrame);
                sourceStream.Position = _metadataFrameSize;

                using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                {
                    foreach (CompressedFileInfo file in metadata.CompressedFiles)
                    {
                        using (FileStream targetStream = new FileStream(GetDecompressedFilePath(file, metadata.Bundle), FileMode.Create))
                        {
                            byte[] singleFileChunk = new byte[file.CompressedSizeBytes];
                            await decompressionStream.ReadAsync(singleFileChunk, 0, singleFileChunk.Length);
                            await targetStream.WriteAsync(singleFileChunk, 0, singleFileChunk.Length);
                        }
                    }
                }

                return metadata.Bundle;
            }
        }

        private string GetDecompressedFilePath(CompressedFileInfo file, Bundle bundle)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                bundle.Name,
                file.Name
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

        private List<string> GetAllFileFromBundle(Bundle bundle)
        {
            IEnumerable<string> elements = bundle.Settings.SelectMany(x => x.Elements).Select(x => x.FilePath);
            IEnumerable<string> effects = bundle.Settings.SelectMany(x => x.Effects).Select(x => x.FilePath);
            IEnumerable<string> settingsImages = bundle.Settings.Select(x => x.ImagePath);
            IEnumerable<string> scenesImages = bundle.Settings.SelectMany(x => x.Scenes).Select(x => x.ImagePath);

            return elements.Union(effects).Union(settingsImages).Union(scenesImages).ToList();
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
        public long CompressedSizeBytes { get; set; }
    }
}
