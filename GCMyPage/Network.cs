using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GCMyPage {
    public class Network {
        private Client client;
        private Logger log = new Logger("Network");
        private Boolean logged_in = false;
        private String id, passwd = "";

        public Network() {
            client = new Client();
        }
        
        public Network(Network c) {
            client = new Client(c.client.GetCookies());
            logged_in = true;
        }

        public void login(Boolean failed) {
            if (failed) {
                log.Error(Constants.divider);
                log.Error("Previous login attempt failed.");
                log.Error("Please check your \"acc.txt\" or the credential you entered.");
                log.Error(Constants.divider);
                log.Error("Press any key to continue...");
                Console.ReadKey(true);
            }
            
            if (File.Exists("acc.txt")) {
                String[] acc = File.ReadAllLines("acc.txt");
                id = acc[0];
                passwd = acc[1];
                log.Info("Auto login with acc.txt");
                log.Info("CardID = " + id + ", password = " + passwd);
            } else {
                Console.Write("Card ID: ");
                id = Console.ReadLine();
                Console.Write("Password(HIDDEN): ");
                while (true) {
                    ConsoleKeyInfo k = Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Enter) break;
                    if (k.Key == ConsoleKey.Backspace && (passwd.Length - 1) >= 0)
                        passwd = passwd.Substring(0, ((passwd.Length - 2) >= 0) ? passwd.Length - 2 : 0);
                    else
                        passwd += k.KeyChar;
                }
            }
            
            var body = new NameValueCollection{
                {"nesicaCardId", id},
                {"password", passwd}
            };
            var resp = client.UploadValues(Constants.URL.Base + Constants.URL.Login, body);
            // log.debug(Encoding.Default.GetString(resp));

            // erase password from memory
            id = "";
            passwd = "";
        }

        public String GetData(String url) {
            
            String url_full = Constants.URL.Base + url;
            
            var resp = client.DownloadString(url_full);
            // log.Debug(resp);
            return resp;
        }
    }
}