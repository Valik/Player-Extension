using PlayerExtension.Common.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PlayerExtension.Common
{
    public class PlayerConnectorConfig
    {
        private bool mIsDevicesSelected;

        private List<DeviceInfo> mSelectedDevices;
        private StorageFolder mMusicLibraryFolder;

        public PlayerConnectorConfig(List<DeviceInfo> selectedDevices, StorageFolder musicLibraryFolder)
        {
            mSelectedDevices = selectedDevices;
            mMusicLibraryFolder = musicLibraryFolder;

            mIsDevicesSelected = true;
        }

        public PlayerConnectorConfig(StorageFolder musicLibraryFolder)
        {
            mMusicLibraryFolder = musicLibraryFolder;
            mIsDevicesSelected = false;
        }

        public List<DeviceInfo> selectedDevices
        {
            get { return mSelectedDevices; }
        }

        public StorageFolder musicLibraryFolder
        {
            get { return mMusicLibraryFolder; }
        }

        public bool IsDevicesSelected
        {
            get { return mIsDevicesSelected; }
        }

        public StoragesConfig storagesConfig
        {
            get
            {
                var config = new StoragesConfig();
                config.musicLibraryStorage = mMusicLibraryFolder.Path;
                config.deviceSelected = mIsDevicesSelected;

                if (mIsDevicesSelected && mSelectedDevices != null)
                {
                    config.deviceStorages = mSelectedDevices.Select(
                    d => new StorageDeviceConfig
                    {
                        name = d.deviceName,
                        deviceFolderName = d.deviceFolder.Name,
                        deviceFolderPath = d.deviceFolder.Path
                    })
                    .ToList();
                }
                else
                {
                    config.deviceStorages = new List<StorageDeviceConfig>();
                }

                return config;
            }
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
