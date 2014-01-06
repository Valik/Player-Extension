using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension.Common.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StoragesConfig
    {
        [JsonProperty]
        public string musicLibraryStorage { get; set; }
        [JsonProperty]
        public List<StorageDeviceConfig> deviceStorages { get; set; }
    }
}
