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
    public class DbJudger : BaseJudger
    {
        private readonly string _dbName;
        private readonly string _dbPassword;
        private readonly DatabaseType _dbType;
        private readonly string _dbUser;

        public DbJudger(JudgeContext context) : base(context)
        {
            MainOperator = DbOperatorFactory.CreateMainOperatorByName(context.Task.Language);
            _dbName = Path.GetFileName(context.TempDirectory).ToLower() + "db";
            _dbUser = Path.GetFileName(context.TempDirectory).ToLower() + "user";
            _dbPassword = Path.GetFileName(context.TempDirectory).ToLower() + "pwd";
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

            int acceptedCasesCount = 0;
            foreach (string dataName in dataNames)
            {
                SingleJudgeResult singleResult = JudgeOneCase(dataName);
                result.TimeCost = Math.Max(result.TimeCost, singleResult.TimeCost);
                if (singleResult.ResultCode == JudgeResultCode.Accepted)
                    acceptedCasesCount++;
                else
                {
                    result.ResultCode = singleResult.ResultCode;
                    result.JudgeDetail = singleResult.JudgeDetail;

                    if (!JudgeTask.JudgeAllCases)
                        break;
                }
            }

            result.PassRate = (double) acceptedCasesCount / dataNames.Length;
        }

        private SingleJudgeResult JudgeOneCase(string dataName)
        {
            DbTestData testData = TestDataManager.GetDbTestData(JudgeTask.ProblemId, _dbType, dataName);
            BuildStandardData(testData, out string inputData, out DbData outputData, out DbQueryData queryData);

            BaseDbOperator userOper = CreateJudgeEnv(inputData);

            SingleCaseJudger singleCaseJudger = new SingleCaseJudger(Context, userOper);
            SingleJudgeResult result = singleCaseJudger.Judge(inputData, outputData, queryData);

            ClearJudgeEnv(userOper);

            return result;
        }

        private void BuildStandardData(DbTestData testData, out string inputData, out DbData outputData,
            out DbQueryData queryData)
        {
            inputData = testData.Input;
            outputData = null;
            queryData = null;
            string stdOperCmd = testData.Operation;
            string stdQueryCmd = testData.Query;

            BaseDbOperator stdOperator = CreateJudgeEnv(inputData);

            try
            {
                DbDataReader reader = stdOperator.ExecuteReader(stdOperCmd ?? stdQueryCmd);
                if (stdOperCmd != null)
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
                ClearJudgeEnv(stdOperator);
            }
        }

        private BaseDbOperator CreateJudgeEnv(string input)
        {
            MainOperator.CreateDatabase(_dbName);
            MainOperator.CreateUser(_dbUser, _dbPassword);
            MainOperator.GeneratePrivileges(_dbName, _dbUser);

            DbLangConfig dbConfig = DbManager.GetDbConfiguration(JudgeTask.Language);
            dbConfig.Database = _dbName;
            dbConfig.User = _dbUser;
            dbConfig.Password = _dbPassword;

            BaseDbOperator userOper = DbOperatorFactory.Create(dbConfig);
            userOper.InitDatabase(input);

            return userOper;
        }

        private void ClearJudgeEnv(BaseDbOperator userOper)
        {
            userOper.Dispose();
            MainOperator.DropDatabase(_dbName);
            MainOperator.DropUser(_dbUser);
        }

        public override void Dispose()
        {
            MainOperator.Dispose();
        }
    }
}