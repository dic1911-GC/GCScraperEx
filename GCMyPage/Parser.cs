using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GCMyPage {
    public class Parser {
        private Logger log = new Logger("Parser");

        public Dictionary<int, ArrayList> ParseMusicListJson(String raw) {
            // log.Debug(raw);
            int parse_type = -1, music_id = -1, pc = 0;
            String music_title = "", last_played = "";
            Dictionary<int, ArrayList> result = new Dictionary<int, ArrayList>();
            
            JsonTextReader reader = new JsonTextReader(new StringReader(raw));
            while (reader.Read()) {
                if (reader.Value == null) continue; // skip unused json library stuff 
                if (reader.TokenType.ToString().Equals("PropertyName")) {
                    // key
                    switch (reader.Value) {
                        case "music_id":
                            parse_type = 0;
                            break;
                        case "music_title":
                            parse_type = 1;
                            break;
                        case "play_count":
                            parse_type = 2;
                            break;
                        case "last_play_time":
                            parse_type = 3;
                            break;
                        default:
                            parse_type = -1;
                            break;
                    }
                } else {
                    // value
                    try {
                        switch (parse_type) {
                            case 0:
                                music_id = int.Parse(reader.Value.ToString());
                                music_title = "";
                                last_played = "";
                                pc = 0;
                                break;
                            case 1:
                                music_title = reader.Value.ToString();
                                break;
                            case 2:
                                pc = int.Parse(reader.Value.ToString());
                                break;
                            case 3:
                                last_played = reader.Value.ToString();
                                break;
                        }
                        if (!last_played.Equals("")) {
                            ArrayList val = new ArrayList();
                            log.Debug(music_id + " " + music_title);
                            val.Add(music_title);
                            val.Add(pc);
                            val.Add(last_played);
                            result.Add(music_id, val);
                            music_id = -1;
                        }
                    } catch (InvalidOperationException e) {
                        log.Error("An error has occurred during parsing music list, ignoring...");
                        log.Error(e.Message);
                    }
                }
            }

            return result;
        }

        /*
          System.Collections.Generic.KeyNotFoundException: The given key '2' was not present in the dictionary.
          at System.Collections.Generic.Dictionary`2[TKey,TValue].get_Item (TKey key) [0x0001e] in <efe941bb62534dc3a62ceb1a818964a0>:0 
          at GCMyPage.SongInfo.SetDiffRank (System.Int32 diff, System.Int32 rank) [0x00001] in <3b879ba6f44b45aabf1b9c5a42fb6f0b>:0 
          at GCMyPage.Util.ParseMusicInfoJson (System.String raw) [0x00320] in <3b879ba6f44b45aabf1b9c5a42fb6f0b>:0 
          at GCMyPage.Handler+ScoreFetcher.run () [0x00049] in <3b879ba6f44b45aabf1b9c5a42fb6f0b>:0 
          
          SetDiffData with wrong diff (-1)
         */
        
        public SongInfo ParseMusicInfoJson(String raw) {
            JObject json = JObject.Parse(raw).GetValue("music_detail") as JObject;
            // log.Debug(json.ToString());
            int music_id = (int) json["music_id"];
            String music_title = (String) json["music_title"];
            bool has_ex = (bool) json["ex_flag"];
            log.SetSubTag(music_id.ToString());
            
            SongInfo result = new SongInfo(music_id, music_title, has_ex);
            if (json["fav_flg"].Value<bool>()) result.SetIsFavorite();

            String[] keys = new[] {"simple_result_data", "normal_result_data", "hard_result_data", "extra_result_data"};
            for (int i = 0; i < keys.Length; i++) {
                if (i == 3 && !has_ex) break;
                result.SetDiffData(i, Util.GetDiffInfoFromJObj(i, json[keys[i]].HasValues ? json[keys[i]].Value<JObject>() : null));
            }

            JArray ranks = (JArray) json["user_rank"];
            for (int i = 0; i < ranks.Count; i++) {
                if (ranks[i].HasValues) {
                    result.SetDiffRank(i, ranks[i]["rank"].Value<int>());
                }
            }

            // log.Debug(result.ToString());
            return result;
        }
    }
}
