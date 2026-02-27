using MapMaker.Core.Models;
using MapMaker.Editor.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker.Editor.Selection
{
    public class SelectionService
    {
        private readonly EditorState _state;

        public SelectionService(EditorState state)
        {
            _state = state;
        }

        public void SelectFace(Face face)
        {
            _state.SelectedFace = face;
        }

        public void ClearSelection()
        {
            _state.SelectedFace = null;
            _state.SelectedBrush = null;
        }
    }
}
