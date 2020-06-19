using System;
using System.Reflection;
using Judger.Core.Program.Internal;
using Judger.Core.Program.Internal.Entity;
using Judger.Models;
using Judger.Models.Judge;
using Judger.Models.Program;
using Xunit;

namespace MainUnitTest
{
    public class JudgerTest
    {
        private readonly SingleCaseJudger _judger =
            new SingleCaseJudger(new JudgeContext(new JudgeTask(), new JudgeResult(), "", new ProgramLangConfig()));

        private CompareResult CompareAnswer(string ans1, string ans2)
        {
            foreach (MethodInfo method in typeof(SingleCaseJudger).GetRuntimeMethods())
            {
                if (method.Name == "CompareAnswer")
                {
                    object result = method.Invoke(_judger, new object[] {"", ans1, ans2});
                    if (result is CompareResult compareResult)
                        return compareResult;
                }
            }

            throw new NotImplementedException("CompareAnswer not implemented");
        }

        /// <summary>
        /// 测试SingleJudger的答案对比
        /// </summary>
        [Fact]
        public void TestSingleJudgerCompare()
        {
            string test1 = @"123123";
            string test2 = @"123123";
            Assert.True(CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\r\n123";
            test2 = "123\n123";
            Assert.True(CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\r\n123\r\n";
            test2 = "123\n123";
            Assert.True(CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\r\n123\r\n";
            test2 = "123 \n123";
            Assert.True(CompareAnswer(test1, test2) == CompareResult.PresentationError);

            test1 = "123123\n\n\r\n";
            test2 = "123123";
            Assert.True(CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\n123\n\n\r\n";
            test2 = "\r\r\n\r\n 123\n123";
            Assert.True(CompareAnswer(test1, test2) == CompareResult.PresentationError);

            test1 = "123\n123\n";
            test2 = "1232\n123\n";
            Assert.True(CompareAnswer(test1, test2) == CompareResult.WrongAnswer);
        }
    }
}