using Judger.Core.Program.Internal;
using Judger.Models;
using Judger.Models.Program;

namespace Judger.Core.Program
{
    /// <summary>
    /// 程序评测器
    /// </summary>
    /// 对普通编程题目实现的评测器
    public class ProgramJudger : BaseProgramJudger
    {
        public ProgramJudger(JudgeContext context) : base(context)
        { }

        public override void Judge()
        {
            CheckUnsafeCode();
            WriteSourceCode();
            CompileSourceCode();

            SingleCaseJudger judger = new SingleCaseJudger(Context);
            ProgramTestDataFile[] dataFiles = GetTestDataFiles();
            JudgeAllCases(judger, dataFiles);
        }
    }
}