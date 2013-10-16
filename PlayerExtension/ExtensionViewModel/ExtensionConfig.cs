using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension
{
    public class ExtensionConfig
    {
        VKConfig mVKConfig;
        LastFMConfig mLastFMConfig;
        PlayerConnectorConfig mPlayerConnectorConfig;

        internal ExtensionConfig()
        { }

        internal ExtensionConfig(VKConfig vkConfig, LastFMConfig lastFMConfig, PlayerConnectorConfig connectorConfig)
        {
            mVKConfig = vkConfig;
            mLastFMConfig = lastFMConfig;
            mPlayerConnectorConfig = connectorConfig;
        } 

        public VKConfig vkConfig
        {
            get { return mVKConfig; }
            internal set { mVKConfig = value; }
        }
        
        public LastFMConfig lastFMConfig
        {
            get { return mLastFMConfig; }
            internal set { mLastFMConfig = value; }
        }
        
        public PlayerConnectorConfig playerConnectorConfig
        {
            get { return mPlayerConnectorConfig; }
            internal set { mPlayerConnectorConfig = value; }
        }
    }
}
