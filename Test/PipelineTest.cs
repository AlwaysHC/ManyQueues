using System;
using Xunit;

#nullable enable

namespace NW.ManyQueues.Test {
    public class PipelineTest {
        readonly static IManager M = new Manager();
        readonly static ILog LOG = new LogFile();
        readonly IPipelineManager _PM = new PipelineManager(M, LOG);

        [Fact]
        public void TestIn() {
            TestPipelineStep2 TestPipelineStep2 = new(2);

            _PM.CreatePipeline("1_2_3", new Type[] { typeof(TestPipelineStep1), typeof(TestPipelineStep2), typeof(TestPipelineStep3) }, TestPipelineStep2);

            Token Token = new();
            int RPL = _PM.RunPipeline(this, "1_2_3", 10, Token);

            Assert.Equal(190, Token.Number);
            Assert.Equal(3, RPL);
        }

        [Fact]
        public void TestOut() {
            TestPipelineStep2 TestPipelineStep2 = new(2);

            _PM.CreatePipeline("1_2_3", new Type[] { typeof(TestPipelineStep1), typeof(TestPipelineStep2), typeof(TestPipelineStep3) }, TestPipelineStep2);

            int RPL = _PM.RunPipeline(this, "1_2_3", 20, out Token Token);

            Assert.Equal(780, Token.Number);
            Assert.Equal(3, RPL);
        }
    }
}
