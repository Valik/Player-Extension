using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayerExtension.ExtensionModel
{
    public class BadAccessTokenEvent
    {
        private String mAccessToken;

        public BadAccessTokenEvent(String accessToken)
        {
            mAccessToken = accessToken;
        }
        
        public String accessToken
        {
            get { return mAccessToken; }
        }
    }
}
