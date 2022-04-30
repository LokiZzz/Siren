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
                ImagePath = vm.ImagePath
            };
        }

        public static SettingViewModel ToVM(this Setting m)
        {
            return new SettingViewModel
            {
                Name = m.Name,
                ImagePath = m.ImagePath,
                Scenes = m.Scenes.Select(x => x.ToVM()).ToObservableCollection(),
                Elements = m.Elements.Select(x => x.ToVM()).ToObservableCollection(),
                Effects = m.Effects.Select(x => x.ToVM()).ToObservableCollection()
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

        public static SceneComponentViewModel ToVM(this Track m)
        {
            return new SceneComponentViewModel
            {
                Alias = m.Alias,
                FilePath = m.FilePath,
                ImagePath = m.ImagePath
            };
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection) where T : class
        {
            return new ObservableCollection<T>(collection);
        }
    }
}
