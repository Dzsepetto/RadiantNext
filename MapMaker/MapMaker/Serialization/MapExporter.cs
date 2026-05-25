using System.Globalization;
using System.Text;
using MapMaker.Core.Models;

namespace MapMaker.Core.IO
{
    public static class MapExporter
    {
        public static string Export(Map map)
        {
            var sb = new StringBuilder();

            foreach (var entity in map.Entities)
            {
                sb.AppendLine("{");

                foreach (var kvp in entity.Properties)
                {
                    sb.AppendLine($"\"{kvp.Key}\" \"{kvp.Value}\"");
                }

                foreach (var brush in entity.Brushes)
                {
                    sb.AppendLine("{");

                    foreach (var face in brush.Faces)
                    {
                        var faceLine = string.Format(
                            CultureInfo.InvariantCulture,
                            "( {0} {1} {2} ) ( {3} {4} {5} ) ( {6} {7} {8} ) {9} {10} {11} {12} {13} {14}",
                            face.P1.X, face.P1.Y, face.P1.Z,
                            face.P2.X, face.P2.Y, face.P2.Z,
                            face.P3.X, face.P3.Y, face.P3.Z,
                            face.Texture,
                            face.ShiftX,
                            face.ShiftY,
                            face.Rotation,
                            face.ScaleX,
                            face.ScaleY
                        );

                        if (face.ExtraParameters.Count > 0)
                        {
                            faceLine += " " + string.Join(" ", face.ExtraParameters);
                        }

                        sb.AppendLine(faceLine);
                    }

                    sb.AppendLine("}");
                }

                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        public static void Save(Map map, string filePath)
        {
            var content = Export(map);

            File.WriteAllText(filePath, content);
        }
    }
}