using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic.Entity
{
    public class ServerResponse
    {
        [JsonProperty(PropertyName = "code")]
        public ResponseCode Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public JToken Data { get; set; }
    }
}