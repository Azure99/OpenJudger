using System;
using System.Data.Common;
using System.Diagnostics;
using Judger.Core.Database.Internal.Entity;
using Judger.Entity;

namespace Judger.Core.Database.Internal
{
    /// <summary>
    /// 数据库单组用例测试器
    /// </summary>
    public class SingleCaseJudger
    {
        /// <summary>
        /// 用户的数据库操作器
        /// </summary>
        public BaseDbOperator UserOperator { get; private set; }

        /// <summary>
        /// 评测任务
        /// </summary>
        public JudgeTask JudgeTask { get; private set; }

        public SingleCaseJudger(JudgeTask task, BaseDbOperator dbOperator)
        {
            JudgeTask = task;
            UserOperator = dbOperator;
        }

        /// <summary>
        /// 评测一组用例
        /// </summary>
        /// <param name="stdInput">标准输入</param>
        /// <param name="stdOutput">标准输出</param>
        /// <param name="stdQuery">标准查询</param>
        /// <returns></returns>
        public SingleJudgeResult Judge(string stdInput, DbData stdOutput, DbQueryData stdQuery)
        {
            Stopwatch sw = new Stopwatch();
            DbDataReader reader = null;

            try
            {
                sw.Start();
                reader = UserOperator.ExecuteReader(JudgeTask.SourceCode, JudgeTask.TimeLimit);
            }
            catch(Exception ex)
            {
                return new SingleJudgeResult
                {
                    ResultCode = JudgeResultCode.RuntimeError,
                    JudgeDetail = ex.Message,
                    TimeCost = 0
                };
            }
            finally
            {
                sw.Stop();
            }

            DbQueryData usrQuery = BaseDbOperator.ReadQueryData(reader);
            DbData usrOutput = UserOperator.ReadDbData();

            CompareResult result = CompareAnswer(stdOutput, stdQuery, usrOutput, usrQuery);

            return new SingleJudgeResult
            {
                ResultCode = (result == CompareResult.Accepted) ? JudgeResultCode.Accepted : JudgeResultCode.WrongAnswer,
                TimeCost = (int)sw.ElapsedMilliseconds
            };
        }

        private CompareResult CompareAnswer(DbData stdOutput, DbQueryData stdQuery, DbData usrOutput, DbQueryData usrQuery)
        {
            if (stdOutput != null)
            {
                if (CompareOutput(stdOutput, usrOutput) == CompareResult.WrongAnswer)
                {
                    return CompareResult.WrongAnswer;
                }
            }

            if (stdQuery != null)
            {
                if (CompareQuery(stdQuery, usrQuery) == CompareResult.WrongAnswer)
                {
                    return CompareResult.WrongAnswer;
                }
            }

            return CompareResult.Accepted;
        }

        private CompareResult CompareOutput(DbData stdOutput, DbData usrOutput)
        {
            if (stdOutput.TablesData.Length != usrOutput.TablesData.Length)
            {
                return CompareResult.WrongAnswer;
            }

            int tableCount = stdOutput.TablesData.Length;
            for (int i = 0; i < tableCount; i++)
            {
                if (stdOutput.TablesData[i].Name != usrOutput.TablesData[i].Name)
                {
                    return CompareResult.WrongAnswer;
                }

                if(CompareQuery(stdOutput.TablesData[i], usrOutput.TablesData[i]) == CompareResult.WrongAnswer)
                {
                    return CompareResult.WrongAnswer;
                }
            }

            return CompareResult.Accepted;
        }

        private CompareResult CompareQuery(DbQueryData stdQuery, DbQueryData usrQuery)
        {
            if (stdQuery.FieldCount != usrQuery.FieldCount || 
                stdQuery.Records.Count != usrQuery.Records.Count)
            {
                return CompareResult.WrongAnswer;
            }

            int filedCount = stdQuery.FieldCount;
            int recordCount = stdQuery.Records.Count;

            for (int i = 0; i < filedCount; i++)
            {
                if (stdQuery.FieldNames[i] != usrQuery.FieldNames[i])
                {
                    return CompareResult.WrongAnswer;
                }
            }

            for (int i = 0; i < recordCount; i++)
            {
                for (int i2 = 0; i2 < filedCount; i2++)
                {
                    if (stdQuery.Records[i][i2] != usrQuery.Records[i][i2])
                    {
                        return CompareResult.WrongAnswer;
                    }
                }
            }

            return CompareResult.Accepted;
        }
    }
}
