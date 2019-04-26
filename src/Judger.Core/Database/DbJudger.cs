using System;
using System.Data.Common;
using System.IO;
using Judger.Core.Database.Internal;
using Judger.Core.Database.Internal.DbOperator;
using Judger.Core.Database.Internal.Entity;
using Judger.Entity;
using Judger.Entity.Database;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Core.Database
{
    public class DbJudger : BaseJudger
    {
        private BaseDbOperator MainOperator { get; set; }
        private string _dbName;
        private string _dbUser;
        private string _dbPassword;
        private DatabaseType _dbType;

        public DbJudger(JudgeTask task) : base(task)
        {
            MainOperator = DbOperatorFactory.CreateMainOperatorByName(JudgeTask.Language);
            _dbName = Path.GetFileName(task.TempJudgeDirectory).ToLower() + "db";
            _dbUser = Path.GetFileName(task.TempJudgeDirectory).ToLower() + "user";
            _dbPassword = Path.GetFileName(task.TempJudgeDirectory).ToLower() + "pwd";
            _dbType = DbManager.GetDatabaseType(JudgeTask.Language);
        }

        public override JudgeResult Judge()
        {
            JudgeResult result = new JudgeResult
            {
                SubmitId = JudgeTask.SubmitId,
                ProblemId = JudgeTask.ProblemId,
                Author = JudgeTask.Author,
                JudgeDetail = "",
                MemoryCost = ConfigManager.Config.MinimumMemoryCost,
                TimeCost = 0,
                PassRate = 0,
                ResultCode = JudgeResultCode.Accepted
            };

            string[] dataNames = TestDataManager.GetDbTestDataNames(JudgeTask.ProblemId, _dbType);
            if (dataNames.Length == 0)
            {
                result.ResultCode = JudgeResultCode.JudgeFailed;
                result.JudgeDetail = "No test data.";
                return result;
            }

            int acceptedCasesCount = 0;
            foreach (string dataName in dataNames)
            {
                SingleJudgeResult singleResult = JudgeOneCase(dataName);
                result.TimeCost = Math.Max(result.TimeCost, singleResult.TimeCost);
                if (singleResult.ResultCode == JudgeResultCode.Accepted)
                {
                    acceptedCasesCount++;
                }
                else
                {
                    result.ResultCode = singleResult.ResultCode;
                    result.JudgeDetail = singleResult.JudgeDetail;

                    if (!JudgeTask.JudgeAllCases)
                    {
                        break;
                    }
                }
            }

            result.PassRate = (double) acceptedCasesCount / dataNames.Length;

            return result;
        }

        private SingleJudgeResult JudgeOneCase(string dataName)
        {
            DbTestData testData = TestDataManager.GetDbTestData(JudgeTask.ProblemId, _dbType, dataName);
            BuildStandardData(testData, out string inputData, out DbData operData, out DbQueryData queryData);

            BaseDbOperator userOper = CreateJudgeEnv(inputData);
            
            SingleCaseJudger singleCaseJudger = new SingleCaseJudger(JudgeTask, userOper);
            SingleJudgeResult result = singleCaseJudger.Judge(inputData, operData, queryData);
            
            ClearJudgeEnv(userOper);

            return result;
        }

        private void BuildStandardData(DbTestData testData, out string inputData, out DbData operData, out DbQueryData queryData)
        {
            inputData = testData.Input;
            operData = null;
            queryData = null;
            string stdOperCmd = testData.Operation;
            string stdQueryCmd = testData.Query;
            
            BaseDbOperator stdOperator = CreateJudgeEnv(inputData);

            try
            {
                DbDataReader reader = stdOperator.ExecuteReader(stdOperCmd ?? stdQueryCmd);
                if (stdOperCmd != null)
                {
                    operData = stdOperator.ReadDbData();
                }

                if (stdQueryCmd != null)
                {
                    queryData = BaseDbOperator.ReadQueryData(reader);
                }
            }
            catch (Exception ex)
            {
                throw ex;
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