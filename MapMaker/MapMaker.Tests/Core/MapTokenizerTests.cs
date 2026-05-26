using MapMaker.Core.Parsing;

namespace MapMaker.Tests.Core
{
    public class MapTokenizerTests
    {
        [Fact]
        public void Tokenize_ShouldReadEntityPropertyTokens()
        {
            var text = """
            {
                "classname" "worldspawn"
            }
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Contains(tokens, x => x.Type == MapTokenType.LeftBrace);
            Assert.Contains(tokens, x => x.Type == MapTokenType.String && x.Value == "classname");
            Assert.Contains(tokens, x => x.Type == MapTokenType.String && x.Value == "worldspawn");
            Assert.Contains(tokens, x => x.Type == MapTokenType.RightBrace);
            Assert.Equal(MapTokenType.EndOfFile, tokens[^1].Type);
        }

        [Fact]
        public void Tokenize_ShouldReadBrushFaceTokens()
        {
            var text = """
            ( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) common/case 0 0 0 1 1
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Equal(3, tokens.Count(x => x.Type == MapTokenType.LeftParen));
            Assert.Equal(3, tokens.Count(x => x.Type == MapTokenType.RightParen));
            Assert.Contains(tokens, x => x.Type == MapTokenType.Identifier && x.Value == "common/case");
            Assert.Equal(14, tokens.Count(x => x.Type == MapTokenType.Number));
        }

        [Fact]
        public void Tokenize_ShouldReadNegativeAndDecimalNumbers()
        {
            var text = """
            ( -64.5 0 128.25 )
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "-64.5");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "0");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "128.25");
        }

        [Fact]
        public void Tokenize_ShouldReadComments()
        {
            var text = """
            // entity start
            {
                "classname" "worldspawn"
            }
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Contains(tokens, x => x.Type == MapTokenType.Comment);
            Assert.Contains(tokens, x => x.Type == MapTokenType.String && x.Value == "classname");
        }

        [Fact]
        public void Tokenize_ShouldReadTexturePathAsIdentifier()
        {
            var text = """
            common/caulk textures/test/my_texture
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Contains(tokens, x => x.Type == MapTokenType.Identifier && x.Value == "common/caulk");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Identifier && x.Value == "textures/test/my_texture");
        }

        [Fact]
        public void Tokenize_ShouldReadSignedNumbers()
        {
            var text = """
            +64 -64 0.25 -0.5
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "+64");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "-64");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "0.25");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "-0.5");
        }
    }
}