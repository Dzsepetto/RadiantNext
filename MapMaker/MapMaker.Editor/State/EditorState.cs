using MapMaker.Core.Geometry;
using MapMaker.Core.Models;

namespace MapMaker.Editor.State
{
    public class EditorState
    {
        public Map CurrentMap { get; set; } = new();
        public Camera3D Camera { get; set; } = new();

        public object? SelectedObject { get; set; }
        public Brush? SelectedBrush { get; set; }
        public Face? SelectedFace { get; set; }
    }
}