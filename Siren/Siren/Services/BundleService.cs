﻿using Siren.Models;
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
        Task DeleteBundleFilesAsync(Guid bundleId);
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
            bundleCopy.Id = Guid.NewGuid();
            bundleCopy.Settings.ForEach(x => x.BundleId = bundleCopy.Id);
            SirenFileMetaData metadata = new SirenFileMetaData
            {
                Bundle = bundleCopy,
                CompressedFiles = new List<CompressedFileInfo>()
            };
            List<string> filesToCompress = GetAllFilesFromBundle(bundleCopy);

            //2. Create a big fused file chunk to give it to the compressor (creating a *.siren.temp)
            foreach (string file in filesToCompress)
            {
                //2.1 Create a gap for metadata frame
                using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(SirenTempFile))
                {
                    await targetStream.WriteAsync(new byte[_metadataFrameSize], 0, _metadataFrameSize);
                }

                //2.2 Write actual files
                using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(file))
                {
                    using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(SirenTempFile))
                    {
                        targetStream.Position = targetStream.Length;
                        await sourceStream.CopyToAsync(targetStream);
                    }

                    metadata.CompressedFiles.Add(new CompressedFileInfo
                    {
                        Name = Path.GetFileName(file),
                        SizeBytesBefore = sourceStream.Length,
                    });
                }
            }

            //3. Add metadata to the created gap
            using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(SirenTempFile))
            {
                byte[] metadataFrame = new byte[_metadataFrameSize];
                byte[] metadataActualBytes = ConvertToByteArray(metadata);
                metadataActualBytes.CopyTo(metadataFrame, 0);
                await targetStream.WriteAsync(metadataFrame, 0, _metadataFrameSize);
            }

            //4. Create a compressed bundle (*.siren) file
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

            //5. Delete temp file
            await _fileManager.DeleteFileAsync(SirenTempFile);
        }

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
                metadata = await GetMetadataModel(sourceStream);

                //2.2 Create bundle folder if it is not exists
                await _fileManager.CreateFolderIfNotExistsAsync(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    metadata.Bundle.Id.ToString()
                );

                //2.3 Divide file
                foreach (CompressedFileInfo file in metadata.CompressedFiles)
                {
                    string path = GetLocalAppDataBundleFilePath(file.Name, metadata.Bundle.Id);

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

        public async Task DeleteBundleFilesAsync(Guid bundleId)
        {
            _fileManager = DependencyService.Resolve<IFileManager>();
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                bundleId.ToString()
            );

            await _fileManager.DeleteFolderAsync(path);
        }

        private async Task<SirenFileMetaData> GetMetadataModel(Stream sourceStream)
        {
            byte[] metadataFrame = new byte[_metadataFrameSize];
            await sourceStream.ReadAsync(metadataFrame, 0, _metadataFrameSize);
            SirenFileMetaData metadata = ConvertToObject<SirenFileMetaData>(metadataFrame);
            metadata.Bundle.Name = string.IsNullOrEmpty(metadata.Bundle.Name)
                ? Path.GetFileNameWithoutExtension(_sirenFilePath)
                : metadata.Bundle.Name;

            return metadata;
        }

        private void SetFilePathsToLocalAppData(SirenFileMetaData metadata)
        {
            Guid bundleId = metadata.Bundle.Id;

            foreach (Setting setting in metadata.Bundle.Settings)
            {
                setting.ImagePath = GetLocalAppDataBundleFilePath(Path.GetFileName(setting.ImagePath), bundleId);

                foreach (Scene scene in setting.Scenes)
                {
                    scene.ImagePath = GetLocalAppDataBundleFilePath(Path.GetFileName(scene.ImagePath), bundleId);
                    scene.ElementsSetup.ForEach(element => {
                        element.FilePath = GetLocalAppDataBundleFilePath(Path.GetFileName(element.FilePath), bundleId);
                    });
                }

                setting.Elements.ForEach(element => {
                    element.FilePath = GetLocalAppDataBundleFilePath(Path.GetFileName(element.FilePath), bundleId);
                });

                setting.Effects.ForEach(effect => {
                    effect.FilePath = GetLocalAppDataBundleFilePath(Path.GetFileName(effect.FilePath), bundleId);
                });
            }
        }

        private string GetLocalAppDataBundleFilePath(string fileName, Guid bundleId)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                bundleId.ToString(),
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
    }
}