using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Judger.Utils
{
    /// <summary>
    /// 简单的JSON序列化/反序列化辅助类
    /// </summary>
    public static class Json
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        static Json()
        {
            Serializer.Formatting = Formatting.Indented;
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="type">类型</param>
        /// <returns>序列化后的Json字符串</returns>
        public static string Serialize(object obj, Type type)
        {
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                Serializer.Serialize(writer, obj, type);
                return writer.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>序列化后的Json字符串</returns>
        public static string Serialize<T>(T obj)
        {
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                Serializer.Serialize(writer, obj, typeof(T));
                return writer.ToString();
            }
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="jsonObject">对象的Json字符串</param>
        /// <param name="type">类型</param>
        /// <returns>反序列化的对象</returns>
        public static object DeSerialize(string jsonObject, Type type)
        {
            using (StringReader reader = new StringReader(jsonObject))
            {
                return Serializer.Deserialize(reader, type);
            }
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="jsonObject">对象的Json字符串</param>
        /// <returns>反序列化后的对象</returns>
        public static T DeSerialize<T>(string jsonObject)
        {
            using (StringReader reader = new StringReader(jsonObject))
            {
                return (T) Serializer.Deserialize(reader, typeof(T));
            }
        }
    }
}