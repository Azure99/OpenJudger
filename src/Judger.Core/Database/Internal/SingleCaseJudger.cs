using System;
using System.Data.Common;
using System.Diagnostics;
using Judger.Core.Database.Internal.DbOperator;
using Judger.Core.Database.Internal.Entity;
using Judger.Models;
using Judger.Models.Database;
using Judger.Models.Judge;

namespace Judger.Core.Database.Internal
{
    /// <summary>
    /// 数据库单组用例评测器
    /// </summary>
    public class SingleCaseJudger
    {
        /// <summary>
        /// 对查询字段是否大小写敏感
        /// </summary>
        private readonly bool _caseSensitive;

        public SingleCaseJudger(JudgeContext context, BaseDbOperator dbOperator)
        {
            JudgeTask = context.Task;
            UserOperator = dbOperator;
            _caseSensitive = ((DbLangConfig) context.LangConfig).CaseSensitive;
        }

        private JudgeTask JudgeTask { get; }
        private BaseDbOperator UserOperator { get; }

        /// <summary>
        /// 评测一组用例
        /// </summary>
        /// <param name="stdDbData">标准输出</param>
        /// <param name="stdQueryData">标准查询</param>
        /// <returns>单组评测结果</returns>
        public SingleJudgeResult Judge(DbData stdDbData, DbQueryData stdQueryData)
        {
            Stopwatch watch = new Stopwatch();
            DbQueryData usrQuery;
            DbData usrOutput;

            try
            {
                watch.Start();
                DbDataReader reader = UserOperator.ExecuteQuery(JudgeTask.SourceCode, JudgeTask.TimeLimit);
                usrQuery = BaseDbOperator.ReadQueryData(reader);
                usrOutput = UserOperator.ReadDbData();
            }
            catch (Exception ex)
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
                watch.Stop();
            }

            CompareResult result = CompareAnswer(stdDbData, stdQueryData, usrOutput, usrQuery);

            JudgeResultCode resultCode =
                result == CompareResult.Accepted ? JudgeResultCode.Accepted : JudgeResultCode.WrongAnswer;

            return new SingleJudgeResult
            {
                ResultCode = resultCode,
                TimeCost = (int) watch.ElapsedMilliseconds
            };
        }

        private CompareResult CompareAnswer(DbData stdOutput, DbQueryData stdQuery, DbData usrOutput,
            DbQueryData usrQuery)
        {
            if (stdOutput != null && CompareDbData(stdOutput, usrOutput) == CompareResult.WrongAnswer)
                return CompareResult.WrongAnswer;

            if (stdQuery != null && CompareQuery(stdQuery, usrQuery) == CompareResult.WrongAnswer)
                return CompareResult.WrongAnswer;

            return CompareResult.Accepted;
        }

        private CompareResult CompareDbData(DbData stdOutput, DbData usrOutput)
        {
            if (stdOutput.TablesData.Length != usrOutput.TablesData.Length)
                return CompareResult.WrongAnswer;

            int tableCount = stdOutput.TablesData.Length;
            for (int i = 0; i < tableCount; i++)
            {
                if (!CmpString(stdOutput.TablesData[i].Name, usrOutput.TablesData[i].Name))
                    return CompareResult.WrongAnswer;

                if (CompareQuery(stdOutput.TablesData[i], usrOutput.TablesData[i]) == CompareResult.WrongAnswer)
                    return CompareResult.WrongAnswer;
            }

            return CompareResult.Accepted;
        }

        private CompareResult CompareQuery(DbQueryData stdQuery, DbQueryData usrQuery)
        {
            if (stdQuery.FieldCount != usrQuery.FieldCount || stdQuery.Records.Count != usrQuery.Records.Count)
                return CompareResult.WrongAnswer;

            int filedCount = stdQuery.FieldCount;
            int recordCount = stdQuery.Records.Count;

            for (int i = 0; i < filedCount; i++)
            {
                if (!CmpString(stdQuery.FieldNames[i], usrQuery.FieldNames[i]))
                    return CompareResult.WrongAnswer;
            }

            for (int i = 0; i < recordCount; i++)
            {
                for (int j = 0; j < filedCount; j++)
                {
                    if (stdQuery.Records[i][j] != usrQuery.Records[i][j])
                        return CompareResult.WrongAnswer;
                }
            }

            return CompareResult.Accepted;
        }

        /// <summary>
        /// 对比文本是否相同(大小写敏感根据_caseSensitive)
        /// </summary>
        private bool CmpString(string a, string b)
        {
            if (_caseSensitive)
                return a == b;

            return string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}