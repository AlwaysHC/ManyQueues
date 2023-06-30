#nullable enable

namespace NW.ManyQueues.Test {
    class TestPipelineStep2: IPipeline<Token> {
        Token _Token = new();
        readonly int _Factor;

        public TestPipelineStep2(int factor) {
            _Factor = factor;
        }

        public void SetCaller<T>(T caller) where T : class {
        }

        public void Execute1(int number) {
            _Token.Number *= number * _Factor;
        }

        public void SetToken(Token token) {
            _Token = token;
        }
    }
}
