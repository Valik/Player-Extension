using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension.ExtensionModel
{
    public class ModelController
    {
        public delegate void TrackCompliteHandler(TrackSynchronizedEvent e);
        public delegate void NoTracksToSyncHandler();
        public delegate void BadNickHandler(BadUserNickEvent e);
        public delegate void BadTokenHandler(BadAccessTokenEvent e);
        public delegate void BadLibraryHandler(TroubleWithMusicLibraryFolderEvent e);
        public delegate void BadDeviceHandler(TroubleWithDeviceEvent e);

        public event TrackCompliteHandler TrackComplite;
        public event NoTracksToSyncHandler NoTracksToSync;
        public event BadNickHandler BadNick;
        public event BadTokenHandler BadToken;
        public event BadLibraryHandler BadLibrary;
        public event BadDeviceHandler BadDevice;

        LastFMSearcher mLastFMSeacher;
        VKSearcher mVKSearcher;
        PlayerConnector mPlayerConnector;

        public ModelController()
        {
            mLastFMSeacher = new LastFMSearcher(ExtConfig.lastFMConfig);
            mVKSearcher = new VKSearcher(ExtConfig.vkConfig);
            mPlayerConnector = new PlayerConnector(ExtConfig.playerConnectorConfig);

            mLastFMSeacher.BadUserNick += OnBadUserNick;
            mVKSearcher.BadAccessToken += OnBadAccessToken;// Add event handler
            mPlayerConnector.TrackSynchronized += OnTrackSynchronized;
            mPlayerConnector.NoTracksToSyncronization += OnNoTracksToSyncronization;
            mPlayerConnector.TroubleWithDevice += OnTroubleWithDevice;// Add event handler
            mPlayerConnector.TroubleWithMusicLibraryFolder += OnTroubleWithMusicLibraryFolder;// Add event handler  
        }

        public void UpdateVKConfig(VKConfig vkConfig)
        {
            mVKSearcher.UpdateConfig(vkConfig);
        }

        public void UpdateLastFMConfig(LastFMConfig lastFMConfig)
        {
            mLastFMSeacher.UpdateConfig(lastFMConfig);
        }

        public void UpdatePlayerConnectorConfig(PlayerConnectorConfig connectorConfig)
        {
            mPlayerConnector.UpdateConfig(connectorConfig);
        }

        public static async Task<List<String>> GetAvailableDevices()
        {
            return await PlayerConnector.GetAvailableDevices();
        }

        public async Task Synchronize()
        {
            List<TrackInfo> lovedTraks = mLastFMSeacher.GetTracks();
            if (lovedTraks.Count == 0)
                return;
            List<TrackInfo> tracksWithoutUri = mVKSearcher.FillTracksURI(lovedTraks);
            if (mVKSearcher.IsBadAccessToken)
                return;
            List<TrackInfo> downloadingTraks = GetDownloadingTracks(lovedTraks, tracksWithoutUri);
            await mPlayerConnector.SendTracksToDevices(downloadingTraks);
        }

        private List<TrackInfo> GetDownloadingTracks(List<TrackInfo> lovedTraks, List<TrackInfo> tracksWithoutUri)
        {
            if (tracksWithoutUri.Count > 0)
            {
                List<TrackInfo> downloadingTraks = new List<TrackInfo>();
                foreach (TrackInfo curTrack in lovedTraks)
                {
                    if (!tracksWithoutUri.Contains(curTrack))
                        downloadingTraks.Add(curTrack);
                }
                return downloadingTraks;
            }
            return lovedTraks;
        }

        void OnTrackSynchronized(TrackSynchronizedEvent e)
        {
            if (TrackComplite != null)
                TrackComplite.Invoke(e);
        }

        void OnNoTracksToSyncronization()
        {
            if (NoTracksToSync != null)
                NoTracksToSync.Invoke();
        }

        void OnBadUserNick(BadUserNickEvent e)
        {
            if (BadNick != null)
                BadNick.Invoke(e);
        }

        void OnBadAccessToken(BadAccessTokenEvent e)
        {
            if (BadToken != null)
                BadToken.Invoke(e);
        }
        
        void OnTroubleWithMusicLibraryFolder(TroubleWithMusicLibraryFolderEvent e)
        {
            if (BadLibrary != null)
                BadLibrary.Invoke(e);
        }

        void OnTroubleWithDevice(TroubleWithDeviceEvent e)
        {
            if (BadDevice != null)
                BadDevice.Invoke(e);
        }
    }
}
