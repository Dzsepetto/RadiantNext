using MapMaker.Core;
using MapMaker.Core.IO;
using MapMaker.Core.Models;
using MapMaker.Core.Validation;
using System.Numerics;

namespace MapMaker.Tests.Core
{
    public class BrushValidatorTests
    {
        [Fact]
        public void Validate_ShouldAcceptGeneratedBoxBrush()
        {
            var brush = BrushFactory.CreateBox(
                new Vector3(-64, -64, 0),
                new Vector3(64, 64, 128),
                "common/case"
            );

            var issues = BrushValidator.Validate(brush);

            Assert.DoesNotContain(issues, i => i.Severity == BrushValidationSeverity.Error);
        }

        [Fact]
        public void Validate_ShouldReportErrorForBrushWithTooFewFaces()
        {
            var brush = new Brush();

            brush.Faces.Add(new Face(
                new Vector3(0, 0, 0),
                new Vector3(128, 0, 0),
                new Vector3(128, 128, 0),
                "common/case"
            ));

            var issues = BrushValidator.Validate(brush);

            Assert.Contains(issues, i => i.Severity == BrushValidationSeverity.Error);
        }

        [Fact]
        public void Validate_ShouldReportErrorForDegenerateFace()
        {
            var brush = BrushFactory.CreateBox(
                new Vector3(-64, -64, 0),
                new Vector3(64, 64, 128),
                "common/case"
            );

            brush.Faces[0].SetPoints(
                new Vector3(0, 0, 0),
                new Vector3(64, 0, 0),
                new Vector3(128, 0, 0)
            );

            var issues = BrushValidator.Validate(brush);

            Assert.Contains(issues, i => i.Severity == BrushValidationSeverity.Error);
        }

        [Fact]
        public void Validate_ShouldWarnForDuplicatePlanes()
        {
            var brush = BrushFactory.CreateBox(
                new Vector3(-64, -64, 0),
                new Vector3(64, 64, 128),
                "common/case"
            );

            var duplicateFace = new Face(
                brush.Faces[0].P1,
                brush.Faces[0].P2,
                brush.Faces[0].P3,
                brush.Faces[0].Texture
            );

            brush.Faces.Add(duplicateFace);

            var issues = BrushValidator.Validate(brush);

            Assert.Contains(issues, i => i.Severity == BrushValidationSeverity.Warning);
        }

        [Fact]
        public void Validate_ShouldAcceptRealRoundtripMapWithoutErrors()
        {
            var text = """
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

            var result = MapParser.Parse(text);
            var brush = result.Map.Entities[0].Brushes[0];

            var issues = BrushValidator.Validate(brush);

            Assert.Empty(result.Issues);
            Assert.DoesNotContain(issues, i => i.Severity == BrushValidationSeverity.Error);
        }
    }
}