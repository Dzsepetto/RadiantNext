using MapMaker.Core.Models;
using MapMaker.Editor.Models;
using System.Numerics;

namespace MapMaker.Editor.State
{
    public enum SelectionMode
    {
        Object,
        Face
    }

    public enum EditorTool
    {
        Select,
        Move,
        Brush,
        Rotate
    }

    public sealed class EditorState
    {
        public Map CurrentMap { get; set; } = new();

        public Camera3D Camera { get; set; } = new();   
        public EditorGrid Grid { get; } = new();

        public Vector3 CursorWorldPosition { get; set; }

        public EditorTool CurrentTool { get; set; }
            = EditorTool.Select;

        public SelectionMode SelectionMode { get; set; }
            = SelectionMode.Object;

        public object? SelectedObject { get; set; }

        public Brush? SelectedBrush { get; set; }

        public Face? SelectedFace { get; set; }

        public string? CurrentFilePath { get; set; }
        public bool IsDirty { get; set; }


        public void ClearSelection()
        {
            SelectedObject = null;
            SelectedBrush = null;
            SelectedFace = null;
        }
    }
}