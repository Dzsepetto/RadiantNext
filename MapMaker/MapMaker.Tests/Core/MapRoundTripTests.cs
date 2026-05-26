using MapMaker.Core.IO;
using MapMaker.Core.Validation;

namespace MapMaker.Tests.Core
{
    public class MapRoundtripTests
    {
        [Fact]
        public void Roundtrip_ShouldKeepEntityBrushAndFaceCounts()
        {
            var input = """
            {
            "classname" "worldspawn"
            {
            ( -64 -64 0 ) ( -64 -64 128 ) ( 64 -64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( 64 -64 0 ) ( 64 -64 128 ) ( 64 64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( 64 64 0 ) ( 64 64 128 ) ( -64 64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( -64 64 0 ) ( -64 64 128 ) ( -64 -64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( -64 -64 128 ) ( -64 64 128 ) ( 64 64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( -64 -64 0 ) ( 64 -64 0 ) ( 64 64 0 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            }
            }
            """;

            var firstParse = MapParser.Parse(input);
            var exported = MapExporter.Export(firstParse.Map);
            var secondParse = MapParser.Parse(exported);

            Assert.Empty(firstParse.Issues);
            Assert.Empty(secondParse.Issues);

            Assert.Single(secondParse.Map.Entities);
            Assert.Single(secondParse.Map.Entities[0].Brushes);
            Assert.Equal(6, secondParse.Map.Entities[0].Brushes[0].Faces.Count);
        }

        [Fact]
        public void Roundtrip_ShouldPreserveExtraFaceParameters()
        {
            var input = """
            {
            "classname" "worldspawn"
            {
            ( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) common/case 1 2 3 0.5 0.75 9 8 7 6
            }
            }
            """;

            var firstParse = MapParser.Parse(input);
            var exported = MapExporter.Export(firstParse.Map);
            var secondParse = MapParser.Parse(exported);

            var face = secondParse.Map.Entities[0].Brushes[0].Faces[0];

            Assert.Empty(firstParse.Issues);
            Assert.Empty(secondParse.Issues);

            Assert.Equal("common/case", face.Texture);
            Assert.Equal(1, face.ShiftX);
            Assert.Equal(2, face.ShiftY);
            Assert.Equal(3, face.Rotation);
            Assert.Equal(0.5f, face.ScaleX);
            Assert.Equal(0.75f, face.ScaleY);

            Assert.Equal(new[] { "9", "8", "7", "6" }, face.ExtraParameters);
        }

        [Fact]
        public void Roundtrip_ShouldValidateBoxWithoutErrors()
        {
            var input = """
            {
            "classname" "worldspawn"
            {
            ( -64 -64 0 ) ( -64 -64 128 ) ( 64 -64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( 64 -64 0 ) ( 64 -64 128 ) ( 64 64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( 64 64 0 ) ( 64 64 128 ) ( -64 64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( -64 64 0 ) ( -64 64 128 ) ( -64 -64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( -64 -64 128 ) ( -64 64 128 ) ( 64 64 128 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            ( -64 -64 0 ) ( 64 -64 0 ) ( 64 64 0 ) common/case 0 0 0 0.25 0.25 0 0 0 0
            }
            }
            """;

            var result = MapParser.Parse(input);
            var exported = MapExporter.Export(result.Map);
            var roundtrip = MapParser.Parse(exported);

            var brush = roundtrip.Map.Entities[0].Brushes[0];
            var issues = BrushValidator.Validate(brush);

            Assert.Empty(result.Issues);
            Assert.Empty(roundtrip.Issues);
            Assert.DoesNotContain(issues, i => i.Severity == BrushValidationSeverity.Error);
        }
    }
}