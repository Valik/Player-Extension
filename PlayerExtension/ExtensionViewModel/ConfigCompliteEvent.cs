using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayerExtension
{
    public class ConfigCompliteEvent
    {
        private PlayerExtensionPage mMainPage;

        public PlayerExtensionPage mainPage
        {
            get { return mMainPage; }
        }

        public ConfigCompliteEvent(PlayerExtensionPage page)
        {
            mMainPage = page;
        }
    }
}
