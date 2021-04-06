using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace GCMyPage {
    public class Handler {
        private Network client;

        private Logger log = new Logger("handler");

        public Handler(Network c) {
            client = c;
        }

        private class ScoreFetcher {
            private Network client;
            private int music_id, total_pc;
            private bool self;
            private SongInfo song;
            private String timestamp;

            private Logger log = new Logger("ScoreFetcher");

            public ScoreFetcher(Network c, int m, bool s, int tpc, String last_played) {
                client = new Network(c);
                music_id = m;
                self = s;
                timestamp = last_played;
                total_pc = tpc;
            }

            public void run() {
                String url = self ? (Constants.URL.MusicSelf + music_id) : (Constants.URL.MusicFriend + music_id);
                String resp = client.GetData(url);
                // log.Info(resp);
                song = new Parser().ParseMusicInfoJson(resp);
                song.SetLastPlayTime(timestamp);
                song.SetTotalPlayCount(total_pc);
            }

            public SongInfo GetSongInfo() {
                return song;
            }
        }

        public void BackupScores(Boolean self, String friendHash, int mode) {
            DateTime t0 = DateTime.Now;
            var resp = client.GetData(Constants.URL.MusicList);
            // log.Debug(Util.JsonToString(resp));

            Dictionary<int, ArrayList> data = new Parser().ParseMusicListJson(resp);
            log.Info("Music list length = " + data.Keys.Count);

            List<SongInfo> songs = new List<SongInfo>();

            ArrayList threads = new ArrayList();
            ArrayList runningFetchers = new ArrayList();
            int RunningJobs = 0;
            // use some threads to fetch all the scores from list
            foreach (int id in data.Keys) {
                log.Info("Fetching (" + id + "). " + data[id][0]);
                ScoreFetcher f = new ScoreFetcher(client, id, self, (int) data[id][1], (String) data[id][2]);
                Thread t = new Thread(f.run);
                t.Start();
                runningFetchers.Add(f);
                threads.Add(t);
                ++RunningJobs;

                // Executing thread amount reached limit, wait for the first thread in list to finish
                if (RunningJobs >= Constants.Threads) {
                    ((Thread) threads[0]).Join();
                    songs.Add(((ScoreFetcher) runningFetchers[0]).GetSongInfo());
                    runningFetchers.RemoveRange(0, 1);
                    threads.RemoveRange(0, 1);
                    --RunningJobs;
                    // if (songs.Count > 10) break; // faster testing
                }
            }

            foreach (Thread t in threads) {
                t.Join();
            }

            foreach (ScoreFetcher f in runningFetchers) {
                songs.Add(f.GetSongInfo());
            }

            String name = (DateTime.Today.ToString()).Replace("/", "_").Split(' ')[0];
            if (mode == 0) {
                name += ".txt";
                File.Create(name).Dispose();
                File.WriteAllText(name, "");
                foreach (SongInfo si in songs) {
                    log.Info(si.ToString());
                    File.AppendAllText(name, si.ToString(), Encoding.UTF8);
                }
            } else {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                name += ".xlsx";
                ExcelPackage xls = new ExcelPackage();
                ExcelWorksheet sheet = xls.Workbook.Worksheets.Add("Scores");
                String[] heading = {
                    "ID", "TITLE", "SIMPLE\nMARK", "SIMPLE\nRATING", "SIMPLE\nSCORE",
                    "SIMPLE\nCHAIN", "SIMPLE\nPLAYS", "SIMPLE\nRANK", "NORMAL\nMARK", "NORMAL\nRATING",
                    "NORMAL\nSCORE", "NORMAL\nCHAIN", "NORMAL\nPLAYS", "NORMAL\nRANK", "HARD\nMARK",
                    "HARD\nRATING", "HARD\nSCORE", "HARD\nCHAIN", "HARD\nPLAYS", "HARD\nRANK",
                    "EXTRA\nMARK", "EXTRA\nRATING", "EXTRA\nSCORE", "EXTRA\nCHAIN", "EXTRA\nPLAYS",
                    "EXTRA\nRANK", "TIMESTAMP", "FAVORITE"
                };
                Char col = 'A';
                try {
                    for (int i = 0; i < 26; i++) {
                        sheet.Cells[col++ + "1"].Value = heading[i];
                    }

                    sheet.Cells["AA1"].Value = heading[26];
                    sheet.Cells["AB1"].Value = heading[27];
                    sheet.View.FreezePanes(2, 1);
                } catch (TypeLoadException e) {
                    log.Debug("current col = " + col);
                    log.Debug(e.StackTrace);
                }

                log.Debug("heading all set");

                int row = 2;
                foreach (SongInfo si in songs) {
                    sheet.Cells["A" + row].Value = si.GetID();
                    sheet.Cells["B" + row].Value = si.GetTitle();
                    col = 'C';
                    for (int i = 0; i < (si.HasEx() ? 4 : 3); i++) {
                        DiffInfo score = si.GetDiff(i);
                        if (score.score == -1) {
                            sheet.Cells[(col).ToString() + row].Value = "NOT PLAYED";
                            ++col;++col;++col;++col;++col;++col; // col += 6;
                            continue;
                        } 
                        sheet.Cells[(col++).ToString() + row].Value =
                            Constants.SongMark.ChainStatus[score.chain_status];
                        sheet.Cells[(col++).ToString() + row].Value = score.rating;
                        sheet.Cells[(col++).ToString() + row].Value = score.score;
                        sheet.Cells[(col++).ToString() + row].Value = score.chain_max;
                        sheet.Cells[(col++).ToString() + row].Value = score.play_count;
                        sheet.Cells[(col++).ToString() + row].Value = score.rank;
                    }

                    sheet.Cells["AA" + (row)].Value = si.GetTimestamp();
                    sheet.Cells["AB" + (row++)].Value = si.IsFavorite() ? "Yes" : "No";
                }

                sheet.Cells["A1:AB1"].Style.WrapText = true;
                sheet.Row(1).Height = 30;
                sheet.Cells["A1:AB" + (songs.Count + 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells["A1:AB" + (songs.Count + 1)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet.Cells["A1:AB" + (row - 1)].AutoFitColumns(20);

                FileInfo fi = new FileInfo(name);
                if (fi.Exists) fi.Delete();
                fi.Create();
                xls.SaveAs(fi);
                log.Info("Elapsed time: " + (DateTime.Now - t0));
            }
        }
    }
}
