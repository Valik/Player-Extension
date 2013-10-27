using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace PlayerExtension.ExtensionModel
{
    internal class LastFMSearcher
    {
        public delegate void BadUserNickHandler(BadUserNickEvent e);
        public event BadUserNickHandler BadUserNick;

        private string mLovedTracksMethod = "user.getlovedtracks";
        private string mRecentTracksMethod = "user.getrecenttracks";

        private LastFMConfig mConfig;


        public LastFMSearcher(LastFMConfig config)
        {
            mConfig = config;
        }

        public void UpdateConfig(LastFMConfig config)
        { mConfig = config; }

        public IEnumerable<TrackInfo> GetTracks()
        {
            switch (mConfig.maskSearchType)
            {
                case LastFMSearchType.LOVED_TRACKS:
                    {
                        return GetLovedTracks();
                    }
                case LastFMSearchType.LAST_LISTENED_TRACKS:
                    {
                        return GetLastListenedTracks();
                    }
                case LastFMSearchType.LOVED_TRACKS | LastFMSearchType.LAST_LISTENED_TRACKS:
                    {
                        List<TrackInfo> result = new List<TrackInfo>();
                        result.AddRange(GetLovedTracks());
                        result.AddRange(GetLastListenedTracks());
                        return result;
                    }
                default:
                    {
                        return new List<TrackInfo>();
                    }
            }
        }

        private List<TrackInfo> GetLovedTracks()
        {
            string responseURL = LastFMConfig.WS_ADDRESS + 
                                "?method=" + mLovedTracksMethod + 
                                "&user=" + mConfig.userName + 
                                "&api_key=" + LastFMConfig.API_KEY;

            return GetTracksByUrl(responseURL);
        }

        private List<TrackInfo> GetLastListenedTracks()
        {
            int limit = mConfig.limit;
            if(limit < 1 || limit > 200)
                limit = 20;
            string responseURL = LastFMConfig.WS_ADDRESS +
                                "?method=" + mRecentTracksMethod +
                                "&user=" + mConfig.userName +
                                "&api_key=" + LastFMConfig.API_KEY +
                                "&limit=" + limit;

            return GetTracksByUrl(responseURL);
        }

        private List<TrackInfo> GetTracksByUrl(string _responseURL)
        {
            XmlReader _xmlReader = null;
            XDocument _requestDoc = null;
            List<TrackInfo> _lovedTracks = new List<TrackInfo>();

            try
            {
                _xmlReader = XmlReader.Create(_responseURL);
                _requestDoc = XDocument.Load(_xmlReader);
            }
            catch (WebException)
            {
                BadUserNick.BeginInvoke(new BadUserNickEvent(mConfig.userName), null, null);
                return _lovedTracks;
            }

            IEnumerable<XElement> _errorCodeNodes = _requestDoc.Descendants("error");
            if (_errorCodeNodes.Count() != 0)
            {
                if (BadUserNick != null)
                    BadUserNick.BeginInvoke(new BadUserNickEvent(mConfig.userName), null, null);
                return _lovedTracks;
            }

            IEnumerable<XElement> _tracksElements = _requestDoc.Descendants("track");

            XElement _artist;
            XElement _trackName;

            foreach (XElement _curNode in _tracksElements)
            {
                if (_curNode.NodeType == XmlNodeType.Element)
                {
                    _artist = _curNode.Element("artist");
                    XElement _nameArtist = _artist.Element("name");
                    if (_nameArtist != null)
                        _artist = _nameArtist;

                    _trackName = _curNode.Element("name");

                    _lovedTracks.Add(new TrackInfo(_artist.Value, _trackName.Value));
                }
            }

            return _lovedTracks;
        }



    }
}
