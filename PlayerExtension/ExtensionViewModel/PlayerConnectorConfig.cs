using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PlayerExtension
{
    public class PlayerConnectorConfig
    {
        private List<DeviceInfo> mSelectedDevices;
        private StorageFolder mMusicLibraryFolder;

        public PlayerConnectorConfig(List<DeviceInfo> selectedDevices, StorageFolder musicLibraryFolder)
        {
            mSelectedDevices = selectedDevices;
            mMusicLibraryFolder = musicLibraryFolder;
        }

        public static PlayerConnectorConfig Init(List<DeviceInfo> selectedDevices, StorageFolder musicLibraryFolder)
        { return new PlayerConnectorConfig(selectedDevices, musicLibraryFolder); }

        public List<DeviceInfo> selectedDevices
        {
            get { return mSelectedDevices; }
        }

        public StorageFolder musicLibraryFolder
        {
            get { return mMusicLibraryFolder; }
        }
    }

    public class DeviceInfo
    {
        private String mDeviceName;
        private StorageFolder mDeviceFolder;

        public DeviceInfo(string deviceName, StorageFolder deviceFolder)
        {
            mDeviceName = deviceName;
            mDeviceFolder = deviceFolder;
        }
        
        public String deviceName
        {
            get { return mDeviceName; }
        }
        
        public StorageFolder deviceFolder
        {
            get { return mDeviceFolder; }
        }
    }
}
