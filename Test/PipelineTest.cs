using System;
using System.Collections.Generic;
using Xunit;

#nullable enable

namespace NW.ManyQueues.Test {
    public class PipelineTest {
        readonly static IManager M = new Manager();
        readonly static ILog LOG = new LogFile();
        readonly IPipelineManager _PM = new PipelineManager(M, LOG);

        [Fact]
        public void TestIn() {
            TestPipelineStep2 TestPipelineStep2 = new TestPipelineStep2();

            _PM.CreatePipeline("1_2_3", new Type[] { typeof(TestPipelineStep1), typeof(TestPipelineStep2), typeof(TestPipelineStep3) }, new IPipeline<Token>[] { TestPipelineStep2 });

            Token Token = new Token();
            int RPL = _PM.RunPipeline(this, "1_2_3", 10, Token);

            Assert.True(Token.Number == 90);
            Assert.True(RPL == 3);
        }

        [Fact]
        public void TestOut() {
            TestPipelineStep2 TestPipelineStep2 = new TestPipelineStep2();

            _PM.CreatePipeline("1_2_3", new Type[] { typeof(TestPipelineStep1), typeof(TestPipelineStep2), typeof(TestPipelineStep3) }, new IPipeline<Token>[] { TestPipelineStep2 });

            int RPL = _PM.RunPipeline(this, "1_2_3", 20, out Token Token);

            Assert.True(Token.Number == 380);
            Assert.True(RPL == 3);
        }
    }
}
