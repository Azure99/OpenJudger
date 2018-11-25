using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Judger.Judger;

namespace MainUnitTest
{
    public class JudgerTest
    {
        [Fact]
        public void TestSingleJudgerCompare()
        {
            SingleJudger judger = new SingleJudger(new Judger.Models.JudgeTask());

            string test1 = @"123123";
            string test2 = @"123123";
            Assert.True(judger.CompareAnswer(test1, test2) == SingleJudger.CompareResult.Accepted);

            test1 = "123\r\n123";
            test2 = "123\n123";
            Assert.True(judger.CompareAnswer(test1, test2) == SingleJudger.CompareResult.Accepted);

            test1 = "123\r\n123\r\n";
            test2 = "123\n123";
            Assert.True(judger.CompareAnswer(test1, test2) == SingleJudger.CompareResult.Accepted);

            test1 = "123\r\n123\r\n";
            test2 = "123 \n123";
            Assert.True(judger.CompareAnswer(test1, test2) == SingleJudger.CompareResult.PresentationError);

            test1 = "123123\n\n\r\n";
            test2 = "123123";
            Assert.True(judger.CompareAnswer(test1, test2) == SingleJudger.CompareResult.Accepted);

            test1 = "123\n123\n\n\r\n";
            test2 = "\r\r\n\r\n 123\n123";
            Assert.True(judger.CompareAnswer(test1, test2) == SingleJudger.CompareResult.PresentationError);

            test1 = "123\n123\n";
            test2 = "1232\n123\n";
            Assert.True(judger.CompareAnswer(test1, test2) == SingleJudger.CompareResult.WrongAnswer);
        }
    }
}
