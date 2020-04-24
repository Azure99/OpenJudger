using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic.Entity
{
    /// <summary>
    /// Web后端响应
    /// </summary>
    public class ServerResponse
    {
        /// <summary>
        /// 响应码
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public ResponseCode Code { get; set; }

        /// <summary>
        /// 响应信息
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public JToken Data { get; set; }
    }
}