using System;

namespace GCMyPage {
    public class Constants {
        public const bool Debug = false;
        public const bool DebugLogToFile = true;
        public const int Threads = 4;
        public const String divider = "==============================================";

        public class SongMark {
            public static String[] Difficulty = new[] {"SIMPLE", "NORMAL", "HARD", "EXTRA"};
            public static String[] ChainStatus = new[] {"NONE", "NO MISS", "FULL CHAIN", "PERFECT"};
        }

        public class URL {
            public const String Base = "https://mypage.groovecoaster.jp/sp/";
            public const String Login = "login/auth_con.php";

            public const String Basic = "json/player_data.php";
            
            public const String MusicList = "json/music_list.php";
            public const String MusicSelf = "json/music_detail.php?music_id=";
            public const String MusicFriend = "json/friend_music_detail.php?music_id="; // + music_id + "&hash=" + friendHash;
        }
    }
}