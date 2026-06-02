using MapMaker.Core.Editing;
using MapMaker.Core.Models;
using MapMaker.Editor.Editor;

namespace MapMaker.Editor.Tools;

public class RotateTool : TransformToolBase
{
    private const float RotateMouseSensitivity = 0.5f;

    public RotateTool(EditorState state) : base(state)
    {
    }

    protected override void ApplyTransform(Brush brush, float deltaX, float deltaY)
    {
        float degrees = deltaX * RotateMouseSensitivity;

        BrushRotator.RotateAroundCenterZ(brush, degrees);
    }
}