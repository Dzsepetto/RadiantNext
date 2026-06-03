using MapMaker.Core.Editing;
using MapMaker.Core.Models;
using MapMaker.Editor.Commands;
using MapMaker.Editor.Editor;
using System.Windows.Input;

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

        float snapAngle = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)
            ? 45f
            : 15f;

        degrees = SnapAngle(degrees, snapAngle);

        BrushRotator.RotateAroundCenterZ(brush, degrees);
    }

    protected override IEditorCommand CreateCommand(
        Brush brush,
        List<BrushFaceSnapshot> before,
        List<BrushFaceSnapshot> after)
    {
        return new RotateBrushCommand(brush, before, after);
    }

    private static float SnapAngle(float degrees, float snap)
    {
        if (snap <= 0)
            return degrees;

        return MathF.Round(degrees / snap) * snap;
    }
}