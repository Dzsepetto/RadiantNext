using MapMaker.Core;
using MapMaker.Editor.Editor;
using System.Numerics;
using System.Windows.Input;
using System.Windows;

namespace MapMaker.Editor.Tools;

public class BoxTool : IEditorTool
{
    protected readonly EditorState State;
    private IInputElement? _viewport;

    public event Action? SceneChanged;
    public event Action? Committed;

    public BoxTool(EditorState state)
    {
        State = state;
    }

    public void SetViewport(IInputElement element)
    {
        _viewport = element;
    }

    public void OnMouseDown(MouseButtonEventArgs e)
    {
        if (_viewport == null || e.ChangedButton != MouseButton.Left)
            return;

        if (State.CurrentMap == null)
            return;

        float gridSize = State.Grid.Size > 0 ? State.Grid.Size : 64f;

        Vector3 spawnPos = State.Camera.Position + State.Camera.Forward * 200f;
        spawnPos.X = MathF.Round(spawnPos.X / gridSize) * gridSize;
        spawnPos.Y = MathF.Round(spawnPos.Y / gridSize) * gridSize;
        spawnPos.Z = MathF.Round(spawnPos.Z / gridSize) * gridSize;

        float half = gridSize / 2f;
        Vector3 minBounds = spawnPos - new Vector3(half, half, half);
        Vector3 maxBounds = spawnPos + new Vector3(half, half, half);

        var newBrush = BrushFactory.CreateBox(minBounds, maxBounds, "textures/default");

        var targetEntity = State.CurrentMap.Entities.Count > 0
            ? State.CurrentMap.Entities[0]
            : null;

        if (targetEntity != null)
        {
            State.SelectedBrush = newBrush;
            State.SelectedFace = null;

            targetEntity.Brushes.Add(newBrush);
            State.IsDirty = true;

            Committed?.Invoke();
            SceneChanged?.Invoke();
        }

        e.Handled = true;
    }

    public void OnMouseMove(MouseEventArgs e) { }
    public void OnMouseUp(MouseButtonEventArgs e) { }
    public void Cancel() { }
}