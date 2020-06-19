using Judger.Core.Program.Internal.Entity;
using Judger.Models;

namespace Judger.Core.Program.Internal
{
    /// <summary>
    /// 单组用例评测器
    /// </summary>
    public class SingleCaseJudger : BaseSingleCaseJudger
    {
        public SingleCaseJudger(JudgeContext context) : base(context)
        { }

        protected override CompareResult CompareAnswer(string input, string correctAnswer, string userAnswer)
        {
            if (correctAnswer == userAnswer)
                return CompareResult.Accepted;

            // 正确答案
            string[] crtArr = correctAnswer.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            // 用户答案
            string[] usrArr = userAnswer.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            int crtLength = crtArr.Length;
            int usrLength = usrArr.Length;

            // 替代TrimEnd('\n'), 以减少内存开销
            while (crtLength > 0 && string.IsNullOrEmpty(crtArr[crtLength - 1]))
                crtLength--;

            while (usrLength > 0 && string.IsNullOrEmpty(usrArr[usrLength - 1]))
                usrLength--;

            if (crtLength == usrLength)
            {
                bool correct = true;
                for (int i = 0; i < crtLength; i++)
                {
                    if (crtArr[i] != usrArr[i])
                    {
                        correct = false;
                        break;
                    }
                }

                if (correct) // Accepted
                    return CompareResult.Accepted;
            }

            bool wrongAnswer = false;
            // 判断PE不再重新生成去空数组, 避免创建大量小字符串对象, 减少时空开销
            int crtPos = 0;
            int usrPos = 0;
            while (crtPos < crtLength && usrPos < usrLength)
            {
                bool jump = false;
                while (crtPos < crtLength && crtArr[crtPos] == "") // 跳过空白行
                {
                    crtPos++;
                    jump = true;
                }

                while (usrPos < usrLength && usrArr[usrPos] == "")
                {
                    usrPos++;
                    jump = true;
                }

                if (jump)
                    continue;

                if (usrArr[usrPos].Trim() != crtArr[crtPos].Trim())
                {
                    wrongAnswer = true;
                    break;
                }

                crtPos++;
                usrPos++;
            }

            while (crtPos < crtLength && crtArr[crtPos] == "") // 跳过空白行
                crtPos++;

            while (usrPos < usrLength && usrArr[usrPos] == "")
                usrPos++;

            if (crtPos != crtLength || usrPos != usrLength)
                wrongAnswer = true;

            return wrongAnswer ? CompareResult.WrongAnswer : CompareResult.PresentationError;
        }
    }
}