using Siren.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Siren.Models
{
    public static class BundleExtensions
    {
        public static Setting ToModel(this SettingViewModel vm)
        {
            return new Setting
            {
                BundleId = vm.BundleId,
                Name = vm.Name,
                ImagePath = vm.ImagePath,
                Scenes = vm.Scenes.Select(x => x.ToModel()).ToList(),
                Elements = vm.Elements.Select(x => x.ToModel()).ToList(),
                Effects = vm.Effects.Select(x => x.ToModel()).ToList()
            };
        }

        public static Scene ToModel(this SceneViewModel vm)
        {
            return new Scene
            {
                Name = vm.Name,
                ImagePath = vm.ImagePath,
                ElementsSetup = vm.Elements.Select(x => x.ToModel()).ToList()
            };
        }

        public static TrackSetup ToModel(this TrackSetupViewModel vm)
        {
            return new TrackSetup
            {
                FilePath = vm.FilePath,
                Volume = vm.Volume,
            };
        }

        public static Track ToModel(this SceneComponentViewModel vm)
        {
            return new Track
            {
                Alias = vm.Alias,
                FilePath = vm.FilePath,
            };
        }

        public static SettingViewModel ToVM(this Setting m)
        {
            return new SettingViewModel
            {
                BundleId = m.BundleId,
                Name = m.Name,
                ImagePath = m.ImagePath,
                Scenes = m.Scenes.Select(x => x.ToVM()).ToObservableCollection(),
                Elements = m.Elements.Select(x => x.ToVM(true)).ToObservableCollection(),
                Effects = m.Effects.Select(x => x.ToVM(false)).ToObservableCollection()
            };
        }

        public static SceneViewModel ToVM(this Scene m)
        {
            return new SceneViewModel
            {
                Name = m.Name,
                ImagePath = m.ImagePath,
                Elements = m.ElementsSetup.Select(x => x.ToVM()).ToObservableCollection()
            };
        }

        public static TrackSetupViewModel ToVM(this TrackSetup m)
        {
            return new TrackSetupViewModel
            {
                FilePath = m.FilePath,
                Volume = m.Volume,
            };
        }

        public static SceneComponentViewModel ToVM(this Track m, bool loop)
        {
            return new SceneComponentViewModel
            {
                Alias = m.Alias,
                FilePath = m.FilePath,
                Loop = loop
            };
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection) where T : class
        {
            return new ObservableCollection<T>(collection);
        }
    }
}
