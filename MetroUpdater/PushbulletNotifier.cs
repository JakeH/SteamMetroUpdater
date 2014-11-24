using log4net;
using System;
using System.Net;
using System.Text;

namespace MetroUpdater
{
    /// <summary>
    /// Notification handler for Pushbullet
    /// </summary>
    internal class PushbulletNotifier : INotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger("MetroUpdater");

        private readonly string APIToken;
        private readonly string APIUri;

        public PushbulletNotifier(string apiToken, string apiUri)
        {
            this.APIToken = apiToken;
            this.APIUri = apiUri;
        }

        public void Notify(string title, string message)
        {
            string json = @"{""type"":""note"", ""title"":""" + title + @""", ""body"":""" + message + @"""}";

            using (CookieClient client = new CookieClient())
            {
                client.Encoding = Encoding.UTF8;

                client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(this.APIToken + ":")));

                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                try
                {
                    client.UploadString(this.APIUri, "POST", json);
                }
                catch (WebException webex)
                {
                    Logger.Error("Could not push to Pushbullet.", webex);
                }
            }
        }
    }
}