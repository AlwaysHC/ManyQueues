using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace NW.ManyQueues {
    class TestPluginReflection: IPluginStep1 {
        public TestPluginReflection() {
        }

        public int GetPriority() {
            return 1;
        }

        public FirePluginResult GetResult(FirePluginResult? previous) {
            return previous ?? new FirePluginResult();
        }

        public void SetCaller<T>(T caller) where T : class {
        }

        public bool Step(int number) {
            return number < 0;
        }
    }
}
