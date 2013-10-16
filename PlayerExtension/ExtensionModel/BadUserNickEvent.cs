using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayerExtension.ExtensionModel
{
    public class BadUserNickEvent
    {
        private String mUserNick;

        public BadUserNickEvent(String userNick)
        {
            mUserNick = userNick;
        }

        public String userNick
        {
            get { return mUserNick; }
        }
    }
}
