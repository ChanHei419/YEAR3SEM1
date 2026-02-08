using System.Collections.Generic;

namespace TaikoGame
{
    public class SongManager
    {
        public static Dictionary<string, (string filename, double duration, string title, string artist)> GetSongs()
        {
            return new Dictionary<string, (string, double, string, string)>
            {
                { "song1", ("1.wav", 173.15, "阿修羅ちゃん", "Ado") },
                { "song2", ("2.wav", 96.71, "千本桜", "初音ミク") }
            };
        }
    }
}