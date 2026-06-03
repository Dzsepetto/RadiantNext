using MapMaker.Core.Models;

namespace MapMaker.Editor.Commands;

public sealed class MoveBrushCommand : BrushTransformCommand
{
    public override string Name => "Move Brush";

    public MoveBrushCommand(
        Brush brush,
        List<BrushFaceSnapshot> before,
        List<BrushFaceSnapshot> after)
        : base(brush, before, after)
    {
    }
}