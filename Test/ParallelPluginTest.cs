using System.Collections.Generic;
using Xunit;

#nullable enable

namespace NW.ManyQueues.Test {

    public class ParallelPluginTest {
        readonly static IManager M = new Manager();
        readonly static ILog LOG = new LogFile();
        readonly IParallelPluginManager _PM = new ParallelPluginManager(M, LOG);

        [Fact]
        public void TestImplicit() {

            _PM.DeclarePlugin<IPluginStep1>(nameof(IPluginStep1));
            _PM.LoadPlugins<IPluginStep1>(nameof(IPluginStep1));

            IList<FirePluginResult> R = _PM.FirePlugin(this, nameof(IPluginStep1), 1);

            Assert.True(!R[0].Blocking);
        }

        [Fact]
        public void TestExplicit() {

            _PM.DeclarePlugin<IPluginStep1>(nameof(IPluginStep1));


            TestPluginConstructor TP = new TestPluginConstructor(_PM);
            _PM.SubscribePlugin(nameof(IPluginStep1), TP);

            IList<FirePluginResult> R = _PM.FirePlugin(this, nameof(IPluginStep1), 1);

            Assert.True(R[0].Blocking);
        }
    }
}
