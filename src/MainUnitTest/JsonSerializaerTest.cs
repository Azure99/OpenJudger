using System;
using Xunit;
using Judger.Utils;

namespace MainUnitTest
{
    public class JsonSerializaerTest
    {
        /// <summary>
        /// 测试序列化反序列化功能
        /// </summary>
        [Fact]
        public void TestSerializer()
        {
            TempTestClass classA = new TempTestClass
            {
                ID = 1,
                Name = "NNN",
                Arr = new int[] { 1, 2, 3 }
            };

            string json = SampleJsonSerializaer.Serialize(classA, typeof(TempTestClass));
            string json2 = SampleJsonSerializaer.Serialize<TempTestClass>(classA);

            //序列化是否成功
            Assert.True(json == json2);

            TempTestClass classB = SampleJsonSerializaer.DeSerialize(json2, typeof(TempTestClass)) as TempTestClass;
            TempTestClass classC = SampleJsonSerializaer.DeSerialize<TempTestClass>(json);

            //反序列化是否成功
            Assert.True(Compare(classA, classB) && Compare(classA, classB));

            json = json.Replace("NNN", "A");
            classB = SampleJsonSerializaer.DeSerialize<TempTestClass>(json);
            classC = SampleJsonSerializaer.DeSerialize<TempTestClass>(json2);

            //修改json的情况下是否正确
            Assert.False(Compare(classB, classC));
        }

        private bool Compare(TempTestClass a, TempTestClass b)
        {
            if (a.ID == b.ID && a.Name == b.Name)
            {
                if (a.Arr == null && b.Arr == null)
                {
                    return true;
                }

                for (int i = 0; i < a.Arr.Length; i++)
                {
                    if (a.Arr[i] != b.Arr[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private class TempTestClass
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int[] Arr { get; set; }
        }
    }
}
