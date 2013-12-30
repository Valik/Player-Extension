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

        public FirstPage GetFirstPage()
        {
            bool lastRes = TryToRecoverLastConfig();
            bool vkRes = TryToRecoverVKConfig();

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

        private void SaveLastConfig(LastFMConfig config)
        {
            ApplicationDataCompositeValue savingAppConfig = new ApplicationDataCompositeValue();

            savingAppConfig.Add("lastUserName", config.userName);
            savingAppConfig.Add("lastSearchType", (int)config.maskSearchType);
            savingAppConfig.Add("lastLimit", config.limit);

            SaveConfig(LastConfigName, savingAppConfig);
        }

        private void SaveVKConfig(VKConfig config)
        {
            ApplicationDataCompositeValue savingAppConfig = new ApplicationDataCompositeValue();

            savingAppConfig.Add("vkAccessToken", config.ACCESS_TOKEN);

            SaveConfig(VkConfigName, savingAppConfig);
        }

        private void SaveConnectorConfig(PlayerConnectorConfig config)
        {
            ApplicationDataCompositeValue savingAppConfig = new ApplicationDataCompositeValue();

            savingAppConfig.Add("musicLibraryFolder", config.musicLibraryFolder.Path);

            SaveConfig(MusicLibraryConfigName, savingAppConfig);

            if (config.IsDevicesSelected)
            {
                ApplicationDataCompositeValue devicesInfo = new ApplicationDataCompositeValue();
                foreach (var curDevice in config.selectedDevices)
                {
                    devicesInfo.Add(curDevice.deviceName, curDevice.deviceFolder.Name);
                }

                SaveConfig(DevicesInfoConfigName, devicesInfo);
            }
        }

        private void SaveConfig(string configName, ApplicationDataCompositeValue config)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            var container = localSettings.CreateContainer(configName, Windows.Storage.ApplicationDataCreateDisposition.Always);
            if (localSettings.Containers.ContainsKey(configName))
            {
                localSettings.Values[configName] = config;
            }
        }

        private bool TryToRecoverLastConfig()
        {
            ApplicationDataContainer localContainer = ApplicationData.Current.LocalSettings;
            if (!localContainer.Values.ContainsKey("lastConfig"))
                return false;

            ApplicationDataCompositeValue lastConfigCont = localContainer.Values["lastConfig"] as ApplicationDataCompositeValue;

            LastFMConfig lastFMConfig = new LastFMConfig((String)lastConfigCont["lastUserName"],
                                                    (LastFMSearchType)((int)lastConfigCont["lastSearchType"]),
                                                    (int)lastConfigCont["lastLimit"]);

            ExtConfig.lastFMConfig = lastFMConfig;
            return true;
        }

        private bool TryToRecoverVKConfig()
        {
            ApplicationDataContainer localContainer = ApplicationData.Current.LocalSettings;
            if (localContainer.Values["vkConfig"] == null)
                return false;

            ApplicationDataCompositeValue vkConfigCont = (ApplicationDataCompositeValue)localContainer.Values["vkConfig"];
            VKConfig vkConfig = new VKConfig((String)vkConfigCont["vkAccessToken"]);
            ExtConfig.vkConfig = vkConfig;
            return true;
        }

        private async Task TryToRecoverConnectorConfig()
        {
            ApplicationDataContainer localContainer = ApplicationData.Current.LocalSettings;
            if (!localContainer.Values.ContainsKey("musicLibraryConfig") &&
                !localContainer.Values.ContainsKey("devicesInfo"))
            {
                mBIsRecoveredConnectorConfig = false;
                TroubleWithRecoveringConnectorConfig();
                return;
            }

            ApplicationDataCompositeValue musicLibraryConfigCont = localContainer.Values["musicLibraryConfig"] as ApplicationDataCompositeValue;
            StorageFolder musicLibraryFolder = null;
            try
            {
                musicLibraryFolder = await StorageFolder.GetFolderFromPathAsync(musicLibraryConfigCont["musicLibraryFolder"] as string);
            }
            catch (Exception)
            {
                mBIsRecoveredConnectorConfig = false;
                OnBadLibrary(new TroubleWithMusicLibraryFolderEvent("Что-то не так с папкой для закачки треков с ласта. Выбери другую папку.", ""));
                return;
            }

            ApplicationDataCompositeValue devicesInfo = localContainer.Values["devicesInfo"] as ApplicationDataCompositeValue;
            List<DeviceInfo> devices = null;
            if (devicesInfo != null)
            {
                devices = await GetDevicesInfo(devicesInfo);
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

        private async Task<List<DeviceInfo>> GetDevicesInfo(ApplicationDataCompositeValue devicesInfo)
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();

            foreach (var curDevice in devicesInfo)
            {
                String deviceName = curDevice.Key;
                String folderName = (String)curDevice.Value;

                try
                {
                    StorageFolder deviceFolder = await GetDeviceFolder(deviceName, folderName);
                    if (deviceFolder == null)
                    {
                        //TODO: Оповестить пользователя, устройства не доступны
                        return null;
                    }

                    devices.Add(new DeviceInfo(curDevice.Key, deviceFolder));
                }
                catch (Exception)
                {
                    //TODO: Оповестить пользователя, устройства не доступны
                    return null;
                }
            }
            return devices;
        }

        private async Task<StorageFolder> GetDeviceFolder(String deviceName, String folderName)
        {
            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(StorageDevice.GetDeviceSelector());
            if (deviceInfoCollection.Count > 0)
                foreach (DeviceInformation curDeviceInfo in deviceInfoCollection)
                    if (curDeviceInfo.Name == deviceName)
                    {
                        StorageFolder devStorage = StorageDevice.FromId(curDeviceInfo.Id);
                        return await GetDestinationFolder(devStorage, folderName);
                    }
            return null;
        }

        private async Task<StorageFolder> GetDestinationFolder(StorageFolder devStorage, String folderName)
        {
            if (devStorage.Name == folderName)
                return devStorage;

            var subFolders = await devStorage.GetFoldersAsync();

            foreach (var curFolder in subFolders)
                if (curFolder.Name == folderName)
                    return curFolder;
                else
                {
                    var result = await GetDestinationFolder(curFolder, folderName);
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
