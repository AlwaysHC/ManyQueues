#nullable enable

namespace NW.ManyQueues.Test {
    class TestPipelineStep2: IPipeline<Token> {
        Token _Token = new Token();

        public void SetCaller<T>(T caller) where T : class {
        }

        public void Execute1(int number) {
            _Token.Number *= number;
        }

        public void SetToken(Token token) {
            _Token = token;
        }
    }
}
