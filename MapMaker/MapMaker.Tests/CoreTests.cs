using System.Numerics;
using MapMaker.Core.IO;
using MapMaker.Core.Parsing;
using MapMaker.Core.Models;

namespace MapMaker.Tests
{
    public class CoreTests
    {
        [Fact]
        public void Tokenizer_ShouldReadEntityProperty()
        {
            string text = """
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
        public void Tokenizer_ShouldReadBrushFaceTokens()
        {
            string text = """
            ( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) caulk 0 0 0 1 1
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Equal(3, tokens.Count(x => x.Type == MapTokenType.LeftParen));
            Assert.Equal(3, tokens.Count(x => x.Type == MapTokenType.RightParen));
            Assert.Contains(tokens, x => x.Type == MapTokenType.Identifier && x.Value == "caulk");
            Assert.Equal(14, tokens.Count(x => x.Type == MapTokenType.Number));
        }

        [Fact]
        public void Tokenizer_ShouldReadNegativeAndDecimalNumbers()
        {
            string text = """
            ( -64.5 0 128.25 )
            """;

            var tokenizer = new MapTokenizer(text);
            var tokens = tokenizer.Tokenize();

            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "-64.5");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "0");
            Assert.Contains(tokens, x => x.Type == MapTokenType.Number && x.Value == "128.25");
        }

        [Fact]
        public void Tokenizer_ShouldReadComments()
        {
            string text = """
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
        public void Parser_ShouldParseWorldspawnEntity()
        {
            string text = """
            {
                "classname" "worldspawn"
            }
            """;

            var result = MapParser.Parse(text);

            Assert.True(result.Success);
            Assert.Single(result.Map.Entities);
            Assert.Equal("worldspawn", result.Map.Entities[0].Properties["classname"]);
        }

        [Fact]
        public void Parser_ShouldParseBrushWithOneFace()
        {
            string text = """
            {
                "classname" "worldspawn"
                {
                    ( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) caulk 0 0 0 1 1
                }
            }
            """;

            var result = MapParser.Parse(text);

            Assert.True(result.Success);
            Assert.Single(result.Map.Entities);

            var entity = result.Map.Entities[0];

            Assert.Single(entity.Brushes);
            Assert.Single(entity.Brushes[0].Faces);

            var face = entity.Brushes[0].Faces[0];

            Assert.Equal("caulk", face.Texture);
            Assert.Equal(Vector3.Zero, face.P1);
            Assert.Equal(new Vector3(128, 0, 0), face.P2);
            Assert.Equal(new Vector3(128, 128, 0), face.P3);
        }

        [Fact]
        public void Parser_ShouldReportErrorForInvalidFace()
        {
            string text = """
            {
                "classname" "worldspawn"
                {
                    ( 0 0 ) ( 128 0 0 ) ( 128 128 0 ) caulk 0 0 0 1 1
                }
            }
            """;

            var result = MapParser.Parse(text);

            Assert.False(result.Success);
            Assert.Contains(result.Issues, x => x.Severity == MapParseIssueSeverity.Error);
        }

        [Fact]
        public void Parser_ThenExport_ShouldKeepEntityAndTexture()
        {
            string text = """
            {
                "classname" "worldspawn"
                {
                    ( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) caulk 0 0 0 1 1
                }
            }
            """;

            var result = MapParser.Parse(text);

            Assert.True(result.Success);

            string exported = MapExporter.Export(result.Map);

            Assert.Contains("\"classname\" \"worldspawn\"", exported);
            Assert.Contains("caulk", exported);
        }
    }
}