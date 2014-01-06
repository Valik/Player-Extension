using Newtonsoft.Json;
using PlayerExtension.Common.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace PlayerExtension.Common
{
    public static class ConfigManager
    {
        private const string ConfigFolderName = "SavingConfig";
        private const string VkConfigName = "vkConfig.json";
        private const string LastFMConfigName = "lastfmConfig.json";
        private const string StoragesConfigName = "storagesConfig.json";
        private static StorageFolder mConfigFolder;


        public static async Task<VKConfig> GetVkConfig()
        {
            var config = await GetConfig<VKConfig>(VkConfigName).ConfigureAwait(false);
            return config;
        }

        public static async Task<LastFMConfig> GetLastFMConfig()
        {
            var config = await GetConfig<LastFMConfig>(LastFMConfigName).ConfigureAwait(false);
            return config;
        }

        public static async Task<StoragesConfig> GetStoragesConfig()
        {
            var config = await GetConfig<StoragesConfig>(StoragesConfigName).ConfigureAwait(false);
            return config;
        }

        public static async Task<bool> TrySaveVkConfig(VKConfig config)
        {
            bool result = await SaveConfig(config, VkConfigName).ConfigureAwait(false);
            return result;
        }

        public static async Task<bool> TrySaveLastConfig(LastFMConfig config)
        {
            bool result = await SaveConfig(config, LastFMConfigName).ConfigureAwait(false);
            return result;
        }

        public static async Task<bool> TrySaveStoragesConfig(StoragesConfig config)
        {
            bool result = await SaveConfig(config, StoragesConfigName).ConfigureAwait(false);
            return result;
        }

        private static async Task<T> GetConfig<T>(string fileName) where T : class
        {
            try
            {
                var configFolder = await GetConfigFolder();
                var configFile = await configFolder.GetFileAsync(fileName).AsTask().ConfigureAwait(false);

                string strConfig = await FileIO.ReadTextAsync(configFile).AsTask().ConfigureAwait(false);

                var result = JsonConvert.DeserializeObject<T>(strConfig);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<bool> SaveConfig(object config, string fileName)
        {
            try
            {
                var str = JsonConvert.SerializeObject(config);

                var configFolder = await GetConfigFolder();
                var configFile = await configFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
                await FileIO.WriteTextAsync(configFile, str).AsTask().ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<StorageFolder> GetConfigFolder()
        {
            if (mConfigFolder != null)
                return mConfigFolder;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            mConfigFolder = await localFolder.CreateFolderAsync(ConfigFolderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
            return mConfigFolder;
        }


    }
}
