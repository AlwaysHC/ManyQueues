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
            TestPipelineStepMul TestPipelineStepMul = new(2);

            _PM.CreatePipeline(new Type[] { typeof(TestPipelineStepAdd), typeof(TestPipelineStepMul), typeof(TestPipelineStepSub) }, TestPipelineStepMul);

            Token Token = new();
            int RPL = _PM.RunPipeline(this, Token, 10);

            Assert.Equal(190, Token.Number);
            Assert.Equal(3, RPL);
        }

        [Fact]
        public void TestAddStep() {
            TestPipelineStepMul TestPipelineStepMul = new(2);
            TestPipelineToken2Add AlreadyCreatedToken2Add = new();

            _PM.CreatePipeline(new Type[] { typeof(TestPipelineStepAdd), typeof(TestPipelineStepMul) }, TestPipelineStepMul);
            _PM.AddSteps<Token>(typeof(TestPipelineStepSub));
            _PM.AddSteps<Token2>(typeof(TestPipelineToken2Add));
            _PM.AddSteps(AlreadyCreatedToken2Add);

            Token Token = new();
            int RPL = _PM.RunPipeline(this, Token, 10);

            Assert.Equal(190, Token.Number);
            Assert.Equal(3, RPL);

            Token2 Token2 = new();
            int RPL2 = _PM.RunPipeline(this, Token2, 10);

            Assert.Equal(20, Token2.Number);
            Assert.Equal(2, RPL2);
        }

        [Fact]
        public void TestOut() {
            TestPipelineStepMul TestPipelineStep2 = new(2);

            _PM.CreatePipeline(new Type[] { typeof(TestPipelineStepAdd), typeof(TestPipelineStepMul), typeof(TestPipelineStepSub) }, TestPipelineStep2);

            int RPL = _PM.RunPipeline(this, out Token Token, 20);

            Assert.Equal(780, Token.Number);
            Assert.Equal(3, RPL);
        }

        [Fact]
        public void TestFluent() {
            TestPipelineStepMul TestPipelineStepMul = new(2);

            _PM.AddSteps<Token>(typeof(TestPipelineStepAdd))
               .AddSteps(TestPipelineStepMul)
               .AddSteps(typeof(TestPipelineStepSub))
               .AddStep(TestPipelineStepMul)
               .AddStep<TestPipelineStepSub>();

            Token Token = new();
            int RPL = _PM.RunPipeline(this, Token, 10);

            Assert.Equal(3790, Token.Number);
            Assert.Equal(5, RPL);
        }
    }
}
