using System;
using System.Collections.Generic;
using System.Text;

namespace GCMyPage {
    public class DiffInfo {
        public String diff = "";
        public int play_count;
        public String rating = " - ";
        public int score = -1;
        public bool clear = false;
        public int chain_status = 0;
        public int chain_max = 0;
        public int rank = -1;

        public override string ToString() {
            StringBuilder sb = new StringBuilder("\t" + diff);
            if (score == -1)
                sb.Append(" Not played");
            else {
                sb.Append(" [").Append(clear ? "Cleared" : "Failed").Append(" - ")
                    .Append(Constants.SongMark.ChainStatus[chain_status]).Append("]\n");
                sb.Append("\t\tScore: ").Append(score).Append(" (").Append(rating).Append(")\n");
                sb.Append("\t\tPlay count: ").Append(play_count).Append(" Max Chain: ").Append(chain_max).Append("\n");
                sb.Append("\t\tRank: ").Append(rank).Append("\n\n");
            }

            return sb.ToString();
        }
    }
    public class SongInfo {
        private int id, total_pc;
        private String title, timestamp = "-";
        private Dictionary<int, DiffInfo> scores;
        private bool isFavorite;
        private bool has_ex;

        private Logger log = new Logger("SongInfo");

        public SongInfo(int i, String t, bool ex) {
            id = i;
            title = t;
            scores = new Dictionary<int, DiffInfo>();
            isFavorite = false;
            has_ex = ex;
        }

        public void SetDiffData(int diff, DiffInfo info) {
            scores.Add(diff, info);
        }

        public void SetDiffData(int diff, int pc, String rating, int score, bool clear, int chain_status, int chain_max) {
            DiffInfo data = new DiffInfo();

            data.play_count = pc;
            data.rating = rating;
            data.score = score;
            data.clear = clear;
            data.chain_status = chain_status;
            data.chain_max = chain_max;

            scores.Add(diff, data);
        }

        public void SetDiffRank(int diff, int rank) {
            log.Debug("Setting " + id + " - " + diff);
            scores[diff].rank = rank;
        }

        public void SetIsFavorite() {
            isFavorite = true;
        }

        public void SetLastPlayTime(String time) {
            timestamp = time;
        }

        public void SetTotalPlayCount(int pc) {
            total_pc = pc;
        }

        public int GetID() {
            return id;
        }

        public String GetTitle() {
            return title;
        }

        public bool HasEx() {
            return has_ex;
        }

        public bool IsFavorite() {
            return isFavorite;
        }

        public DiffInfo GetDiff(int i) {
            return scores[i];
        }

        public String GetTimestamp() {
            return timestamp;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder("\nSong: ");
            sb.Append(title).Append(" (").Append(id).Append(")\n\n");
            for (int i = 0; i < scores.Count; i++) {
                if (i != 0) sb.Append("\n\n");
                DiffInfo s = scores[i];
                sb.Append(s);
            }

            sb.Append(Constants.divider + "\n");
            return sb.ToString();
        }
    }
}
