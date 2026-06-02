using MapMaker.Core.Editing;
using MapMaker.Core.Models;
using MapMaker.Editor.Editor;
using System.Numerics;

namespace MapMaker.Editor.Tools;

public class MoveTool : TransformToolBase
{
    private const float MouseMoveSensitivity = 1.0f;

    public MoveTool(EditorState state) : base(state)
    {
    }

    protected override void ApplyTransform(Brush brush, float deltaX, float deltaY)
    {
        var delta = new Vector3(
            deltaX * MouseMoveSensitivity,
            -deltaY * MouseMoveSensitivity,
            0);

        BrushMover.MoveRaw(brush, delta);
    }
}