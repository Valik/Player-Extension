using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension
{
    public static class ExtConfig
    {
        public delegate void LastFMConfigChangedHandler(LastFMConfig config);
        public delegate void VKConfigChangedHandler(VKConfig config);
        public delegate void ConnectorConfigChangedHandler(PlayerConnectorConfig config);

        public static event LastFMConfigChangedHandler LastFMConfigChanged;
        public static event VKConfigChangedHandler VKConfigChanged;
        public static event ConnectorConfigChangedHandler ConnectorConfigChanged;

        static VKConfig mVKConfig;
        static LastFMConfig mLastFMConfig;
        static PlayerConnectorConfig mPlayerConnectorConfig;
       
        public static LastFMConfig lastFMConfig
        {
            get { return mLastFMConfig; }
            internal set 
            { 
                mLastFMConfig = value;
                if (LastFMConfigChanged != null)
                    LastFMConfigChanged.Invoke(mLastFMConfig);
            }
        }

        public static VKConfig vkConfig
        {
            get { return mVKConfig; }
            internal set 
            { 
                mVKConfig = value;
                if (VKConfigChanged != null)
                    VKConfigChanged.Invoke(mVKConfig);
            }
        }

        public static PlayerConnectorConfig playerConnectorConfig
        {
            get { return mPlayerConnectorConfig; }
            internal set 
            { 
                mPlayerConnectorConfig = value;
                if (ConnectorConfigChanged != null)
                    ConnectorConfigChanged.Invoke(mPlayerConnectorConfig);
            }
        }
    }
}
