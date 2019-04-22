using System.IO;
using Judger.Core.Database.Internal;
using Judger.Core.Database.Internal.DbOperator;
using Judger.Core.Database.Internal.Entity;
using Judger.Utils;

namespace MainUnitTest
{
    public class DbTestDataCreator
    {
        private const string INPUT_FILE = "input.sql";
        private const string OUTPUT_FILE = "output.mysql";
        private const string QUERY_FILE = "query.mysql";
        private const string DRIVER_FILE = "Pomelo.Data.MySql.dll";
        private const string CONNECTION_STRING = "Server=lo11calhost;Database=judger;User=root;Password=961523404;CharSet=utf8;";
        private const string COMMAND = "SELECT * FROM student";

        /// <summary>
        /// 创建DbJudge测试数据(临时使用)
        /// </summary>
        //[Fact]
        public void CreateTestData()
        {
            BaseDbOperator oper = CreateDbOperator();
            oper.ExecuteNonQuery(File.ReadAllText(INPUT_FILE));

            var reader = oper.ExecuteReader(COMMAND);

            DbQueryData query = BaseDbOperator.ReadQueryData(reader);
            DbData output = oper.ReadDbData();

            string outputData = SampleJsonSerializaer.Serialize(output);
            string queryData = SampleJsonSerializaer.Serialize(query);

            File.WriteAllText(OUTPUT_FILE, outputData);
            File.WriteAllText(QUERY_FILE, queryData);
        }

        private BaseDbOperator CreateDbOperator()
        {
            var driver = DbDriverLoader.Load(DRIVER_FILE);
            return new MySQL5xOperator(CONNECTION_STRING, driver);
        }
    }
}
