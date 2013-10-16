using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayerExtension.ExtensionModel
{
    public class TrackSynchronizedEvent
    {
        private String mTrackName;


        internal TrackSynchronizedEvent(string trackName)
        {
            mTrackName = trackName;
        } 
        
        public String trackName
        {
            get { return mTrackName; }
        }
    }
}
