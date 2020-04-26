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
        /// <summary>
        /// 测试SingleJudger的答案对比
        /// </summary>
        [Fact]
        public void TestSingleJudgerCompare()
        {
            SingleCaseJudger judger =
                new SingleCaseJudger(new JudgeContext(new JudgeTask(), new JudgeResult(), "", new ProgramLangConfig()));

            string test1 = @"123123";
            string test2 = @"123123";
            Assert.True(judger.CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\r\n123";
            test2 = "123\n123";
            Assert.True(judger.CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\r\n123\r\n";
            test2 = "123\n123";
            Assert.True(judger.CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\r\n123\r\n";
            test2 = "123 \n123";
            Assert.True(judger.CompareAnswer(test1, test2) == CompareResult.PresentationError);

            test1 = "123123\n\n\r\n";
            test2 = "123123";
            Assert.True(judger.CompareAnswer(test1, test2) == CompareResult.Accepted);

            test1 = "123\n123\n\n\r\n";
            test2 = "\r\r\n\r\n 123\n123";
            Assert.True(judger.CompareAnswer(test1, test2) == CompareResult.PresentationError);

            test1 = "123\n123\n";
            test2 = "1232\n123\n";
            Assert.True(judger.CompareAnswer(test1, test2) == CompareResult.WrongAnswer);
        }
    }
}