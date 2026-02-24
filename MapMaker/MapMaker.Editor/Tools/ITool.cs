using System.Windows.Input;

namespace MapMaker.Editor.Tools
{
    public interface ITool
    {
        void OnMouseDown(MouseButtonEventArgs e);
        void OnMouseMove(MouseEventArgs e);
        void OnMouseUp(MouseButtonEventArgs e);
    }
}