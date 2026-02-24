using MapMaker.Core;
using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using MapMaker.Core.IO;
using System.Numerics;
namespace MapMaker.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void CreateBox_ShouldGenerateSixFaces()
        {
            var brush = BrushFactory.CreateBox(
                new Vector3(0, 0, 0),
                new Vector3(128, 128, 128),
                "caulk");

            Assert.Equal(6, brush.Faces.Count);
        }

        [Fact]
        public void Export_ShouldGenerateValidMapText()
        {
            var map = new Map();

            var world = new Entity();
            world.Properties["classname"] = "worldspawn";

            var brush = BrushFactory.CreateBox(
                new Vector3(0, 0, 0),
                new Vector3(128, 128, 128),
                "caulk");

            world.Brushes.Add(brush);
            map.Entities.Add(world);

            var result = MapExporter.Export(map);

            Assert.Contains("\"classname\" \"worldspawn\"", result);
            Assert.Contains("caulk", result);
        }

        [Fact]
        public void Plane_ShouldCalculateCorrectNormal()
        {
            var p1 = new Vector3(0, 0, 0);
            var p2 = new Vector3(1, 0, 0);
            var p3 = new Vector3(0, 1, 0);

            var plane = Plane3D.FromPoints(p1, p2, p3);

            Assert.True(Vector3.Distance(plane.Normal, new Vector3(0, 0, 1)) < 0.0001f);
        }

        [Fact]
        public void PlaneIntersection_ShouldReturnCorrectPoint()
        {
            var p1 = Plane3D.FromPoints(
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0)); // Z=0 plane

            var p2 = Plane3D.FromPoints(
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1)); // X=0 plane

            var p3 = Plane3D.FromPoints(
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 1)); // Y=0 plane

            var result = Plane3D.Intersect(p1, p2, p3);

            Assert.NotNull(result);
            Assert.Equal(Vector3.Zero, result.Value);
        }
        [Fact]
        public void Box_ShouldGenerateEightVertices()
        {
            var brush = BrushFactory.CreateBox(
                new Vector3(0, 0, 0),
                new Vector3(128, 128, 128),
                "caulk");

            var vertices = brush.GenerateVertices();
            var inward = brush.AreNormalsFacingInward();
            Assert.Equal(8, vertices.Count);
        }
        [Fact]
        public void Box_ShouldGeneratePolygonsWithFourVertices()
        {
            var brush = BrushFactory.CreateBox(
                new Vector3(0, 0, 0),
                new Vector3(128, 128, 128),
                "caulk");

            brush.GenerateFacePolygons();

            foreach (var face in brush.Faces)
            {
                Assert.NotNull(face.Polygon);
                Assert.Equal(4, face.Polygon.Vertices.Count);
            }
        }
        [Fact]
        public void Quad_ShouldTriangulateToTwoTriangles()
        {
            var polygon = new Polygon3D(new[]
            {
                new Vector3(0,0,0),
                new Vector3(1,0,0),
                new Vector3(1,1,0),
                new Vector3(0,1,0),
            });

            var triangles = Triangulator.Triangulate(polygon);

            Assert.Equal(2, triangles.Count);
        }

        [Fact]
        public void Brush_Move_ShouldChangeWorldVertices()
        {
            var brush = BrushFactory.CreateBox(
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1),
                "caulk");

            brush.Transform.Position = new Vector3(10, 0, 0);

            var vertices = brush.GetWorldVertices();

            Assert.Contains(vertices, v => v.X >= 10);
        }
        [Fact]
        public void Camera_ShouldProjectPointToScreen()
        {
            var camera = new Camera3D
            {
                Position = new Vector3(0, -500, 0),
                Target = Vector3.Zero,
                AspectRatio = 1f
            };

            var screen = camera.WorldToScreen(
                Vector3.Zero,
                800,
                800);

            Assert.True(screen.X > 300 && screen.X < 500);
        }
    }
}