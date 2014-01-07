using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerExtension.ExtensionModel;
using Windows.Storage;
using Windows.Devices.Enumeration;
using Windows.Devices.Portable;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using PlayerExtension.Common;
using PlayerExtension.Common.Config;

namespace PlayerExtension
{
    public class ExtensionViewModel
    {
        private PlayerExtensionPage mMainPage;
        private ModelController mModel;
        private bool mBIsRecoveredConnectorConfig = false;
        private bool mBInSinchronization = false;
        private bool mBIsModelInit = false;

        private const string LastConfigName = "lastConfig";
        private const string VkConfigName = "vkConfig";
        private const string MusicLibraryConfigName = "musicLibraryConfig";
        private const string DevicesInfoConfigName = "devicesInfo";


        private ResourceLoader mErrorLoader = new ResourceLoader("Errors");

        private double mSyncDelayInSec = 10;
        private double mDelay = 0.2;

        public ExtensionViewModel()
        {
            PlayerExtensionPage.ConfigComplite += OnConfigComplite;
            PlayerExtensionPage.SyncStarted += OnSyncStarted;
            PlayerExtensionPage.SyncStoped += OnSyncStoped;
        }

        public async Task<FirstPage> GetFirstPage()
        {
            bool lastRes = await TryToRecoverLastConfig();
            bool vkRes = await TryToRecoverVKConfig();

            SubscribeConfigEvents();
            if(!lastRes)
                return new FirstPage(typeof(LastFMPage));
            if (!vkRes)
                return new FirstPage(typeof(VKAuthPage));

            return new FirstPage(typeof(PlayerExtensionPage));
        }

        private void SubscribeConfigEvents()
        {
            ExtConfig.ConnectorConfigChanged += OnConnectorConfigChanged;
            ExtConfig.VKConfigChanged += OnVKConfigChanged;
            ExtConfig.LastFMConfigChanged += OnLastFMConfigChanged;
        }

        private void OnConfigComplite(ConfigCompliteEvent e)
        {
            mMainPage = e.mainPage;
            InitModel();
        }

        private void InitModel()
        {
            if (!mBIsModelInit)
            {
                mModel = new ModelController();

                mModel.TrackComplite += OnTrackComplite;
                mModel.NoTracksToSync += OnNoTracksToSync;
                mModel.BadNick += OnBadNick;
                mModel.BadToken += OnBadToken;
                mModel.BadLibrary += OnBadLibrary;
                mModel.BadDevice += OnBadDevice;

                mBIsModelInit = true;
            }
        }

        private async void OnSyncStarted(StartSyncEvent e)
        {
            if(!mBIsRecoveredConnectorConfig)
                await TryToRecoverConnectorConfig();

            if(!mBInSinchronization && mBIsRecoveredConnectorConfig)
                await RunSynchronization();
        }

        private void OnSyncStoped(StopSyncEvent e)
        {
            StopSynchronization();
        }

        private void SaveAppConfig()
        {
            SaveLastConfig(ExtConfig.lastFMConfig);
            SaveVKConfig(ExtConfig.vkConfig);
            SaveConnectorConfig(ExtConfig.playerConnectorConfig);
        }

        private async void SaveLastConfig(LastFMConfig config)
        {
            var success = await ConfigManager.TrySaveLastConfig(config);
        }

        private async void SaveVKConfig(VKConfig config)
        {
            var success = await ConfigManager.TrySaveVkConfig(config);
        }

        private async void SaveConnectorConfig(PlayerConnectorConfig config)
        {
            var storagesConfig = config.storagesConfig;
            var success = await ConfigManager.TrySaveStoragesConfig(storagesConfig);
        }

        private async Task<bool> TryToRecoverLastConfig()
        {
            var lastFMConfig = await ConfigManager.GetLastFMConfig();
            if (lastFMConfig == null)
                return false;

            ExtConfig.lastFMConfig = lastFMConfig;
            return true;
        }

        private async Task<bool> TryToRecoverVKConfig()
        {
            var vkConfig = await ConfigManager.GetVkConfig();
            if (vkConfig == null)
                return false;

            ExtConfig.vkConfig = vkConfig;
            return true;
        }

        private async Task TryToRecoverConnectorConfig()
        {
            StoragesConfig storagesConfig = await ConfigManager.GetStoragesConfig();

            if (storagesConfig == null)
            {
                mBIsRecoveredConnectorConfig = false;
                TroubleWithRecoveringConnectorConfig();
                return;
            }

            StorageFolder musicLibraryFolder = null;
            try
            {
                musicLibraryFolder = await StorageFolder.GetFolderFromPathAsync(storagesConfig.musicLibraryStorage);
            }
            catch (Exception)
            {
                mBIsRecoveredConnectorConfig = false;
                OnBadLibrary(new TroubleWithMusicLibraryFolderEvent("Что-то не так с папкой для закачки треков с ласта. Выбери другую папку.", ""));
                return;
            }

            List<DeviceInfo> devices = null;
            if (storagesConfig.deviceSelected && storagesConfig.deviceStorages != null)
            {
                devices = await GetDevicesInfo(storagesConfig.deviceStorages);
                if (devices == null)
                {
                    mBIsRecoveredConnectorConfig = false;
                    OnBadDevice(new TroubleWithDeviceEvent("Плеер или папка на плеере сейчас не доступны. Подключи плеер или выбери другую папку. А может место кончилось.", "", ""));
                    return;
                }
            }

            PlayerConnectorConfig connectorConfig = devices == null ? new PlayerConnectorConfig(musicLibraryFolder) :
                                                                      new PlayerConnectorConfig(devices, musicLibraryFolder);
            if (mBIsRecoveredConnectorConfig)
                return;
            ExtConfig.playerConnectorConfig = connectorConfig;
            mBIsRecoveredConnectorConfig = true;
        }

