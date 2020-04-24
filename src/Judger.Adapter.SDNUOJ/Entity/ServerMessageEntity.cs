﻿using System;
using Newtonsoft.Json;

namespace Judger.Adapter.SDNUOJ.Entity
{
    [Serializable]
    public class ServerMessageEntity
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccess => Status == "success";
    }
}