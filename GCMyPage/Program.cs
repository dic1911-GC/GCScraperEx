using System;
using Newtonsoft.Json.Linq;

namespace GCMyPage {
    internal class Program {
        public static void Main(string[] args) {
            Logger log = new Logger("main");

            Network client = new Network();
            client.login(false);
            String stats = client.GetData(Constants.URL.Basic);
            while (JObject.Parse(stats)["status"].Value<int>() == 1) {
                client.login(true);
                stats = client.GetData(Constants.URL.Basic);
            }
            stats = Util.JsonToString(stats);
            Handler handler = new Handler(client);
            // Console.WriteLine("");

            int c, cc;
            while (true) {
                log.Info(Constants.divider);
                log.Info("1. Show basic stats");
                log.Info("2. Backup Score");
                log.Info("\n0. Exit");
                log.Info(Constants.divider);
                Console.Write("Enter your choice: ");
                String input = Console.ReadKey().KeyChar.ToString();
                Console.WriteLine();
                if (!int.TryParse(input, out c)) continue;
                switch (c) {
                    case 1:
                        log.Info(stats);
                        log.Info(Constants.divider);
                        log.Info("Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                    case 2:
                        log.Info(Constants.divider);
                        log.Info("0. Pretty/Parsed plain-text (.txt)");
                        log.Info("1. Excel (.xlsx)");
                        log.Info(Constants.divider);
                        Console.Write("Choose file format: ");
                        cc = int.Parse(Console.ReadKey().KeyChar.ToString());
                        Console.WriteLine();
                        if (cc != 0 && cc != 1) {
                            log.Error("\nInvalid input, returning to menu...\n");
                            break;
                        }
                        handler.BackupScores(true, "", cc);
                        break;
                    case 0:
                        return;
                }
            }
        }
    }
}