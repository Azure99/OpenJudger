using System;
using Newtonsoft.Json;

namespace Judger.Adapter.SDNUOJ.Entity
{
    [Serializable]
    public class ServerMessage
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        public bool IsSuccess => Status == "success";
    }
}