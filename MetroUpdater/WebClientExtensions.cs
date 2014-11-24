using HtmlAgilityPack;
using log4net;
using System;
using System.IO;
using System.Net;

namespace MetroUpdater
{
    /// <summary>
    /// Extensions for <see cref="WebClient"/>
    /// </summary>
    internal static class WebClientExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger("MetroUpdater");

        public static HtmlDocument GetHtmlDocument(this WebClient client, Uri uri)
        {
            var doc = new HtmlDocument();

            try
            {
                using (Stream stream = client.OpenRead(uri))
                {
                    doc.Load(stream);
                }
            }
            catch (WebException webex)
            {
                Logger.Error("Failed to load request.", webex);
            }

            return doc;
        }
    }
}