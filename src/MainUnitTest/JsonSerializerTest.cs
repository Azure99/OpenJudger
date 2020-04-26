using Judger.Utils;
using Xunit;

namespace MainUnitTest
{
    public class JsonSerializerTest
    {
        private bool Compare(TempTestClass a, TempTestClass b)
        {
            if (a.Id == b.Id && a.Name == b.Name)
            {
                if (a.Arr == null && b.Arr == null)
                    return true;

                for (int i = 0; i < a.Arr.Length; i++)
                {
                    if (a.Arr[i] != b.Arr[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        private class TempTestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int[] Arr { get; set; }
        }

        [Fact]
        public void TestSerializer()
        {
            TempTestClass classA = new TempTestClass
            {
                Id = 1,
                Name = "NNN",
                Arr = new[] {1, 2, 3}
            };

            string json = Json.Serialize(classA, typeof(TempTestClass));
            string json2 = Json.Serialize(classA);

            // 序列化是否成功
            Assert.True(json == json2);

            TempTestClass classB = Json.DeSerialize(json2, typeof(TempTestClass)) as TempTestClass;

            // 反序列化是否成功
            Assert.True(Compare(classA, classB) && Compare(classA, classB));

            json = json.Replace("NNN", "A");
            classB = Json.DeSerialize<TempTestClass>(json);
            TempTestClass classC = Json.DeSerialize<TempTestClass>(json2);

            // 修改json的情况下是否正确
            Assert.False(Compare(classB, classC));
        }
    }
}