using System;
using System.IO;

namespace GCMyPage {
    public class Logger {
        private String TAG;

        public Logger(String t) {
            TAG = t;
        }

        public void SetSubTag(String s) {
            TAG += ("-" + s);
        }

        public void Info(String s) {
            foreach(String ss in s.Split('\n'))
                Console.WriteLine(DateTime.Now + " " + TAG + " [I] " + ss);
        }

        public void Debug(String s) {
            if (Constants.Debug) {
                foreach(String ss in s.Split('\n'))
                    Console.WriteLine(DateTime.Now + " " + TAG + " [D] " + ss);
            }
        }

        public void Error(String s) {
            foreach(String ss in s.Split('\n'))
                Console.WriteLine(DateTime.Now + " " + TAG + " [E] " + ss);
        }
    }
}