using MapMaker.Core.Models;
using MapMaker.Editor.Commands;
using MapMaker.Editor.Editor;
using System.Windows;
using System.Windows.Input;

namespace MapMaker.Editor.Tools;

public abstract class TransformToolBase : IEditorTool
{
    protected readonly EditorState State;
    protected IInputElement? Viewport;

    private bool _dragging;
    private Point _startMousePos;
    private Brush? _brush;
    private List<BrushFaceSnapshot>? _before;

    public event Action? SceneChanged;
    public event Action? Committed;

    protected TransformToolBase(EditorState state)
    {
        State = state;
    }

    public void SetViewport(IInputElement viewport)
    {
        Viewport = viewport;
    }

    public virtual void OnMouseDown(MouseButtonEventArgs e)
    {
        if (Viewport == null)
            return;

        if (e.ChangedButton != MouseButton.Left)
            return;

        if (State.SelectedBrush == null)
            return;

        _dragging = true;
        _brush = State.SelectedBrush;
        _startMousePos = Mouse.GetPosition(Viewport);

        _before = CreateSnapshot(_brush);

        Mouse.Capture(Viewport);

        e.Handled = true;
    }

    public virtual void OnMouseMove(MouseEventArgs e)
    {
        if (!_dragging || Viewport == null || _brush == null || _before == null)
            return;

        var pos = e.GetPosition(Viewport);

        float deltaX = (float)(pos.X - _startMousePos.X);
        float deltaY = (float)(pos.Y - _startMousePos.Y);

        Restore(_before, _brush);

        ApplyTransform(_brush, deltaX, deltaY);

        SceneChanged?.Invoke();

        e.Handled = true;
    }

    public virtual void OnMouseUp(MouseButtonEventArgs e)
    {
        if (!_dragging)
            return;

        if (e.ChangedButton != MouseButton.Left)
            return;

        if (_brush != null && _before != null)
        {
            var after = CreateSnapshot(_brush);

            if (HasChanged(_before, after))
            {
                var command = CreateCommand(_brush, _before, after);
                State.History.PushExecuted(command);

                State.IsDirty = true;
                Committed?.Invoke();
            }
        }

        _dragging = false;
        _brush = null;
        _before = null;

        Mouse.Capture(null);

        SceneChanged?.Invoke();

        e.Handled = true;
    }

    public void Cancel()
    {
        if (!_dragging)
            return;

        if (_brush != null && _before != null)
            Restore(_before, _brush);

        _dragging = false;
        _brush = null;
        _before = null;

        Mouse.Capture(null);

        SceneChanged?.Invoke();
    }

    protected abstract void ApplyTransform(Brush brush, float deltaX, float deltaY);

    protected abstract IEditorCommand CreateCommand(
        Brush brush,
        List<BrushFaceSnapshot> before,
        List<BrushFaceSnapshot> after);

    private static List<BrushFaceSnapshot> CreateSnapshot(Brush brush)
    {
        return brush.Faces
            .Select(face => new BrushFaceSnapshot(face))
            .ToList();
    }

    private static void Restore(List<BrushFaceSnapshot> snapshots, Brush brush)
    {
        foreach (var snapshot in snapshots)
            snapshot.Restore();

        brush.Invalidate();
    }

    private static bool HasChanged(
        List<BrushFaceSnapshot> before,
        List<BrushFaceSnapshot> after)
    {
        for (int i = 0; i < before.Count; i++)
        {
            if (before[i].P1 != after[i].P1) return true;
            if (before[i].P2 != after[i].P2) return true;
            if (before[i].P3 != after[i].P3) return true;
        }

        return false;
    }
}