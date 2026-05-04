using MapMaker.Core;
using MapMaker.Core.IO;
using MapMaker.Core.Models;
using System.Numerics;

var map = new Map();

var world = new Entity();
world.Properties["classname"] = "worldspawn";

var brush = BrushFactory.CreateBox(
    new Vector3(-64, -64, 0),
    new Vector3(64, 64, 128),
    "common/case");

world.Brushes.Add(brush);
map.Entities.Add(world);

var mapText = MapExporter.Export(map);

File.WriteAllText("test.map", mapText);

Console.WriteLine("Map exported!");