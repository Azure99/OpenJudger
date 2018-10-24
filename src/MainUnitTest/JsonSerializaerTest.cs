using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Judger.Utils;

namespace MainUnitTest
{
    public class JsonSerializaerTest
    {
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

            Assert.True(json == json2);//序列化是否成功

            TempTestClass classB = SampleJsonSerializaer.DeSerialize(json2, typeof(TempTestClass)) as TempTestClass;
            TempTestClass classC = SampleJsonSerializaer.DeSerialize<TempTestClass>(json);

            Assert.True(Compare(classA, classB) && Compare(classA, classB));//反序列化是否成功

            json = json.Replace("NNN", "A");
            classB = SampleJsonSerializaer.DeSerialize<TempTestClass>(json);
            classC = SampleJsonSerializaer.DeSerialize<TempTestClass>(json2);

            Assert.False(Compare(classB, classC));//修改json的情况下是否正确
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
