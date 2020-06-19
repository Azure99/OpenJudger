using System;
using System.Data.Common;
using System.IO;
using Judger.Core.Database.Internal;
using Judger.Core.Database.Internal.DbOperator;
using Judger.Core.Database.Internal.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Database;
using Judger.Models.Exception;
using Judger.Models.Judge;

namespace Judger.Core.Database
{
    /// <summary>
    /// 数据库评测器
    /// </summary>
    /// 对SQL题目实现的评测器
    public class DbJudger : BaseJudger
    {
        private readonly string _dbName;
        private readonly string _dbPassword;
        private readonly DatabaseType _dbType;
        private readonly string _dbUser;

        public DbJudger(JudgeContext context) : base(context)
        {
            MainOperator = DbOperatorFactory.CreateMainOperatorByName(context.Task.Language);
            string prefix = Path.GetFileName(context.TempDirectory)?.ToLower();

            _dbName = prefix + "db";
            _dbUser = prefix + "user";
            _dbPassword = prefix + "pwd";
            _dbType = DbManager.GetDatabaseType(context.Task.Language);
        }

        private BaseDbOperator MainOperator { get; }

        public override void Judge()
        {
            JudgeResult result = JudgeResult;

            string[] dataNames = TestDataManager.GetDbTestDataNames(JudgeTask.ProblemId, _dbType);
            if (dataNames.Length == 0)
            {
                result.ResultCode = JudgeResultCode.JudgeFailed;
                result.JudgeDetail = "No test data.";
                return;
            }

            int acceptedCount = 0;
            foreach (string dataName in dataNames)
            {
                SingleJudgeResult singleResult = JudgeOneCase(dataName);
                result.TimeCost = Math.Max(result.TimeCost, singleResult.TimeCost);

                if (singleResult.ResultCode == JudgeResultCode.Accepted)
                    acceptedCount++;
                else
                {
                    result.ResultCode = singleResult.ResultCode;
                    result.JudgeDetail = singleResult.JudgeDetail;

                    if (!JudgeTask.JudgeAllCases)
                        break;
                }
            }

            result.PassRate = (double) acceptedCount / dataNames.Length;
        }

        private SingleJudgeResult JudgeOneCase(string dataName)
        {
            DbTestData testData = TestDataManager.GetDbTestData(JudgeTask.ProblemId, _dbType, dataName);
            BuildStandardData(testData, out string inputData, out DbData dbData, out DbQueryData queryData);

            BaseDbOperator userOperator = CreateJudgeEnvironment(inputData);

            SingleCaseJudger singleCaseJudger = new SingleCaseJudger(Context, userOperator);
            SingleJudgeResult result = singleCaseJudger.Judge(dbData, queryData);

            ClearJudgeEnvironment(userOperator);

            return result;
        }

        private void BuildStandardData(DbTestData testData,
            out string inputData, out DbData outputData, out DbQueryData queryData)
        {
            inputData = testData.Input;
            outputData = null;
            queryData = null;
            string stdOperateCmd = testData.Operation;
            string stdQueryCmd = testData.Query;

            BaseDbOperator stdOperator = CreateJudgeEnvironment(inputData);

            try
            {
                DbDataReader reader = stdOperator.ExecuteQuery(stdOperateCmd ?? stdQueryCmd);
                if (stdOperateCmd != null)
                    outputData = stdOperator.ReadDbData();

                if (stdQueryCmd != null)
                    queryData = BaseDbOperator.ReadQueryData(reader);
            }
            catch (Exception ex)
            {
                throw new JudgeException("Build Db test data failed!" + ex);
            }
            finally
            {
                ClearJudgeEnvironment(stdOperator);
            }
        }

        private BaseDbOperator CreateJudgeEnvironment(string input)
        {
            MainOperator.CreateDatabase(_dbName);
            MainOperator.CreateUser(_dbUser, _dbPassword);
            MainOperator.GrantPrivileges(_dbName, _dbUser);

            DbLangConfig dbConfig = DbManager.GetDbConfig(JudgeTask.Language);
            dbConfig.Database = _dbName;
            dbConfig.User = _dbUser;
            dbConfig.Password = _dbPassword;

            BaseDbOperator userOperator = DbOperatorFactory.Create(dbConfig);
            userOperator.InitDatabase(input);

            return userOperator;
        }

        private void ClearJudgeEnvironment(BaseDbOperator userOperator)
        {
            userOperator.Dispose();
            MainOperator.DropDatabase(_dbName);
            MainOperator.DropUser(_dbUser);
        }

        public override void Dispose()
        {
            MainOperator.Dispose();
        }
    }
}