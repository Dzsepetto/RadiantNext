using MapMaker.Core.Models;
using MapBrush = MapMaker.Core.Models.Brush;

namespace MapMaker.Editor.Selection;

public sealed class PickResult
{
    public Face? Face { get; }
    public MapBrush? Brush { get; }

    public bool HasHit => Face != null || Brush != null;

    private PickResult(Face? face, MapBrush? brush)
    {
        Face = face;
        Brush = brush;
    }

    public static PickResult None() => new(null, null);

    public static PickResult FromFace(Face face) => new(face, null);

    public static PickResult FromBrush(MapBrush brush) => new(null, brush);
}