using MapMaker.Core.Models;
using MapMaker.Editor.Models;
using System.Windows.Media.Media3D;

namespace MapMaker.Editor.State
{
    public class EditorState
    {
        public Map CurrentMap { get; set; } = new();
        public object? SelectedObject { get; set; }
        public Brush? SelectedBrush { get; set; }
        public Face? SelectedFace { get; set; }
        public Camera3D Camera { get; set; } = new();
    }
}