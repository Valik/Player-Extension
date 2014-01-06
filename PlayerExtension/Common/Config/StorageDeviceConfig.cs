using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension.Common.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StorageDeviceConfig
    {
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public string deviceFolderName { get; set; }
        [JsonProperty]
        public string deviceFolderPath { get; set; }
    }
}
