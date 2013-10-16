using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension
{
    [Flags]
    public enum LastFMSearchType
    {
        LOVED_TRACKS = 2,
        LAST_LISTENED_TRACKS = 4
    }

    public class LastFMConfig
    {
        private String mUserName;
        private LastFMSearchType mMaskSearchType;
        private int mLimit;

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
        public String userName
        { get { return mUserName; } }
        /// <summary>
        /// Serialized last search type
        /// </summary>
        public LastFMSearchType maskSearchType
        { get { return mMaskSearchType; } }
        /// <summary>
        /// Serialized last limit tracks to search
        /// </summary>
        public int limit
        { get { return mLimit; } }
    }
}
