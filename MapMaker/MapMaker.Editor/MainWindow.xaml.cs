using MapMaker.Core.IO;
using MapMaker.Editor.Input;
using MapMaker.Editor.Logging;
using MapMaker.Editor.State;
using MapMaker.Editor.Views;
using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapMaker.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EditorState _state = new();
        private InputController _input;
        public MainWindow()
        {
            InitializeComponent();

            _input = new InputController(_state);

            Viewport.SetInput(_input);

            this.KeyDown += (s, e) => _input.HandleKeyDown(e);
            this.KeyUp += (s, e) => _input.HandleKeyUp(e);

            CompositionTarget.Rendering += (s, e) =>
            {
                _input.Update();
                Viewport.ApplyCamera(_state.Camera);
            };

        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Map files (*.map)|*.map";

            if (dialog.ShowDialog() == true)
            {
                var logger = new DebugLogger();
                var map = MapParser.Load(dialog.FileName);
                Viewport.LoadMap(map);
            }
        }

    }
}