using System.Windows.Input;
using MapMaker.Editor.State;

namespace MapMaker.Editor.Input
{
    public class InputController
    {
        private readonly EditorState _state;

        public InputController(EditorState state)
        {
            _state = state;
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            const float speed = 10f;

            if (e.Key == Key.W)
                _state.Camera.Position += new System.Numerics.Vector3(0, speed, 0);

            if (e.Key == Key.S)
                _state.Camera.Position += new System.Numerics.Vector3(0, -speed, 0);
        }
    }
}