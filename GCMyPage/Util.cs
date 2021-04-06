using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GCMyPage {
    public class Util {
        public static String JsonToString(String raw) {
            StringBuilder sb = new StringBuilder("\n");
            int lvl = -4;
            JsonTextReader reader = new JsonTextReader(new StringReader(raw));
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType.ToString().Equals("PropertyName")) {
                        for (int i = 0; i < lvl; i++)
                            sb.Append("-");
                        sb.Append("|" + reader.Value + " - ");
                    } else {
                        sb.Append(reader.Value + "\n");
                    }
                } else {
                    if (reader.TokenType.ToString().StartsWith("Start")) {
                        lvl += 4;
                        sb.Append("\n");
                    } else if (reader.TokenType.ToString().StartsWith("End")) {
                        lvl -= 4;
                    }
                }
            }
            
            return sb.ToString();
        }

        public static DiffInfo GetDiffInfoFromJObj(int diff, JObject obj) {
            DiffInfo result = new DiffInfo();
            if (obj == null || !obj.HasValues) {
                result.diff = Constants.SongMark.Difficulty[diff];
                return result;
            }
            result.diff = obj["music_level"].Value<String>();
            result.rating = obj["rating"].Value<String>();
            result.score = obj["score"].Value<int>();
            result.chain_max = obj["max_chain"].Value<int>();
            
            if (obj["perfect"].Value<int>() != 0) result.chain_status = 3;
            else if (obj["full_chain"].Value<int>() != 0) result.chain_status = 2;
            else if (obj["no_miss"].Value<int>() != 0) result.chain_status = 1;

            result.clear = (obj["is_clear_mark"].Value<bool>() || !obj["is_failed_mark"].Value<bool>());
            result.play_count = obj["play_count"].Value<int>();

            return result;
        }
    }
}
