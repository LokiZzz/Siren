using System;
using System.Collections.Generic;
using System.Text;

namespace Siren.Messaging
{
    public static class Messages
    {
        public static string ElementPlayingStatusChanged = nameof(ElementPlayingStatusChanged);
        public static string MusicTrackPlayingStatusChanged = nameof(MusicTrackPlayingStatusChanged);

        public static string SettingAdded = nameof(SettingAdded);
        public static string SceneAdded = nameof(SceneAdded);
        public static string IllustratedCardEdited = nameof(IllustratedCardEdited);
        public static string NeedToUpdateEnvironment = nameof(NeedToUpdateEnvironment);
    }
}
