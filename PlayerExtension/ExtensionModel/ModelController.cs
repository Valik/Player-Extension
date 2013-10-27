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

        private LastFMSearcher mLastFMSeacher;
        private VKSearcher mVKSearcher;
        private PlayerConnector mPlayerConnector;

        private List<TrackInfo> mDownloadingTracks = new List<TrackInfo>();

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
            IEnumerable<TrackInfo> lovedTraks = mLastFMSeacher.GetTracks();
            if (!lovedTraks.Any())
                return;

            IEnumerable<TrackInfo> newTracks = GetNewTracks(lovedTraks);

            if (!newTracks.Any())
            {
                OnNoTracksToSyncronization();
                return;
            }

            IEnumerable<TrackInfo> tracksWithoutUri = mVKSearcher.FillTracksURI(newTracks);
            if (mVKSearcher.IsBadAccessToken)
                return;

            IEnumerable<TrackInfo> downloadedTraks = Except(newTracks, tracksWithoutUri);

            await mPlayerConnector.SendTracksToDevices(downloadedTraks);

            SaveDownloadedTracks(downloadedTraks);
        }



        private IEnumerable<TrackInfo> GetNewTracks(IEnumerable<TrackInfo> lovedTraks)
        {
            if (mDownloadingTracks != null)
            {
                var newTracks = Except(lovedTraks, mDownloadingTracks);
                return newTracks;
            }
            return lovedTraks;
        }

        private void SaveDownloadedTracks(IEnumerable<TrackInfo> downloadingTraks)
        {
            IEnumerable<TrackInfo> newDownloadedTracks = Except(downloadingTraks, mDownloadingTracks);
            mDownloadingTracks.AddRange(newDownloadedTracks);
        }

        private static IEnumerable<TrackInfo> Except(IEnumerable<TrackInfo> @this, IEnumerable<TrackInfo> that)
        {
            Func<TrackInfo, TrackInfo, bool> comparer = (x, y) =>
            {
                return x.artist == y.artist &&
                       x.trackName == y.trackName;
            };

            if (that.Any())
            {
                IEnumerable<TrackInfo> exceptTraks = @this.Except(that, new CustomEqualityComparer<TrackInfo>(comparer)).ToList();
                return exceptTraks;
            }
            else
            {
                return @this;
            }
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
