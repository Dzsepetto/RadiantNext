using MapMaker.Core.IO;
using MapMaker.Core.Models;

namespace MapMaker.Editor.Services;

public class MapDocumentService
{
    public Map? CurrentMap { get; private set; }
    public string? FilePath { get; private set; }
    public bool IsDirty { get; private set; }

    public bool HasDocument => CurrentMap != null;

    public void New()
    {
        CurrentMap = new Map();
        FilePath = null;
        IsDirty = false;
    }

    public void Load(string filePath)
    {
        CurrentMap = MapParser.Load(filePath);

        FilePath = filePath;
        IsDirty = false;
    }

    public void Save()
    {
        if (CurrentMap == null)
            throw new InvalidOperationException("No map document is loaded.");

        if (string.IsNullOrWhiteSpace(FilePath))
            throw new InvalidOperationException("File path is not set. Use SaveAs instead.");

        MapExporter.Save(CurrentMap, FilePath);

        IsDirty = false;
    }

    public void SaveAs(string filePath)
    {
        if (CurrentMap == null)
            throw new InvalidOperationException("No map document is loaded.");

        MapExporter.Save(CurrentMap, filePath);

        FilePath = filePath;
        IsDirty = false;
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

    public void Close()
    {
        CurrentMap = null;
        FilePath = null;
        IsDirty = false;
    }
}