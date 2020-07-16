#nullable enable

namespace NW.ManyQueues {
    class Token {
        public int Number = 0;
    }

    class TestPipelineStep1: IPipeline<Token> {
        Token _Token = new Token();

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
