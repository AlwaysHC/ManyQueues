using System.Collections.Generic;
using Xunit;

#nullable enable

namespace NW.ManyQueues.Test {
    public interface IPluginStep1: IPlugin {
        public bool Step(int number);
    }

    public class PluginTest {
        readonly static IManager M = new Manager();
        readonly static ILog LOG = new LogFile();
        readonly IPluginManager _PM = new PluginManager(M, LOG);

        [Fact]
        public void TestImplicit() {

            _PM.DeclarePlugin<IPluginStep1>(nameof(IPluginStep1));
            _PM.LoadPlugins<IPluginStep1>(nameof(IPluginStep1));

            FirePluginResult? R = _PM.FirePlugin(this, nameof(IPluginStep1), 1, out IList<PluginReturn<bool>> Returns);

            Assert.True(!R?.Blocking);
        }

        [Fact]
        public void TestExplicit() {

            _PM.DeclarePlugin<IPluginStep1>(nameof(IPluginStep1));

            TestPluginConstructor TP = new TestPluginConstructor(_PM);
            _PM.SubscribePlugin(nameof(IPluginStep1), TP);

            FirePluginResult? R = _PM.FirePlugin(this, nameof(IPluginStep1), 1, out IList<PluginReturn<bool>> Returns);

            Assert.True(R?.Blocking);
        }
    }
}
