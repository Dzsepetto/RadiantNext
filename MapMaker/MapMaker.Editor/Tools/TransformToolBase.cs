using MapMaker.Core.Models;
using MapMaker.Editor.Editor;
using System.Numerics;
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
    private List<FaceSnapshot>? _originalFaces;

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

        _originalFaces = _brush.Faces
            .Select(face => new FaceSnapshot(face))
            .ToList();

        Mouse.Capture(Viewport);

        e.Handled = true;
    }

    public virtual void OnMouseMove(MouseEventArgs e)
    {
        if (!_dragging || Viewport == null || _brush == null)
            return;

        var pos = e.GetPosition(Viewport);

        float deltaX = (float)(pos.X - _startMousePos.X);
        float deltaY = (float)(pos.Y - _startMousePos.Y);

        RestoreOriginal();

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

        _dragging = false;
        _brush = null;
        _originalFaces = null;

        Mouse.Capture(null);

        State.IsDirty = true;

        Committed?.Invoke();
        SceneChanged?.Invoke();

        e.Handled = true;
    }

    public void Cancel()
    {
        if (!_dragging)
            return;

        RestoreOriginal();

        _dragging = false;
        _brush = null;
        _originalFaces = null;

        Mouse.Capture(null);

        SceneChanged?.Invoke();
    }

    protected abstract void ApplyTransform(Brush brush, float deltaX, float deltaY);

    private void RestoreOriginal()
    {
        if (_originalFaces == null)
            return;

        foreach (var snapshot in _originalFaces)
        {
            snapshot.Face.SetPoints(
                snapshot.P1,
                snapshot.P2,
                snapshot.P3);
        }

        _brush?.Invalidate();
    }

    private sealed class FaceSnapshot
    {
        public Face Face { get; }
        public Vector3 P1 { get; }
        public Vector3 P2 { get; }
        public Vector3 P3 { get; }

        public FaceSnapshot(Face face)
        {
            Face = face;
            P1 = face.P1;
            P2 = face.P2;
            P3 = face.P3;
        }
    }
}