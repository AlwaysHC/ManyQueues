#nullable enable

namespace NW.ManyQueues.Test {
    class TestPipelineStepMul: IPipeline<Token> {
        Token _Token = new();
        readonly int _Factor;

        public TestPipelineStepMul(int factor) {
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
