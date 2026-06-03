using MapMaker.Core.Models;
using MapMaker.Editor.Editor;

namespace MapMaker.Editor.Selection;

public sealed class SelectionService
{
    private readonly EditorState _state;

    public SelectionService(EditorState state)
    {
        _state = state;
    }

    public void Select(PickResult result)
    {
        ClearSelection();

        if (result.Face != null)
        {
            _state.SelectedFace = result.Face;
            return;
        }

        if (result.Brush != null)
        {
            _state.SelectedBrush = result.Brush;
        }
    }

    public void ClearSelection()
    {
        _state.SelectedObject = null;
        _state.SelectedFace = null;
        _state.SelectedBrush = null;
    }
}