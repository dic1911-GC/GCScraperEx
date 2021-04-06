using System;
using System.Net;

namespace GCMyPage {
    public class Client : WebClient {
        private CookieContainer cookies;

        public Client() {
            cookies = new CookieContainer();
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => {
                return true;
            };
        }
        
        public Client(CookieContainer c) {
            cookies = c;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => {
                return true;
            };
        }

        protected override WebRequest GetWebRequest(Uri address) {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest) {
                (request as HttpWebRequest).CookieContainer = cookies;
            }
            return request;
        }

        public CookieContainer GetCookies() {
            return cookies;
        }
    }
}
