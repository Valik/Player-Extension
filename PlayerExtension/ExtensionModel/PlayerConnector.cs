using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Portable;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;

namespace PlayerExtension.ExtensionModel
{
    internal class PlayerConnector
    {
        public delegate void TrackSynchronizedHandler(TrackSynchronizedEvent e);
        public delegate void NoTracksToSyncronizationHandler();
        public delegate void TroubleWithDeviceHandler(TroubleWithDeviceEvent e);
        public delegate void TroubleWithMusicLibraryFolderHandler(TroubleWithMusicLibraryFolderEvent e);

        public event TrackSynchronizedHandler TrackSynchronized;
        public event NoTracksToSyncronizationHandler NoTracksToSyncronization;
        public event TroubleWithDeviceHandler TroubleWithDevice;
        public event TroubleWithMusicLibraryFolderHandler TroubleWithMusicLibraryFolder;
        
        private bool mBIsNoTracksToSync;
        private Downloader mDownloader;
        protected PlayerConnectorConfig mConfig;
        

        public PlayerConnector(PlayerConnectorConfig config)
        {
            mConfig = config;
            mDownloader = new Downloader(this);
        }

        private class Downloader
        {
            private PlayerConnector mConnector;
            public Downloader(PlayerConnector connector)
            { mConnector = connector; }

            public async Task DownloadTracks(List<TrackInfo> tracks)
            {
                mConnector.mBIsNoTracksToSync = true;

                foreach (TrackInfo curTrack in tracks)
                    if (!await DownloadTrack(curTrack))
                        return;

                if (mConnector.mBIsNoTracksToSync && mConnector.NoTracksToSyncronization != null)
                    mConnector.NoTracksToSyncronization.BeginInvoke(null, null);
                    
            }

            private async Task<bool> DownloadTrack(TrackInfo track)
            {
                if (track.trackURI == null
                    || mConnector.mConfig.selectedDevices == null
                    || mConnector.mConfig.musicLibraryFolder == null)
                    throw new InvalidOperationException("Destination folders or musicLibraryFolder or track URI == null");

                try
                {
                    StorageFile destinationFile = await mConnector.mConfig.musicLibraryFolder.CreateFileAsync(track.trackInfoForDownloader, CreationCollisionOption.FailIfExists);
                    BackgroundDownloader downloader = new BackgroundDownloader();
                    DownloadOperation download = downloader.CreateDownload(track.trackURI, destinationFile);
                    await download.StartAsync();
                }
                catch (Exception)
                { }

                StorageFile downloadedFile = await GetDownloadedFile(track);
                if (downloadedFile == null)
                    return false;

                foreach (DeviceInfo curDevice in mConnector.mConfig.selectedDevices)
                {
                    try
                    {
                        StorageFile result = await downloadedFile.CopyAsync(curDevice.deviceFolder, downloadedFile.Name, NameCollisionOption.FailIfExists);
                        if (mConnector.TrackSynchronized != null)
                            mConnector.TrackSynchronized.BeginInvoke(new TrackSynchronizedEvent(track.trackInfoForDisplay), null, null);
                        mConnector.mBIsNoTracksToSync = false;
                    }
                    catch (Exception)
                    { }

                    if (await CheckDownloadedFile(downloadedFile.Name, curDevice) == null)
                        return false;
                }

                return true;
            }

            private async Task<StorageFile> GetDownloadedFile(TrackInfo track)
            {
                StorageFile downloadingFile = null;
                try
                {
                    downloadingFile = await mConnector.mConfig.musicLibraryFolder.CreateFileAsync(track.trackInfoForDownloader, CreationCollisionOption.OpenIfExists);
                }
                catch (Exception)
                {
                    if (mConnector.TroubleWithMusicLibraryFolder != null)
                        mConnector.TroubleWithMusicLibraryFolder.BeginInvoke(new TroubleWithMusicLibraryFolderEvent("files in music library folder is not available.", mConnector.mConfig.musicLibraryFolder.Path), null, null);
                }
                return downloadingFile;
            }

            private async Task<StorageFile> CheckDownloadedFile(String fileName, DeviceInfo curDevice)
            {
                StorageFile file = null;
                try
                {
                    file = await curDevice.deviceFolder.GetFileAsync(fileName);
                }
                catch (Exception)
                {
                    if (mConnector.TroubleWithDevice != null)
                        mConnector.TroubleWithDevice.BeginInvoke(new TroubleWithDeviceEvent("Trouble with device", curDevice.deviceName, curDevice.deviceFolder.Name), null, null);
                }
                return file;
            }
        }

        public void UpdateConfig(PlayerConnectorConfig config)
        { mConfig = config; }

        public static async Task<List<String>> GetAvailableDevices()
        {
            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(StorageDevice.GetDeviceSelector());
            List<String> result = new List<string>();

            foreach (DeviceInformation deviceInfo in deviceInfoCollection)
            {
                result.Add(deviceInfo.Name);
            }

            return result;
        }

        public async Task SendTracksToDevices(List<TrackInfo> tracks)
        {
            await mDownloader.DownloadTracks(tracks);
        }
    }
}
