using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerExtension.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VKConfig
    {
        public string mAccessToken;

        public VKConfig()
        { }

        public VKConfig(string accessToken)
        { mAccessToken = accessToken; }

        public static String API_ID
        { get { return "3432583"; } }
        
        [JsonProperty]
        public String ACCESS_TOKEN
        {
            get { return mAccessToken; }
            set { mAccessToken = value; }
        }

        public static string VK_ADDRESS
        { get { return "https://api.vk.com/method/"; } }

        public static string VK_AUTHADDRESS
        { get { return "http://api.vkontakte.ru/oauth/authorize?client_id={0}&scope={1}&display=popup&response_type=token"; } }
    }
}
