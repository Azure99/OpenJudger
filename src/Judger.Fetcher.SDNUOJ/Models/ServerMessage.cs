using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Judger.Fetcher.SDNUOJ.Models
{
    [Serializable]
    public class ServerMessage
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        public bool IsSuccessful
        {
            get
            {
                return Status == "success";
            }
        }
    }
}
