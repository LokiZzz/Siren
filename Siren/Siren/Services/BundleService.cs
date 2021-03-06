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
using static System.Net.WebRequestMethods;
using System.Diagnostics;
using System.Threading;

namespace Siren.Services
{
    public interface IBundleService
    {
        Task SaveBundleAsync(Bundle bundle, string filePath, CancellationToken cancellationToken);
        Task<Bundle> LoadBundleAsync(string filePath, CancellationToken cancellationToken);
        Task DeleteBundleFilesAsync(Guid bundleId);
        Task<SirenFileMetaData> GetSirenFileMetaData(string filePath);

        event EventHandler<ProcessingProgress> OnCreateProgressUpdate;
        event EventHandler<ProcessingProgress> OnInstallProgressUpdate;
    }

    public class BundleService : IBundleService
    {
        IFileManager _fileManager;

        const int _metadataFrameSize = 1024 * 1024; //1MB
        string _sirenFilePath = string.Empty;

        //string SirenTempFile => $"{_sirenFilePath}.temp";

        public event EventHandler<ProcessingProgress> OnCreateProgressUpdate;
        public event EventHandler<ProcessingProgress> OnInstallProgressUpdate;

        public async Task SaveBundleAsync(Bundle bundle, string filePath, CancellationToken cancellationToken)
        {
            _fileManager = DependencyService.Resolve<IFileManager>();
            _sirenFilePath = filePath;

            try
            {
                await CreateBundleAsync(bundle, cancellationToken);
            }
            catch(OperationCanceledException)
            {
                OnCreateProgressUpdate(this, new ProcessingProgress(0, "Creating cancelled..."));
                await _fileManager.DeleteFileAsync(_sirenFilePath);
            }
        }

        public async Task<Bundle> LoadBundleAsync(string filePath, CancellationToken cancellationToken)
        {
            _fileManager = DependencyService.Resolve<IFileManager>();
            _sirenFilePath = filePath;

            try
            {
                return await UnpackBundleAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                OnInstallProgressUpdate(this, new ProcessingProgress(0, "Creating cancelled..."));

                return null;
            }
        }

        public async Task CreateBundleAsync(Bundle bundle, CancellationToken cancellationToken)
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

            using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(_sirenFilePath))
            {
                await targetStream.WriteAsync(new byte[_metadataFrameSize], 0, _metadataFrameSize);
            }

            foreach (string file in filesToCompress)
            {
                try
                {
                    using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(file))
                    {
                        using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(_sirenFilePath))
                        {
                            targetStream.Position = targetStream.Length;
                            await sourceStream.CopyToAsync(targetStream, 81920, cancellationToken);
                            metadata.Bundle.Size = targetStream.Length;
                        }

                        metadata.CompressedFiles.Add(new CompressedFileInfo
                        {
                            Name = Path.GetFileName(file),
                            SizeBytesBefore = sourceStream.Length,
                        });
                    }

