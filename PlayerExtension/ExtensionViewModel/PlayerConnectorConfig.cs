﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PlayerExtension
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
