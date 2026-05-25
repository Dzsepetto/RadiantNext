using MapMaker.Core;
using MapMaker.Core.IO;
using MapMaker.Core.Models;
using MapMaker.Core.Validation;
using System.Numerics;

RunGeneratedBoxRoundtripTest();
RunRealMapRoundtripTest("test.map", "test_roundtrip.map");
RunRealMapValidationTest("test.map");

static void RunGeneratedBoxRoundtripTest()
{
    PrintHeader("Generated box roundtrip test");

    var map = new Map();

    var world = new Entity();
    world.Properties["classname"] = "worldspawn";

    var brush = BrushFactory.CreateBox(
        new Vector3(-64, -64, 0),
        new Vector3(64, 64, 128),
        "common/case"
    );

    world.Brushes.Add(brush);
    map.Entities.Add(world);

    var mapText = MapExporter.Export(map);
    File.WriteAllText("generated_box.map", mapText);

    var parsed = MapParser.Parse(mapText);
    var roundtripText = MapExporter.Export(parsed.Map);
    File.WriteAllText("generated_box_roundtrip.map", roundtripText);

    PrintParseIssues(parsed.Issues);

    Console.WriteLine($"Entities: {parsed.Map.Entities.Count}");
    Console.WriteLine($"Issues: {parsed.Issues.Count}");
    Console.WriteLine("Generated box exported and roundtripped.");
}

static void RunRealMapRoundtripTest(string inputPath, string outputPath)
{
    PrintHeader($"Real map roundtrip test: {inputPath}");

    if (!File.Exists(inputPath))
    {
        Console.WriteLine($"Missing file: {inputPath}");
        return;
    }

    var result = MapParser.LoadResult(inputPath);

    PrintParseIssues(result.Issues);

    var exported = MapExporter.Export(result.Map);
    File.WriteAllText(outputPath, exported);

    Console.WriteLine($"Entities: {result.Map.Entities.Count}");
    Console.WriteLine($"Issues: {result.Issues.Count}");
    Console.WriteLine($"Roundtrip saved: {outputPath}");
}

static void RunRealMapValidationTest(string inputPath)
{
    PrintHeader($"Real map validation test: {inputPath}");

    if (!File.Exists(inputPath))
    {
        Console.WriteLine($"Missing file: {inputPath}");
        return;
    }

    var result = MapParser.LoadResult(inputPath);

    PrintParseIssues(result.Issues);

    int totalBrushes = 0;
    int totalErrors = 0;
    int totalWarnings = 0;

    for (int entityIndex = 0; entityIndex < result.Map.Entities.Count; entityIndex++)
    {
        var entity = result.Map.Entities[entityIndex];

        var classname = entity.Properties.TryGetValue("classname", out var value)
            ? value
            : "unknown";

        Console.WriteLine();
        Console.WriteLine($"Entity {entityIndex}: classname={classname}, brushes={entity.Brushes.Count}");

        for (int brushIndex = 0; brushIndex < entity.Brushes.Count; brushIndex++)
        {
            totalBrushes++;

            var brush = entity.Brushes[brushIndex];
            var issues = BrushValidator.Validate(brush);

            int errorCount = issues.Count(i => i.Severity == BrushValidationSeverity.Error);
            int warningCount = issues.Count(i => i.Severity == BrushValidationSeverity.Warning);

            totalErrors += errorCount;
            totalWarnings += warningCount;

            Console.WriteLine(
                $"  Brush {brushIndex}: faces={brush.Faces.Count}, errors={errorCount}, warnings={warningCount}"
            );

            if (issues.Count == 0)
            {
                Console.WriteLine("    OK");
                continue;
            }

            foreach (var issue in issues)
            {
                Console.WriteLine($"    {issue}");
            }

            PrintBrushFaces(brush);
        }
    }

    Console.WriteLine();
    Console.WriteLine(
        $"Validation finished. Brushes: {totalBrushes}, errors: {totalErrors}, warnings: {totalWarnings}"
    );
}

static void PrintBrushFaces(Brush brush)
{
    for (int faceIndex = 0; faceIndex < brush.Faces.Count; faceIndex++)
    {
        var face = brush.Faces[faceIndex];

        Console.WriteLine($"    Face {faceIndex}:");
        Console.WriteLine($"      P1: {face.P1}");
        Console.WriteLine($"      P2: {face.P2}");
        Console.WriteLine($"      P3: {face.P3}");
        Console.WriteLine($"      Texture: {face.Texture}");
        Console.WriteLine($"      Polygon: {(face.Polygon == null ? "NULL" : face.Polygon.Vertices.Count + " vertices")}");
    }
}

static void PrintParseIssues(IEnumerable<object> issues)
{
    foreach (var issue in issues)
    {
        Console.WriteLine(issue);
    }
}

static void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine("==================================================");
    Console.WriteLine(title);
    Console.WriteLine("==================================================");
}