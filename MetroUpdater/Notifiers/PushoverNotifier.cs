using log4net;
using System.Net;
using System.Text;

namespace MetroUpdater
{
    /// <summary>
    /// Notification handler for Pushover
    /// </summary>
    internal class PushoverNotifier : INotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger("MetroUpdater");

        private readonly string UserToken;
        private readonly string APIToken;
        private readonly string APIUri;

        public PushoverNotifier(string userToken, string apiToken, string apiUri)
        {
            this.UserToken = userToken;
            this.APIToken = apiToken;
            this.APIUri = apiUri;
        }

        public void Notify(string title, string message)
        {
            string param = string.Format(@"token={0}&user={1}&title={2}&message={3}",
                this.APIToken, this.UserToken, title, message);

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;

                try
                {
                    client.UploadString(this.APIUri, "POST", param);
                }
                catch (WebException webex)
                {
                    Logger.Error("Could not push to Pushover.", webex);
                }
            }
        }
    }
}