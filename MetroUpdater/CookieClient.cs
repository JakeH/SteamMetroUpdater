using System;
using System.Net;

namespace MetroUpdater
{
    /// <summary>
    /// <see cref="WebClient"/> with cookies
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
    internal class CookieClient : WebClient
    {
        private readonly CookieContainer CookieJar = new CookieContainer();

        private void GetCookies(WebResponse response)
        {
            HttpWebResponse wr = response as HttpWebResponse;
            if (wr == null)
                return;

            this.CookieJar.Add(wr.Cookies);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var ret = base.GetWebRequest(address);
            HttpWebRequest request = ret as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = this.CookieJar;

                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            }

            return ret;
        }

        /// <summary>
        /// Returns an <see cref="HttpWebRequest"/> using the current <see cref="WebClient"/> instance
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public HttpWebRequest GetHttpWebRequest(Uri address)
        {
            return this.GetWebRequest(address) as HttpWebRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var ret = base.GetWebResponse(request);
            GetCookies(ret);
            return ret;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            var ret = base.GetWebResponse(request, result);
            GetCookies(ret);
            return ret;
        }
    }
}