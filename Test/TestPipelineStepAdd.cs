#nullable enable

namespace NW.ManyQueues.Test {
    class Token {
        public int Number = 0;
    }

    class TestPipelineStepAdd: IPipeline<Token> {
        Token _Token = new();

        public void SetCaller<T>(T caller) where T : class {
        }

        public void Execute1(int number) {
            _Token.Number += number;
        }

        public void SetToken(Token token) {
            _Token = token;
        }
    }
}
