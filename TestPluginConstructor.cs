using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace NW.ManyQueues {
    class TestPluginConstructor: IPluginStep1 {
        private readonly IPluginManager _SM;

        public TestPluginConstructor(IPluginManager subscriptionManager) {
            _SM = subscriptionManager;
            _SM.SubscribePlugin(nameof(IPluginStep1), this);
        }

        public static bool AutoLoad() => false;

        public int GetPriority() {
            return 0;
        }

        public FirePluginResult GetResult(FirePluginResult? previous) {
            return previous ?? new FirePluginResult() { Blocking = true };
        }

        public void SetCaller<T>(T caller) where T : class {
        }

        public bool Step(int number) {
            return number > 0;
        }
    }
}
