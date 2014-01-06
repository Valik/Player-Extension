using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension.Common
{
    [Flags]
    public enum LastFMSearchType
    {
        LOVED_TRACKS = 2,
        LAST_LISTENED_TRACKS = 4
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LastFMConfig
    {
        private String mUserName;
        private LastFMSearchType mMaskSearchType;
        private int mLimit;

        public LastFMConfig()
        { }

        public LastFMConfig(String userName, LastFMSearchType maskSearchType, int limit)
        { 
            mUserName = userName;
            mMaskSearchType = maskSearchType;
            mLimit = limit;
        }

        public static string API_KEY
        { get { return "45915d35bd721b0d58b6e1762770d2fa"; } }

        public static string WS_ADDRESS
        { get { return "http://ws.audioscrobbler.com/2.0/"; } }
        /// <summary>
        /// Serialized last username
        /// </summary>
        [JsonProperty]
        public String userName 
        {
            get { return mUserName; }
            set { mUserName = value; }
        }
        /// <summary>
        /// Serialized last search type
        /// </summary>
        [JsonProperty]
        public LastFMSearchType maskSearchType
        {
            get { return mMaskSearchType; }
            set { mMaskSearchType = value; }
        }
        /// <summary>
        /// Serialized last limit tracks to search
        /// </summary>
        [JsonProperty]
        public int limit
        {
            get { return mLimit; }
            set { mLimit = value; }
        }
    }
}
