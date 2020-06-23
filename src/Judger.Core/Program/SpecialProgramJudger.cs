using System;
using System.IO;
using Judger.Core.Program.Internal;
using Judger.Core.Program.Internal.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Judge;
using Judger.Models.Program;

namespace Judger.Core.Program
{
    /// <summary>
    /// 特殊程序评测器
    /// </summary>
    /// 适应答案不唯一的情况, 将用户输出交由SPJ程序评测
    public class SpecialProgramJudger : BaseProgramJudger
    {
        public SpecialProgramJudger(JudgeContext context) : base(context)
        {
            SpjContext = SpjManager.CreateSpjJudgeContext(context);
            SpjTask = SpjContext.Task;
            SpjLangConfig = SpjContext.LangConfig as ProgramLangConfig;
        }

        private JudgeContext SpjContext { get; }

        /// <summary>
        /// 注意:未编写:SPJTasks中的语言应对应SPJ程序, 而不是JudgeTask
        /// </summary>
        private JudgeTask SpjTask { get; }

        private ProgramLangConfig SpjLangConfig { get; }

        public override void Judge()
        {
            CheckUnsafeCode();
            BuildSpecialJudgeProgram();
            WriteSourceCode();
            CompileSourceCode();

            SpecialSingleCaseJudger judger = new SpecialSingleCaseJudger(Context, SpjContext);
            ProgramTestDataFile[] dataFiles = GetTestDataFiles();
            JudgeAllCases(judger, dataFiles);
        }

        /// <summary>
        /// 构建并写出供Judger使用的SPJ程序
        /// </summary>
        private void BuildSpecialJudgeProgram()
        {
            string sourceCodePath = Path.Combine(SpjContext.TempDirectory, SpjLangConfig.SourceCodeFileName);
            File.WriteAllText(sourceCodePath, SpjTask.SourceCode);

            if (!LangConfig.NeedCompile)
                return;

            // 构建SPJ程序
            if (TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId) == null)
            {
                if (!CompileSpecialJudgeProgram())
                    throw new CompileException("Can not build special judge program!");
            }

            SpecialJudgeProgram spjProgram = TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId);
            File.WriteAllBytes(
                Path.Combine(SpjContext.TempDirectory, spjProgram.LangConfig.ProgramFileName),
                spjProgram.Program);
        }

        /// <summary>
        /// 编译SPJ程序
        /// </summary>
        /// <returns>是否编译并写出成功</returns>
        private bool CompileSpecialJudgeProgram()
        {
            Compiler compiler = new Compiler(SpjContext);
            string compileResult = compiler.Compile();
            if (compileResult != "")
            {
                throw new CompileException("Can not compile special judge program!" + Environment.NewLine +
                                           compileResult);
            }

            string spjProgramPath =
                Path.Combine(SpjContext.TempDirectory, LangConfig.ProgramFileName);

            if (!File.Exists(spjProgramPath))
                throw new CompileException("Special judge program not found!");

            SpecialJudgeProgram spjProgram = new SpecialJudgeProgram
            {
                LangConfig = LangConfig,
                Program = File.ReadAllBytes(spjProgramPath)
            };

            TestDataManager.WriteSpecialJudgeProgramFile(JudgeTask.ProblemId, spjProgram);

            return TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId) != null;
        }
    }
}