        private async Task<List<DeviceInfo>> GetDevicesInfo(List<StorageDeviceConfig> deviceStorages)
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();

            foreach (var curDevice in deviceStorages)
            {
                try
                {
                    StorageFolder deviceFolder = await GetDeviceFolder(curDevice);
                    if (deviceFolder == null)
                    {
                        //TODO: Оповестить пользователя, устройства не доступны
                        return null;
                    }

                    devices.Add(new DeviceInfo(curDevice.name, deviceFolder));
                }
                catch (Exception)
                {
                    //TODO: Оповестить пользователя, устройства не доступны
                    return null;
                }
            }
            return devices;
        }

        private async Task<StorageFolder> GetDeviceFolder(StorageDeviceConfig deviceConfig)
        {
            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(StorageDevice.GetDeviceSelector());
            if (deviceInfoCollection.Count > 0)
                foreach (DeviceInformation curDeviceInfo in deviceInfoCollection)
                    if (curDeviceInfo.Name == deviceConfig.name)
                    {
                        StorageFolder devStorage = StorageDevice.FromId(curDeviceInfo.Id);
                        return await GetDestinationFolder(devStorage, deviceConfig.deviceFolderName, deviceConfig.deviceFolderPath);
                    }
            return null;
        }

        private async Task<StorageFolder> GetDestinationFolder(StorageFolder devStorage, String folderName, String folderPath)
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(folderPath);
            }
            catch
            {
            }

            return await GetFindFolderRecursively(devStorage, folderName, folderPath);
        }


        private async Task<StorageFolder> GetFindFolderRecursively(StorageFolder devStorage, String folderName, String folderPath)
        {
            if (devStorage.Name == folderName && devStorage.Path == folderPath)
                return devStorage;

            var subFolders = await devStorage.GetFoldersAsync();

            foreach (var curFolder in subFolders)
                if (curFolder.Name == folderName && curFolder.Path == folderPath)
                    return curFolder;
                else
                {
                    var result = await GetFindFolderRecursively(curFolder, folderName, folderPath);
                    if (result != null)
                        return result;
                }
            return null;
        }

        private async Task RunSynchronization()
        {
            mBInSinchronization = true;
            while (mBInSinchronization)
            {
                await mModel.Synchronize();
                await SyncDelay();
            }
        }

        private async Task SyncDelay()
        {
            int numberOfDelays = (int)(mSyncDelayInSec / mDelay);
            for (int curDelay = 0; curDelay < numberOfDelays; curDelay++)
            {
                if (!mBInSinchronization)
                    return;
                await Task.Delay(TimeSpan.FromSeconds(mDelay));
            }
        }

        private void StopSynchronization()
        {
            mBInSinchronization = false;
        }

        private void OnLastFMConfigChanged(LastFMConfig config)
        {
            SaveLastConfig(config);
            if (mBIsModelInit)
                mModel.UpdateLastFMConfig(config);
        }

        private void OnVKConfigChanged(VKConfig config)
        {
            SaveVKConfig(config);
            if (mBIsModelInit)
                mModel.UpdateVKConfig(config);
        }

        private void OnConnectorConfigChanged(PlayerConnectorConfig config)
        {
            SaveConnectorConfig(config);
            mBIsRecoveredConnectorConfig = true;
            if (mBIsModelInit)
                mModel.UpdatePlayerConnectorConfig(config);
        }

        private async void OnTrackComplite(TrackSynchronizedEvent e)
        {
            if (mBInSinchronization)
            {
                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mMainPage.DisplaySynchronizedTrack(e.trackName);
                });
            }
        }

        private async void OnNoTracksToSync()
        {
            if (mBInSinchronization)
            {
                await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mMainPage.DisplayNoTracksToSync();
                });
            }
        }

        private async void OnBadNick(BadUserNickEvent e)
        {
            StopSynchronization();
            await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await mMainPage.ShowMessage(mErrorLoader.GetString("extBadLastNick"));
                GoToPage(typeof(LastFMPage));
            });
        }

        private async void OnBadToken(BadAccessTokenEvent e)
        {
            StopSynchronization();
            await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mMainPage.UpdateAccessToken();
            });
        }
        
        private async void OnBadLibrary(TroubleWithMusicLibraryFolderEvent e)
        {
            StopSynchronization();
            await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await mMainPage.ShowMessage(mErrorLoader.GetString("extBadLibraryFolder"));
                GoToPage(typeof(DevicesInfoPage));
            });
        }

        private async void OnBadDevice(TroubleWithDeviceEvent e)
        {
            StopSynchronization();
            await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await mMainPage.ShowMessage(mErrorLoader.GetString("extBadDeviceFolder"));
                if (mMainPage.Frame != null)
                    mMainPage.Frame.Navigate(typeof(DevicesInfoPage));
            });
        }

        private async void TroubleWithRecoveringConnectorConfig()
        {
            StopSynchronization();
            await mMainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await mMainPage.ShowMessage(mErrorLoader.GetString("extTroubleWithRecoveringData"));
                GoToPage(typeof(DevicesInfoPage));
            });
        }

        private void GoToPage(Type typeOfPage)
        {
            if (mMainPage.Frame != null)
                mMainPage.Frame.Navigate(typeOfPage);
        }

    }
}