                    double progress = (double)(filesToCompress.IndexOf(file) + 1) / (double)filesToCompress.Count();
                    OnCreateProgressUpdate(this, new ProcessingProgress(progress, "Fusing files together..."));
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            //3. Add metadata to the created gap
            using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(_sirenFilePath))
            {
                byte[] metadataFrame = new byte[_metadataFrameSize];
                byte[] metadataActualBytes = ConvertToByteArray(metadata);
                metadataActualBytes.CopyTo(metadataFrame, 0);
                await targetStream.WriteAsync(metadataFrame, 0, _metadataFrameSize);
            }

            //4. Create a compressed bundle (*.siren) file
            //using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(_sirenFilePath))
            //{
            //    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionLevel.Optimal))
            //    {
            //        using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(SirenTempFile))
            //        {
            //            using (ProgressStream progress = new ProgressStream(sourceStream))
            //            {
            //                progress.UpdateProgress += UpdateCompressingProgress;
            //                await progress.CopyToAsync(compressionStream);
            //            }
            //        }
            //    }
            //}

            //5. Delete temp file
            //await _fileManager.DeleteFileAsync(SirenTempFile);

            OnCreateProgressUpdate(this, new ProcessingProgress(1, "Complete!"));
        }

        private void UpdateCompressingProgress(object sender, ProgressEventArgs e)
        {
            OnCreateProgressUpdate(this, new ProcessingProgress(
                e.Progress, 
                "Compressing temp file to the *.siren bundle file..."
            ));
        }

        public async Task<Bundle> UnpackBundleAsync(CancellationToken cancellationToken)
        {
            SirenFileMetaData metadata = null;

            //using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(_sirenFilePath))
            //{
            //    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
            //    {
            //        using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(_sirenFilePath))
            //        {
            //            await decompressionStream.CopyToAsync(targetStream);
            //        }
            //    }
            //}

            using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(_sirenFilePath))
            {
                metadata = await GetMetadataModel(sourceStream);
            }

            await _fileManager.CreateFolderIfNotExistsAsync(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                metadata.Bundle.Id.ToString()
            );

            long offset = _metadataFrameSize;

            foreach (CompressedFileInfo file in metadata.CompressedFiles)
            {
                using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(_sirenFilePath))
                {
                    sourceStream.Position = offset;
                    string path = GetLocalAppDataBundleFilePath(file.Name, metadata.Bundle.Id);

                    using (Stream targetStream = await _fileManager.GetStreamToWriteAsync(path))
                    {
                        try
                        {
                            byte[] buffer = new byte[file.SizeBytesBefore];
                            await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                            await targetStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                        }
                        catch (OperationCanceledException ex)
                        {
                            string folderToDelete = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                metadata.Bundle.Id.ToString()
                            );
                            await _fileManager.DeleteFolderAsync(folderToDelete);

                            throw ex;
                        }
                    }

                    double progress = (double)(metadata.CompressedFiles.IndexOf(file) + 1) / (double)metadata.CompressedFiles.Count();
                    OnInstallProgressUpdate(this, new ProcessingProgress(progress, "Unpacking elements..."));

                    offset = sourceStream.Position;
                }
            }

            //await _fileManager.DeleteFileAsync(SirenTempFile);

            SetFilePathsToLocalAppData(metadata);

            OnInstallProgressUpdate(this, new ProcessingProgress(1, "Complete!"));

            return metadata.Bundle;
        }

        public async Task<SirenFileMetaData> GetSirenFileMetaData(string path)
        {
            SirenFileMetaData metadata = null;
            _fileManager = DependencyService.Resolve<IFileManager>();

            using (Stream sourceStream = await _fileManager.GetStreamToReadAsync(path))
            {
                metadata = await GetMetadataModel(sourceStream);
            }

            return metadata;
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

                setting.Music.ForEach(musicTrack => {
                    musicTrack.FilePath = GetLocalAppDataBundleFilePath(Path.GetFileName(musicTrack.FilePath), bundleId);
                });
            }
        }

        private string GetLocalAppDataBundleFilePath(string fileName, Guid bundleId)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    bundleId.ToString(),
                    fileName
                );
            }
            else
            {
                return null;
            }
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
            IEnumerable<string> music = bundle.Settings.SelectMany(x => x.Music).Select(x => x.FilePath);
            IEnumerable<string> settingsImages = bundle.Settings.Select(x => x.ImagePath)
                .Where(x => !string.IsNullOrEmpty(x));
            IEnumerable<string> scenesImages = bundle.Settings.SelectMany(x => x.Scenes).Select(x => x.ImagePath)
                .Where(x => !string.IsNullOrEmpty(x)); ;

            return elements.Union(effects).Union(music).Union(settingsImages).Union(scenesImages).ToList();
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

    public class ProcessingProgress
    {
        public ProcessingProgress(double progress, string message)
        {
            Progress = progress;
            Message = message;
        }

        public double Progress { get; set; }
        public string Message { get; set; }
    }
}