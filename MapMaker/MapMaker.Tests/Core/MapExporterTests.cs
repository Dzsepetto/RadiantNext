using MapMaker.Core.IO;
using MapMaker.Core.Models;
using System.Numerics;

namespace MapMaker.Tests.Core
{
    public class MapExporterTests
    {
        [Fact]
        public void Export_ShouldWriteWorldspawnEntity()
        {
            var map = new Map();

            var entity = new Entity();
            entity.Properties["classname"] = "worldspawn";

            map.Entities.Add(entity);

            var output = MapExporter.Export(map);

            Assert.Contains("\"classname\" \"worldspawn\"", output);
        }

        [Fact]
        public void Export_ShouldWriteBrushFace()
        {
            var map = new Map();

            var entity = new Entity();
            entity.Properties["classname"] = "worldspawn";

            var brush = new Brush();

            brush.Faces.Add(new Face(
                new Vector3(0, 0, 0),
                new Vector3(128, 0, 0),
                new Vector3(128, 128, 0),
                "common/case",
                0,
                0,
                0,
                0.25f,
                0.25f
            ));

            entity.Brushes.Add(brush);
            map.Entities.Add(entity);

            var output = MapExporter.Export(map);

            Assert.Contains("( 0 0 0 ) ( 128 0 0 ) ( 128 128 0 ) common/case 0 0 0 0.25 0.25", output);
        }

        [Fact]
        public void Export_ShouldPreserveExtraFaceParameters()
        {
            var map = new Map();

            var entity = new Entity();
            entity.Properties["classname"] = "worldspawn";

            var brush = new Brush();

            brush.Faces.Add(new Face(
                new Vector3(0, 0, 0),
                new Vector3(128, 0, 0),
                new Vector3(128, 128, 0),
                "common/case",
                0,
                0,
                0,
                0.25f,
                0.25f,
                new[] { "0", "0", "0", "0" }
            ));

            entity.Brushes.Add(brush);
            map.Entities.Add(entity);

            var output = MapExporter.Export(map);

            Assert.Contains("common/case 0 0 0 0.25 0.25 0 0 0 0", output);
        }

        [Fact]
        public void Export_ShouldBeParseableAgain()
        {
            var map = new Map();

            var entity = new Entity();
            entity.Properties["classname"] = "worldspawn";

            var brush = new Brush();

            brush.Faces.Add(new Face(
                new Vector3(0, 0, 0),
                new Vector3(128, 0, 0),
                new Vector3(128, 128, 0),
                "common/case",
                0,
                0,
                0,
                0.25f,
                0.25f,
                new[] { "0", "0", "0", "0" }
            ));

            entity.Brushes.Add(brush);
            map.Entities.Add(entity);

            var output = MapExporter.Export(map);
            var parsed = MapParser.Parse(output);

            Assert.Empty(parsed.Issues);
            Assert.Single(parsed.Map.Entities);
            Assert.Single(parsed.Map.Entities[0].Brushes);
            Assert.Single(parsed.Map.Entities[0].Brushes[0].Faces);
        }
    }
}