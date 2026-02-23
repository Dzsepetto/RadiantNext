using MapMaker.Core;
using MapMaker.Core.IO;
using System.Numerics;
using MapMaker.Core.Models;
var map = new Map();

var world = new Entity();
world.Properties["classname"] = "worldspawn";

var brush = BrushFactory.CreateBox(
    new Vector3(-64, -64, 0),
    new Vector3(64, 64, 128),
    "common/case");
brush.GenerateFacePolygons();

world.Brushes.Add(brush);
map.Entities.Add(world);

var mapText = MapExporter.Export(map);

File.WriteAllText("test.map", mapText);

Console.WriteLine("Map exported!");