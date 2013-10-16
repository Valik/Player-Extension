using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayerExtension.ExtensionModel
{
    public class TroubleWithDeviceEvent
    {
        private String mMessage;
        private String mDeviceName;
        private String mDeviceFolder;

        public TroubleWithDeviceEvent(string message, string deviceName, string deviceFolder)
        {
            mMessage = message;
            mDeviceName = deviceName;
            mDeviceFolder = deviceFolder;
        }

        public String message
        {
            get { return mMessage; }
        }

        public String deviceName
        { get { return mDeviceName; } }

        public String deviceFolder
        { get { return mDeviceFolder; } }
    }
}
