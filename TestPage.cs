using System;
using System.Collections.Generic;

#nullable enable

namespace NW.ManyQueues {
    public interface IPluginStep1: IPlugin {
        public bool Step(int number);
    }

    public interface IPipeplineToken: IPlugin {
        public bool Step(int number);
    }

    public class TestPage {
        readonly static IManager M = new Manager();
        readonly static ILog LOG = new LogFile();
        readonly IPluginManager _PM = new PluginManager(M, LOG);
        readonly IDataManager _DM = new DataManager(M, LOG);
        readonly IPipelineManager _PL = new PipelineManager(M, LOG);

        public void Page() {
            _PM.DeclarePlugin<IPluginStep1>(nameof(IPluginStep1));
            _PM.LoadPlugins<IPluginStep1>(nameof(IPluginStep1));

            TestPluginConstructor TP = new TestPluginConstructor(_PM);
            _PM.SubscribePlugin(nameof(IPluginStep1), TP);

            FirePluginResult? R1 = _PM.FirePlugin(this, nameof(IPluginStep1), 1, out IList<PluginReturn<bool>> Returns1);
            FirePluginResult? R2 = _PM.FirePlugin<TestPage, bool, int>(this, nameof(IPluginStep1), 1, out var Returns2);

            _DM.LoadDataReaders<int>("AAA");

            M.WriteConf("Start", 1000);

            _DM.StartBatchWrite("AAA");
            _DM.WriteBatchData("AAA", 1);
            _DM.WriteBatchData("AAA", "a");
            _DM.EndBatchWrite("AAA", false);

            _DM.WriteSingleData("AAA", 2);

            TestPipelineStep2 TestPipelineStep2 = new TestPipelineStep2();

            _PL.CreatePipeline("1_2_3", new Type[] { typeof(TestPipelineStep1), typeof(TestPipelineStep2), typeof(TestPipelineStep3) }, new IPipeline<Token>[] { TestPipelineStep2 });

            Token Token = new Token();
            int RPL = _PL.RunPipeline(this, "1_2_3", 10, Token);
            int RPL2 = _PL.RunPipeline(this, "1_2_3", 20, out Token T);
        }
    }
}
