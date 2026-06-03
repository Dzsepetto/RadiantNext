using MapMaker.Core.Models;

namespace MapMaker.Editor.Commands;

public sealed class RotateBrushCommand : BrushTransformCommand
{
    public override string Name => "Rotate Brush";

    public RotateBrushCommand(
        Brush brush,
        List<BrushFaceSnapshot> before,
        List<BrushFaceSnapshot> after)
        : base(brush, before, after)
    {
    }
}