using System;
using System.Data.Common;
using System.Diagnostics;
using Judger.Core.Database.Internal.DbOperator;
using Judger.Core.Database.Internal.Entity;
using Judger.Models;
using Judger.Models.Database;

namespace Judger.Core.Database.Internal
{
    /// <summary>
    /// 数据库单组用例测试器
    /// </summary>
    public class SingleCaseJudger
    {
        /// <summary>
        /// 对查询字段是否大小写敏感
        /// </summary>
        private bool _caseSensitive;

        public SingleCaseJudger(JudgeContext context, BaseDbOperator dbOperator)
        {
            JudgeContext = context;
            JudgeTask = context.Task;
            UserOperator = dbOperator;
            _caseSensitive = (context.LangConfig as DbLangConfig).CaseSensitive;
        }

        /// <summary>
        /// 用户的数据库操作器
        /// </summary>
        public BaseDbOperator UserOperator { get; private set; }

        /// <summary>
        /// 评测上下文
        /// </summary>
        public JudgeContext JudgeContext { get; private set; }
        
        /// <summary>
        /// 评测任务
        /// </summary>
        public JudgeTask JudgeTask { get; private set; }

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
            DbQueryData usrQuery = null;
            DbData usrOutput = null;

            try
            {
                sw.Start();
                reader = UserOperator.ExecuteReader(JudgeTask.SourceCode, JudgeTask.TimeLimit);

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
                sw.Stop();
            }

            CompareResult result = CompareAnswer(stdOutput, stdQuery, usrOutput, usrQuery);
            JudgeResultCode resultCode = (result == CompareResult.Accepted)
                ? JudgeResultCode.Accepted
                : JudgeResultCode.WrongAnswer;

            return new SingleJudgeResult
            {
                ResultCode = resultCode,
                TimeCost = (int) sw.ElapsedMilliseconds
            };
        }

        private CompareResult CompareAnswer(DbData stdOutput, DbQueryData stdQuery, DbData usrOutput,
            DbQueryData usrQuery)
        {
            if (stdOutput != null && CompareOutput(stdOutput, usrOutput) == CompareResult.WrongAnswer)
                return CompareResult.WrongAnswer;

            if (stdQuery != null && CompareQuery(stdQuery, usrQuery) == CompareResult.WrongAnswer)
                return CompareResult.WrongAnswer;

            return CompareResult.Accepted;
        }

        private CompareResult CompareOutput(DbData stdOutput, DbData usrOutput)
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
                for (int i2 = 0; i2 < filedCount; i2++)
                {
                    if (stdQuery.Records[i][i2] != usrQuery.Records[i][i2])
                        return CompareResult.WrongAnswer;
                }
            }

            return CompareResult.Accepted;
        }

        /// <summary>
        /// 对比文本是否相同(大小写敏感根据_caseSensitive)
        /// </summary>
        /// <param name="a">string a</param>
        /// <param name="b">string b</param>
        /// <returns>是否相同</returns>
        private bool CmpString(string a, string b)
        {
            if (_caseSensitive)
                return a == b;

            return a.ToLower() == b.ToLower();
        }
    }
}