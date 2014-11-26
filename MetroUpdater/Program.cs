using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using MetroUpdater.Properties;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace MetroUpdater
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("MetroUpdater");
        private static INotifier Notifier;

        private struct RemoteMetroVersionInfo
        {
            public string Version;
            public Uri DeviantArtPage;
        }

        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Logger.Error("Unhandled exception", (e.ExceptionObject as Exception));
            };

            switch (Settings.Default.Notifier.ToLower().Trim())
            {
                case "pushover":
                    Notifier = new PushoverNotifier(Settings.Default.PushoverUserToken, Settings.Default.PushoverAPIToken, Settings.Default.PushoverAPIUri);
                    break;

                case "pushbullet":
                    Notifier = new PushbulletNotifier(Settings.Default.PushbulletAPIToken, Settings.Default.PushbulletAPIUri);
                    break;
            }
        }

        private static void Main(string[] args)
        {
            PerformTasks();
        }

        /// <summary>
        /// Performs the entire set of tasks required to update the local Metro skin folder
        /// </summary>
        private static void PerformTasks()
        {
            var remoteVersion = GetRemoteVersionInfo();

            if (string.IsNullOrWhiteSpace(remoteVersion.Version) || remoteVersion.DeviantArtPage == null)
            {
                Logger.Error("Could not get latest version information.");

                if (Notifier != null)
                    Notifier.Notify(NotifyStrings.ApplicationTitle, NotifyStrings.FailedToGetLatestInfo);

                return;
            }

            string localVersion = (Settings.Default.LocalVersion ?? string.Empty).Trim();

            // if the remote version and local version are the same, there's no work to be done.
            if (string.Equals(remoteVersion.Version, localVersion))
            {
                Logger.Info("Local version is already up-to-date.");
                return;
            }

            var zipFile = DownloadFromDeviantArt(remoteVersion.DeviantArtPage);

            if (zipFile == null || zipFile.Length == 0)
            {
                Logger.Error("Could not get skin zip file.");

                if (Notifier != null)
                    Notifier.Notify(NotifyStrings.ApplicationTitle, NotifyStrings.FailedToGetZip);

                return;
            }

            ExtractZipToSkinFolder(zipFile);

            // update the last known version information
            Settings.Default.LocalVersion = remoteVersion.Version;
            Settings.Default.Save();

            // if we have a notifier attached, send the notification
            if (Notifier != null)
                Notifier.Notify(NotifyStrings.ApplicationTitle, string.Format(NotifyStrings.UpdatedToVersion, remoteVersion.Version));

            Logger.Info("Updated skin to version " + remoteVersion.Version);
        }

        /// <summary>
        /// Extracts the downloaded skin zip to the destination folder
        /// </summary>
        /// <param name="zipFile"></param>
        private static void ExtractZipToSkinFolder(FileInfo zipFile)
        {
            // the zip will have a few different files and folders, we're only concerened with the one containing the skin
            string matchStartFolder = "Metro for Steam/";

            using (ZipFile zf = new ZipFile(zipFile.OpenRead()))
            {
                foreach (ZipEntry entry in zf)
                {
                    if (entry.IsDirectory)
                        continue;

                    if (!entry.Name.StartsWith(matchStartFolder, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string outputFilename = Path.Combine(Settings.Default.SkinFolder, entry.Name.Remove(0, matchStartFolder.Length).Replace('/', '\\'));

                    if (!Directory.Exists(Path.GetDirectoryName(outputFilename)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(outputFilename));
                    }

                    using (Stream zipStream = zf.GetInputStream(entry))
                    using (FileStream writer = File.Create(outputFilename))
                    {
                        byte[] buffer = new byte[4096];
                        StreamUtils.Copy(zipStream, writer, buffer);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the remote version information from the Metro Skin main page
        /// </summary>
        /// <returns></returns>
        private static RemoteMetroVersionInfo GetRemoteVersionInfo()
        {
            RemoteMetroVersionInfo ret = new RemoteMetroVersionInfo();

            using (CookieClient client = new CookieClient())
            {
                HtmlDocument doc = client.GetHtmlDocument(new Uri(Settings.Default.MetroHomeUri));

                // there are two such "Latest Version" elements on the page. we're assuming the first one is the skin
                var versionElements = doc.DocumentNode.SelectNodes("//p[contains(text(), 'Latest Version')]");
                if (versionElements.Count == 0)
                {
                    Logger.Error("No version elements found!");
                    return ret;
                }

                var firstVersionElement = versionElements[0];
                ret.Version = firstVersionElement.InnerText.Replace("Latest Version:", string.Empty).Trim();

                // the download link element will be a sibling to the versio element
                var downloadElement = firstVersionElement.ParentNode.SelectSingleNode("//a[contains(@class, 'button')]");

                if (downloadElement == null)
                {
                    Logger.Error("No download link elements found!");
                    return ret;
                }

                var downloadUri = new Uri(downloadElement.Attributes["href"].Value);

                // if the downloadUri is pointing to the fav.me domain, we need to resolve by requesting the resource and then getting the location header entry in the response
                if (Regex.IsMatch(downloadUri.Host, @"^.*\.?fav\.me$", RegexOptions.IgnoreCase))
                {
                    var req = client.GetHttpWebRequest(downloadUri);
                    req.KeepAlive = true;
                    req.AllowAutoRedirect = false;
                    var res = req.GetResponse();
                    ret.DeviantArtPage = new Uri(res.Headers[HttpResponseHeader.Location]);
                }
                else if (Regex.IsMatch(downloadUri.Host, @"^.*\.?deviantart\.com$", RegexOptions.IgnoreCase))
                {
                    // exact match for deviantart domain
                    ret.DeviantArtPage = downloadUri;
                }
                else
                {
                    Logger.Error("Cannot handle the domain given in the download link! Got: " + downloadUri.ToString());
                    return ret;
                }
            }

            return ret;
        }

        /// <summary>
        /// Downloads the compressed file containing the skin from the Deviant Art page.
        /// </summary>
        /// <param name="downloadPageUri"></param>
        /// <returns></returns>
        private static FileInfo DownloadFromDeviantArt(Uri downloadPageUri)
        {
            // create a temp file for us to download to
            FileInfo zipFile = new FileInfo(Path.GetTempFileName());

            using (CookieClient client = new CookieClient())
            {
                var doc = client.GetHtmlDocument(downloadPageUri);

                // the download element should be the only one with the class dev-page-download
                var downloadElement = doc.DocumentNode.SelectSingleNode("//a[contains(@class, 'dev-page-download')]");

                if (downloadElement == null)
                {
                    Logger.Error("No download element found on Deviant Art page. " + downloadPageUri.ToString());
                    return zipFile;
                }

                var href = downloadElement.Attributes["href"];

                Uri downloadUri = new Uri(WebUtility.HtmlDecode(href.Value));

                client.Headers.Set(HttpRequestHeader.Host, "www.deviantart.com");
                client.Headers.Set(HttpRequestHeader.Referer, downloadPageUri.ToString());

                var req = client.GetHttpWebRequest(downloadUri);

                req.KeepAlive = true;
                req.AllowAutoRedirect = false;

                var res = req.GetResponse();

                // the actual download link will be in the response header when we request the uri from the download page itself.

                string zipUri = res.Headers[HttpResponseHeader.Location];

                if (string.IsNullOrWhiteSpace(zipUri))
                {
                    Logger.Error("No location returned for download link on Deviant Art page.");
                    return zipFile;
                }

                if (!string.Equals(".zip", Path.GetExtension(zipUri), StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Error("Download link location on Deviant Art page was not a zip file." + zipUri);
                    return zipFile;
                }

                client.DownloadFile(zipUri, zipFile.FullName);
            }

            return zipFile;
        }
    }
}