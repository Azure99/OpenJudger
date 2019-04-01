using System;
using System.IO;
using Xunit;
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

        //[Fact]
        public void Test()
        {
            BaseDbOperator oper = CreateDbOperator();
            oper.ExecuteNonQuery(File.ReadAllText(INPUT_FILE));

            var reader = oper.ExecuteReader(COMMAND);

            DbQueryData query = BaseDbOperator.ReadQueryData(reader);
            DbData output = oper.ReadDbData();

            string outputData = SampleJsonSerializaer.Serialize<DbData>(output);
            string queryData = SampleJsonSerializaer.Serialize<DbQueryData>(query);

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
