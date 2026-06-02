using MapMaker.Core.Builders;
using MapMaker.Core.Geometry;
using MapMaker.Core.Models;
using MapMaker.Core.Validation;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MapBrush = MapMaker.Core.Models.Brush;

namespace MapMaker.Editor.Viewports
{
    public static class MapRenderer
    {

        public static void AddMap(
            Model3DGroup scene,
            Map map,
            Dictionary<GeometryModel3D, Face> modelToFace,
            Dictionary<Face, GeometryModel3D> faceToModel,
            Dictionary<GeometryModel3D, MapBrush> modelToBrush,
            Dictionary<MapBrush, List<GeometryModel3D>> brushToModels)
        {
            foreach (var entity in map.Entities)
            {
                foreach (var brush in entity.Brushes)
                {
                    var issues = BrushValidator.Validate(brush);

                    if (issues.Count > 0)
                    {
                        Debug.WriteLine("---- BRUSH VALIDATION ----");

                        foreach (var issue in issues)
                        {
                            Debug.WriteLine(issue.ToString());
                        }
                    }

                    foreach (var face in brush.Faces)
                    {
                        AddFaceModel(
                            scene,
                            brush,
                            face,
                            modelToFace,
                            faceToModel,
                            modelToBrush,
                            brushToModels);
                    }
                   
                }
            }
        }

        private static void AddFaceModel(
            Model3DGroup scene,
            MapBrush brush,
            Face face,
            Dictionary<GeometryModel3D, Face> modelToFace,
            Dictionary<Face, GeometryModel3D> faceToModel,
            Dictionary<GeometryModel3D, MapBrush> modelToBrush,
            Dictionary<MapBrush, List<GeometryModel3D>> brushToModels)
        {
            if (face.Polygon == null)
                return;

            var triangles = Triangulator.Triangulate(face.Polygon);
            var mesh = new MeshGeometry3D();

            foreach (var tri in triangles)
            {
                int start = mesh.Positions.Count;

                mesh.Positions.Add(new Point3D(tri.A.X, tri.A.Y, tri.A.Z));
                mesh.Positions.Add(new Point3D(tri.B.X, tri.B.Y, tri.B.Z));
                mesh.Positions.Add(new Point3D(tri.C.X, tri.C.Y, tri.C.Z));

                mesh.TriangleIndices.Add(start + 0);
                mesh.TriangleIndices.Add(start + 1);
                mesh.TriangleIndices.Add(start + 2);
            }

            var material = new DiffuseMaterial(
                new SolidColorBrush(Color.FromRgb(180, 180, 180)));

            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                BackMaterial = material
            };

            modelToFace[model] = face;
            faceToModel[face] = model;

            modelToBrush[model] = brush;

            if (!brushToModels.TryGetValue(brush, out var models))
            {
                models = new List<GeometryModel3D>();
                brushToModels[brush] = models;
            }

            models.Add(model);

            scene.Children.Add(model);

            AddDebugNormal(scene, face);
        }

        private static void AddDebugNormal(Model3DGroup scene, Face face)
        {
            if (face.Polygon == null)
                return;

            var verts = face.Polygon.Vertices;

            var center = Vector3.Zero;
            foreach (var v in verts)
                center += v;

            center /= verts.Count;

            var normal = face.Plane.Normal;

            var lineMesh = DebugLineBuilder.CreateLine(
                center,
                center + normal * 30f,
                1f);

            scene.Children.Add(new GeometryModel3D
            {
                Geometry = lineMesh,
                Material = new DiffuseMaterial(
                    new SolidColorBrush(Colors.Red))
            });
        }
    }
}