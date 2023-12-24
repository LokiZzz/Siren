using Siren.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;
using Newtonsoft.Json;
using Siren.Utility;
using System.Threading;

namespace Siren.Services
{
    public interface IBundleService
    {
        Task<bool> SaveBundleAsync(Bundle bundle, string filePath, CancellationToken cancellationToken);
        Task<Bundle> LoadBundleAsync(List<Guid> installedBundles, CancellationToken cancellationToken);
        Task DeleteBundleFilesAsync(Guid bundleId);
        Task<SirenFileMetaData> GetSirenFileMetaData();

        event EventHandler<ProcessingProgress> OnCreateProgressUpdate;
        event EventHandler<ProcessingProgress> OnInstallProgressUpdate;
    }

    public class BundleService : IBundleService
    {
        IFileManager _fileManager;

        const int _metadataFrameSize = 1024 * 1024; //1MB
        string _sirenFilePath = string.Empty;

        public event EventHandler<ProcessingProgress> OnCreateProgressUpdate;
        public event EventHandler<ProcessingProgress> OnInstallProgressUpdate;

        public async Task<bool> SaveBundleAsync(Bundle bundle, string filePath, CancellationToken cancellationToken)
        {
            _fileManager = DependencyService.Resolve<IFileManager>();
            _sirenFilePath = filePath;

            try
            {
                return await CreateBundleAsync(bundle, cancellationToken);
            }
            catch(OperationCanceledException)
            {
                OnCreateProgressUpdate(this, new ProcessingProgress(0, "Creating cancelled..."));
                await _fileManager.DeleteFileOpenedToWrite();

                return false;
            }
        }

        public async Task<Bundle> LoadBundleAsync(List<Guid> installedBundles, CancellationToken cancellationToken)
        {
            _fileManager = DependencyService.Resolve<IFileManager>();

            try
            {
                return await UnpackBundleAsync(installedBundles, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                OnInstallProgressUpdate(this, new ProcessingProgress(0, "Creating cancelled..."));

                return null;
            }
        }

        private async Task<bool> CreateBundleAsync(Bundle bundle, CancellationToken cancellationToken)
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

            Stream targetStream = await _fileManager.PickAndGetStreamToWrite(_sirenFilePath);

            if (targetStream != null)
            {
                await targetStream.WriteAsync(new byte[_metadataFrameSize], 0, _metadataFrameSize);

                foreach (string file in filesToCompress)
                {
                    try
                    {
                        using (Stream sourceStream = await _fileManager.GetStreamToRead(file))
                        {
                            await sourceStream.CopyToAsync(targetStream, 81920, cancellationToken);
                            metadata.Bundle.Size = targetStream.Length;

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
                targetStream.Position = 0;
                byte[] metadataFrame = new byte[_metadataFrameSize];
                byte[] metadataActualBytes = ConvertToByteArray(metadata);
                metadataActualBytes.CopyTo(metadataFrame, 0);
                await targetStream.WriteAsync(metadataFrame, 0, _metadataFrameSize);

                targetStream.Dispose();
                OnCreateProgressUpdate(this, new ProcessingProgress(1, "Complete!"));

                return true;
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

            return false;
        }

        private void UpdateCompressingProgress(object sender, ProgressEventArgs e)
        {
            OnCreateProgressUpdate(this, new ProcessingProgress(
                e.Progress, 
                "Compressing temp file to the *.siren bundle file..."
            ));
        }

        public async Task<Bundle> UnpackBundleAsync(List<Guid> installedBundles, CancellationToken cancellationToken)
        {
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

            Stream sourceStream = await _fileManager.PickAndGetStreamToRead();

            if (sourceStream != null)
            {
                SirenFileMetaData metadata = await GetMetadataModel(sourceStream);

                if(installedBundles.Any(x => x == metadata.Bundle.Id))
                {
                    throw new BundleAlreadyInstalledException();
                }

                await _fileManager.CreateFolderForBundleFiles(metadata.Bundle.Id);

                foreach (CompressedFileInfo file in metadata.CompressedFiles)
                {
                    using (Stream targetStream = await _fileManager.GetStreamToWriteToBundleFolder(metadata.Bundle.Id, file.Name))
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
                }

                //await _fileManager.DeleteFileAsync(SirenTempFile);

                SetFilePathsToLocalAppData(metadata);

                OnInstallProgressUpdate(this, new ProcessingProgress(1, "Complete!"));

                return metadata.Bundle;
            }

            return null;
        }

        public async Task<SirenFileMetaData> GetSirenFileMetaData()
        {
            SirenFileMetaData metadata = null;
            _fileManager = DependencyService.Resolve<IFileManager>();

            using (Stream sourceStream = await _fileManager.PickAndGetStreamToRead())
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

    public class BundleAlreadyInstalledException : Exception { }
}