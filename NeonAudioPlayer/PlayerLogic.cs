using System;

namespace NeonAudioPlayer
{
    public static class PlayerLogic
    {
        public static string FormatTime(TimeSpan value)
        {
            return value.TotalHours >= 1
                ? value.ToString(@"hh\:mm\:ss")
                : value.ToString(@"mm\:ss");
        }

        public static bool CanSeek(bool hasSource, bool hasDuration)
        {
            return hasSource && hasDuration;
        }
    }
}
