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
using System.Diagnostics;

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

            public async Task DownloadTracks(IEnumerable<TrackInfo> tracks)
            {
                mConnector.mBIsNoTracksToSync = true;

                var taskList = new List<Task<DownloadingResult>>();

                foreach (TrackInfo curTrack in tracks)
                {
                    taskList.Add(DownloadTrack(curTrack));
                }

                while (taskList.Count > 0)
                {
                    Task<DownloadingResult> complitedTask = await Task.WhenAny(taskList);

                    var downloadingResult = await complitedTask;
                    if (!downloadingResult.IsComplite)
                    {
                        Debug.WriteLine("Failed download track: {0}", downloadingResult.TrackInfo.trackInfoForDisplay);
                    }
                    else
                    {
                        mConnector.OnTrackSynchronized(downloadingResult.TrackInfo);
                        mConnector.mBIsNoTracksToSync = false;
                    }

                    taskList.Remove(complitedTask);
                }

                if (mConnector.mBIsNoTracksToSync && mConnector.NoTracksToSyncronization != null)
                    mConnector.NoTracksToSyncronization.BeginInvoke(null, null);
            }

            private Task<DownloadingResult> DownloadTrack(TrackInfo track)
            {
                return Task.Run(async () =>
                {
                    if (track.trackURI == null
                        || mConnector.mConfig.musicLibraryFolder == null)
                        throw new InvalidOperationException("MusicLibraryFolder or track URI == null");

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
                    {
                        return new DownloadingResult
                        {
                            TrackInfo = track,
                            IsComplite = false
                        };
                    }

                    if (mConnector.mConfig.IsDevicesSelected)
                    {
                        var isCopied = await CopyFileToDevice(downloadedFile);
                        return new DownloadingResult
                        {
                            TrackInfo = track,
                            IsComplite = isCopied
                        };
                    }

                    return new DownloadingResult
                    {
                        TrackInfo = track,
                        IsComplite = true
                    };
                });
            }

            private async Task<bool> CopyFileToDevice(StorageFile downloadedFile)
            {
                foreach (DeviceInfo curDevice in mConnector.mConfig.selectedDevices)
                {
                    try
                    {
                        StorageFile result = await downloadedFile.CopyAsync(curDevice.deviceFolder, downloadedFile.Name, NameCollisionOption.FailIfExists);
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

        public async Task SendTracksToDevices(IEnumerable<TrackInfo> tracks)
        {
            await mDownloader.DownloadTracks(tracks);
        }

        private void OnTrackSynchronized(TrackInfo track)
        {
            if (TrackSynchronized != null)
                TrackSynchronized.BeginInvoke(new TrackSynchronizedEvent(track.trackInfoForDisplay), null, null);
        }
    }
}
