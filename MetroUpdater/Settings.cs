using Nini.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroUpdater
{
    /// <summary>
    /// Application setting
    /// </summary>
    public static class Settings
    {
        private static readonly IConfigSource ConfigSource;
        private static readonly IConfig AppConfig;

        /// <summary>
        /// Pushbullet settings
        /// </summary>
        public static readonly PushbulletSettings Pushbullet;

        /// <summary>
        /// Pushover settings
        /// </summary>
        public static readonly PushoverSettings Pushover;

        /// <summary>
        /// Uri for the skin home page
        /// </summary>
        public static readonly string MetroHomeUri;

        /// <summary>
        /// Path to the local skin folder
        /// </summary>
        public static readonly string SkinFolder;

        /// <summary>
        /// Notifier service to be used
        /// </summary>
        public static readonly string Notifier;

        /// <summary>
        /// Local version of the skin on this computer
        /// </summary>
        public static string LocalVersion
        {
            get
            {
                return AppConfig.GetString("LocalVersion");
            }
            set
            {
                AppConfig.Set("LocalVersion", value);
            }
        }

        static Settings()
        {
            ConfigSource = new IniConfigSource("settings.ini");
            ConfigSource.AutoSave = true;

            AppConfig = ConfigSource.Configs["App"];

            Pushbullet = new PushbulletSettings
            {
                APIUri = ConfigSource.Configs["Pushbullet"].GetString("APIUri"),
                APIToken = ConfigSource.Configs["Pushbullet"].GetString("APIToken")
            };

            Pushover = new PushoverSettings
            {
                APIUri = ConfigSource.Configs["Pushover"].GetString("APIUri"),
                APIToken = ConfigSource.Configs["Pushover"].GetString("APIToken"),
                UserToken = ConfigSource.Configs["Pushover"].GetString("UserToken"),
            };

            Notifier = AppConfig.GetString("Notifier");

            SkinFolder = AppConfig.GetString("SkinFolder");
            MetroHomeUri = AppConfig.GetString("MetroHomeUri");

        }
    }

    public struct PushbulletSettings
    {
        public string APIUri;
        public string APIToken;
    }
    public struct PushoverSettings
    {
        public string APIUri;
        public string APIToken;
        public string UserToken;
    }
}
