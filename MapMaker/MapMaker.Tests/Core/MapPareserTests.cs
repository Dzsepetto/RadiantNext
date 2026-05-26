using MapMaker.Core.IO;

namespace MapMaker.Tests.Core
{
    public class MapParserTests
    {
        [Fact]
        public void Parse_ShouldReadWorldspawnEntity()
        {
            var text = """
            {
            "classname" "worldspawn"
            }
            """;

            var result = MapParser.Parse(text);

            Assert.Empty(result.Issues);
            Assert.Single(result.Map.Entities);
            Assert.Equal("worldspawn", result.Map.Entities[0].Properties["classname"]);
        }

        [Fact]
        public void Parse_ShouldReadBrushFace()
        {
            var text = """
            {
            "classname" "worldspawn"
            {
            ( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) common/case 0 0 0 0.25 0.25
            }
            }
            """;

            var result = MapParser.Parse(text);

            Assert.Empty(result.Issues);

            var entity = result.Map.Entities[0];
            var brush = entity.Brushes[0];
            var face = brush.Faces[0];

            Assert.Equal("common/case", face.Texture);
            Assert.Equal(0, face.ShiftX);
            Assert.Equal(0, face.ShiftY);
            Assert.Equal(0, face.Rotation);
            Assert.Equal(0.25f, face.ScaleX);
            Assert.Equal(0.25f, face.ScaleY);
        }

        [Fact]
        public void Parse_ShouldPreserveExtraFaceParameters()
        {
            var text = """
            {
            "classname" "worldspawn"
            {
            ( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            }
            }
            """;

            var result = MapParser.Parse(text);

            Assert.Empty(result.Issues);

            var face = result.Map.Entities[0].Brushes[0].Faces[0];

            Assert.Equal(4, face.ExtraParameters.Count);
            Assert.Equal("0", face.ExtraParameters[0]);
            Assert.Equal("0", face.ExtraParameters[1]);
            Assert.Equal("0", face.ExtraParameters[2]);
            Assert.Equal("0", face.ExtraParameters[3]);
        }

        [Fact]
        public void Parse_ShouldReportIssueForInvalidFace()
        {
            var text = """
            {
            "classname" "worldspawn"
            {
            ( 0 0 0 ) ( 128 0 0 ) common/case 0 0 0 0.25 0.25
            }
            }
            """;

            var result = MapParser.Parse(text);

            Assert.NotEmpty(result.Issues);
        }
    }
}