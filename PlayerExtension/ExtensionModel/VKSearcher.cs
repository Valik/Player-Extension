using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace PlayerExtension.ExtensionModel
{
    internal class VKSearcher
    {
        private static readonly int mLimitAttemptsSearch = 2;
        private static readonly int mDelay = 500;

        private VKConfig mConfig;
        private String mVKAPIMethod = "audio.search.xml";
        private int mNumberOfAttemptsSearch = 0;

        private bool mBBadAccessToken = false;

        public delegate void BadAccessTokenHandler(BadAccessTokenEvent e);
        public event BadAccessTokenHandler BadAccessToken;

        public VKSearcher(VKConfig config)
        { mConfig = config; }

        public void UpdateConfig(VKConfig config)
        { mConfig = config; }

        public bool IsBadAccessToken
        {
            get { return mBBadAccessToken; }
        }

        public IEnumerable<TrackInfo> FillTracksURI(IEnumerable<TrackInfo> tracks)
        {
            List<TrackInfo> failedtracks = new List<TrackInfo>();
            mBBadAccessToken = false;

            foreach (TrackInfo curTrack in tracks)
            {
                mNumberOfAttemptsSearch = 0;
                if (!FillURIForTrack(curTrack))
                    failedtracks.Add(curTrack);
                if (mBBadAccessToken)
                    break;
                Task delay = Task.Delay(mDelay);// Ждем, из-за ограничения, не больше 3х запросов в секунду к vk.com
                delay.Wait();
            }

            return failedtracks;
        }

        private bool FillURIForTrack(TrackInfo curTrack)
        {
            if (mConfig.ACCESS_TOKEN == null)
                return false; //TODO: Создать Сообщение об ошибке.

            string _responseURL =  VKConfig.VK_ADDRESS + mVKAPIMethod +
                                    "?q=" + curTrack.trackInfoForRequest +
                                    "&access_token=" + mConfig.ACCESS_TOKEN;

            try
            {
                XmlReader _xmlReader = XmlReader.Create(_responseURL);

                XDocument _requestDoc = XDocument.Load(_xmlReader);

                IEnumerable<XElement> _errorCodeNodes = _requestDoc.Descendants("error_code");

                if (CheckErrorCode(_errorCodeNodes))
                {
                    if (BadAccessToken != null)
                    {
                        BadAccessToken.BeginInvoke(new BadAccessTokenEvent(mConfig.ACCESS_TOKEN), null, null);
                        mBBadAccessToken = true;
                    }
                    return true;
                }
                    
                IEnumerable<XElement> urlElements = _requestDoc.Descendants("url");
                XElement _firstTrackURL = null;

                if(urlElements.Count() > 0)
                {
                    _firstTrackURL = urlElements.First();
                    curTrack.trackURI = new Uri(_firstTrackURL.Value);
                    return true;
                }
                else
                {
                    mNumberOfAttemptsSearch++;
                    Task delay = Task.Delay(mDelay);
                    delay.Wait();
                    while (mNumberOfAttemptsSearch < mLimitAttemptsSearch && FillURIForTrack(curTrack) == false)
                        return false;
                }
            }
            catch (WebException)
            {
                //Console.WriteLine("VKSearcher: Problem in Network; exeption: " + exp.Message);
                return false;
            }
            catch (NotSupportedException)
            {
                //Console.WriteLine("VKSearcher: Not Supported Exception: " + exp.Message);
                return false;
            }

            return false;
        }

        private bool CheckErrorCode(IEnumerable<XElement> errorCodeNodes)
        {
            if (errorCodeNodes.Count() == 0)
                return false;

            XElement errorElement = errorCodeNodes.First();
            String errorCode = errorElement.Value;

            switch (errorCode)
            {
                case "5":
                    {
                        return true;
                    }
                case "1":
                case "2":
                case "3":
                case "4":
                    {
                        //Console.WriteLine("VKSearcher: Unknow error code");
                        return false;
                    }
                default:
                    {
                        //Console.WriteLine("VKSearcher: Unknow error code");
                        return false;
                    }
            }
        }
    }
}
