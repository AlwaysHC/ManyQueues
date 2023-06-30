#nullable enable

namespace NW.ManyQueues.Test {
    class Token2 {
        public int Number = 0;
    }

    class TestPipelineToken2Add: IPipeline<Token2> {
        Token2 _Token = new();

        public void SetCaller<T>(T caller) where T : class {
        }

        public void Execute1(int number) {
            _Token.Number += number;
        }

        public void SetToken(Token2 token) {
            _Token = token;
        }
    }
}
