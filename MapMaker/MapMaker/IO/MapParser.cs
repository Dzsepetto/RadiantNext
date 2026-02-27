using System.Globalization;
using System.Numerics;
using MapMaker.Core.Logger;
using MapMaker.Core.Models;

namespace MapMaker.Core.IO
{
    public static class MapParser
    {
        public static Map Load(string path, ILogger? logger = null)
        {
            logger?.Info($"Loading map from: {path}");

            var lines = File.ReadAllLines(path);
            var map = new Map();

            Entity? currentEntity = null;
            Brush? currentBrush = null;

            foreach (var raw in lines)
            {
                var line = raw.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("//"))
                    continue;

                if (line == "{")
                {
                    if (currentEntity == null)
                        currentEntity = new Entity();
                    else
                        currentBrush = new Brush();

                    continue;
                }

                if (line == "}")
                {
                    if (currentBrush != null)
                    {
                        currentEntity!.Brushes.Add(currentBrush);
                        currentBrush = null;
                    }
                    else if (currentEntity != null)
                    {
                        map.Entities.Add(currentEntity);
                        currentEntity = null;
                    }

                    continue;
                }

                // Entity property
                if (line.StartsWith("\""))
                {
                    var parts = line.Split('"', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2)
                        currentEntity!.Properties[parts[0]] = parts[1];

                    continue;
                }

                // Brush face
                if (line.StartsWith("("))
                {
                    var face = ParseFace(line);
                    currentBrush!.Faces.Add(face);
                }
            }

            return map;
        }

        private static Face ParseFace(string line)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(
                line,
                @"\(\s*([^)]+)\s*\)");

            if (matches.Count < 3)
                throw new FormatException("Invalid brush face format.");

            var p1 = ParseVector(matches[0].Groups[1].Value);
            var p2 = ParseVector(matches[1].Groups[1].Value);
            var p3 = ParseVector(matches[2].Groups[1].Value);

            var restStart = matches[2].Index + matches[2].Length;
            var rest = line.Substring(restStart)
                           .Trim()
                           .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var texture = rest[0];

            float shiftX = float.Parse(rest[1], CultureInfo.InvariantCulture);
            float shiftY = float.Parse(rest[2], CultureInfo.InvariantCulture);
            float rotation = float.Parse(rest[3], CultureInfo.InvariantCulture);
            float scaleX = float.Parse(rest[4], CultureInfo.InvariantCulture);
            float scaleY = float.Parse(rest[5], CultureInfo.InvariantCulture);

            return new Face(
                p1,
                p2,
                p3,
                texture,
                shiftX,
                shiftY,
                rotation,
                scaleX,
                scaleY);
        }

        private static Vector3 ParseVector(string str)
        {
            var values = str
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => float.Parse(v, CultureInfo.InvariantCulture))
                .ToArray();

            return new Vector3(values[0], values[1], values[2]);
        }
    }
}