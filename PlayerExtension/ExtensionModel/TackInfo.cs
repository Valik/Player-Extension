using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayerExtension.ExtensionModel
{
    public class TrackInfo
    {
        private String mArtist = "";
        private String mTrackName = "";

        public TrackInfo(String artist, String trackName)
        {
            mArtist = artist;
            mTrackName = trackName;
        }

        public String artist
        { get { return mArtist; } }

        public String trackName
        { get { return mTrackName; } }

        public String trackInfoForDisplay
        {
            get { return mArtist + " - " + mTrackName; }
        }

        public String trackInfoForRequest
        {
            get { return (mArtist + " - " + mTrackName).Replace(" ", "%20"); }
        }

        public String trackInfoForDownloader
        {
            get 
            {
                String result = (mArtist + " - " + mTrackName).Trim();
                String query = new String(new char[] 
                { '[', '\\', '\\', '\\', '/', ':', '*', '?', '\\', '"', '<', '>', '|', ']' });

                result = Regex.Replace(result, query, "");

                if (result == "")
                    result = "unknown - unknown";

                return result + ".mp3"; 
            }
        }

        public Uri trackURI
        { get; set; }

        public override int GetHashCode()
        {
            return (artist + trackName).GetHashCode();
        }
    }
}
