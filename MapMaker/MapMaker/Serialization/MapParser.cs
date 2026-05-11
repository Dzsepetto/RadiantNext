using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using MapMaker.Core.Logger;
using MapMaker.Core.Models;

namespace MapMaker.Core.IO
{
    public static class MapParser
    {
        public static Map Load(string path, ILogger? logger = null)
        {
            var lines = File.ReadAllLines(path);
            var map = new Map();

            Entity? currentEntity = null;
            Brush? currentBrush = null;

            int depth = 0;

            foreach (var raw in lines)
            {
                var line = raw.Trim();

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                if (line == "{")
                {
                    depth++;

                    if (depth == 1)
                        currentEntity = new Entity();
                    else if (depth == 2)
                        currentBrush = new Brush();

                    continue;
                }

                if (line == "}")
                {
                    if (depth == 2 && currentBrush != null)
                    {
                        currentEntity!.Brushes.Add(currentBrush);
                        currentBrush = null;
                    }
                    else if (depth == 1 && currentEntity != null)
                    {
                        map.Entities.Add(currentEntity);
                        currentEntity = null;
                    }

                    depth--;
                    continue;
                }

                // entity property
                if (depth == 1 && line.StartsWith("\""))
                {
                    var matches = Regex.Matches(line, "\"([^\"]*)\"");

                    if (matches.Count >= 2)
                    {
                        var key = matches[0].Groups[1].Value;
                        var value = matches[1].Groups[1].Value;

                        currentEntity!.Properties[key] = value;
                    }

                    continue;
                }

                // brush face
                if (depth == 2 && line.StartsWith("("))
                {
                    try
                    {
                        var face = ParseFace(line);
                        currentBrush!.Faces.Add(face);
                    }
                    catch
                    {
                        logger?.Error($"Failed to parse: {line}");
                    }
                }
            }

            return map;
        }

        private static Face ParseFace(string line)
        {
            var matches = Regex.Matches(line, @"\(\s*([^)]+)\s*\)");

            if (matches.Count < 3)
                throw new FormatException("Invalid face");

            var p1 = ParseVector(matches[0].Groups[1].Value);
            var p2 = ParseVector(matches[1].Groups[1].Value);
            var p3 = ParseVector(matches[2].Groups[1].Value);

            var rest = line.Substring(matches[2].Index + matches[2].Length)
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (rest.Length < 6)
                throw new FormatException("Invalid face params");

            return new Face(
                p1, p2, p3,
                rest[0],
                float.Parse(rest[1], CultureInfo.InvariantCulture),
                float.Parse(rest[2], CultureInfo.InvariantCulture),
                float.Parse(rest[3], CultureInfo.InvariantCulture),
                float.Parse(rest[4], CultureInfo.InvariantCulture),
                float.Parse(rest[5], CultureInfo.InvariantCulture)
            );
        }

        private static Vector3 ParseVector(string str)
        {
            var v = str.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => float.Parse(x, CultureInfo.InvariantCulture))
                .ToArray();

            return new Vector3(v[0], v[1], v[2]);
        }
    }
}