using MapMaker.Core.Models;

namespace MapMaker.Editor.Commands;

public abstract class BrushTransformCommand : IEditorCommand
{
    private readonly Brush _brush;
    private readonly List<BrushFaceSnapshot> _before;
    private readonly List<BrushFaceSnapshot> _after;

    public abstract string Name { get; }

    protected BrushTransformCommand(
        Brush brush,
        List<BrushFaceSnapshot> before,
        List<BrushFaceSnapshot> after)
    {
        _brush = brush;
        _before = before;
        _after = after;
    }

    public void Execute()
    {
        Restore(_after);
    }

    public void Undo()
    {
        Restore(_before);
    }

    private void Restore(List<BrushFaceSnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
            snapshot.Restore();

        _brush.Invalidate();
    }
}