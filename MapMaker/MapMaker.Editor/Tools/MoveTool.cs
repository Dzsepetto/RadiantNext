using MapMaker.Core.Editing;
using MapMaker.Core.Models;
using MapMaker.Editor.Editor;
using System.Numerics;
using MapMaker.Editor.Commands;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System;

namespace MapMaker.Editor.Tools;

public class MoveTool : TransformToolBase
{
    private const float MouseMoveSensitivity = 1.0f;

    public MoveTool(EditorState state) : base(state)
    {
    }

    public override void OnMouseDown(MouseButtonEventArgs e)
    {
        // Ha van kijelölt lap, de nincs kijelölt ecset, trükkösen elindítjuk az ősosztályt
        if (State.SelectedFace != null && State.SelectedBrush == null)
        {
            var parentBrush = FindParentBrush(State.SelectedFace);
            if (parentBrush != null)
            {
                // MEGOLDÁS: Nem a State.SelectedBrush-t írjuk át!
                // Ideiglenesen betesszük a SelectedBrush helyére CSAK az OnMouseDown (indítás) erejéig,
                // majd a base.OnMouseDown után AZONNAL visszaállítjuk null-ra, hogy a kijelölés ne ugorjon át!

                State.SelectedBrush = parentBrush;
                try
                {
                    base.OnMouseDown(e);
                }
                finally
                {
                    State.SelectedBrush = null; // Visszaállítjuk, így a Face kijelölés megmarad a UI-on!
                }
                return;
            }
        }

        // Alapértelmezett eset (amikor tényleg ecsetet mozgatunk)
        base.OnMouseDown(e);
    }

    protected override void ApplyTransform(Brush brush, float deltaX, float deltaY)
    {
        var camera = State.Camera;

        var horizontalForward = new Vector3(camera.Forward.X, camera.Forward.Y, 0);
        if (horizontalForward.LengthSquared() < 0.0001f) horizontalForward = Vector3.UnitY;
        else horizontalForward = Vector3.Normalize(horizontalForward);

        var horizontalRight = new Vector3(camera.Right.X, camera.Right.Y, 0);
        if (horizontalRight.LengthSquared() < 0.0001f) horizontalRight = Vector3.UnitX;
        else horizontalRight = Vector3.Normalize(horizontalRight);

        var rawDelta = horizontalRight * deltaX * MouseMoveSensitivity +
                       horizontalForward * -deltaY * MouseMoveSensitivity;

        var snappedDelta = SnapDeltaToGrid(rawDelta, State.Grid.Size);

        // Ha van kijelölt lap, csak azt mozgatjuk
        if (State.SelectedFace != null)
        {
            FaceMover.Move(State.SelectedFace, snappedDelta, State.Grid.Size);
        }
        else
        {
            BrushMover.MoveRaw(brush, snappedDelta);
        }
    }

    private static Vector3 SnapDeltaToGrid(Vector3 delta, float gridSize)
    {
        if (gridSize <= 0) return delta;
        return new Vector3(
            MathF.Round(delta.X / gridSize) * gridSize,
            MathF.Round(delta.Y / gridSize) * gridSize,
            MathF.Round(delta.Z / gridSize) * gridSize);
    }

    protected override IEditorCommand CreateCommand(Brush brush, List<BrushFaceSnapshot> before, List<BrushFaceSnapshot> after)
    {
        return new MoveBrushCommand(brush, before, after);
    }

    private Brush? FindParentBrush(Face face)
    {
        return State.CurrentMap?.Entities
            .SelectMany(entity => entity.Brushes)
            .FirstOrDefault(b => b.Faces.Contains(face));
    }
}