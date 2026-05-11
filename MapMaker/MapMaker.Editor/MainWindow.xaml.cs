using MapMaker.Core.IO;
using MapMaker.Editor.Input;
using MapMaker.Editor.Logging;
using MapMaker.Editor.State;
using MapMaker.Editor.Views;
using Microsoft.Win32;
using System.Numerics;
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

            Viewport.SetInput(_input, _state);

            this.KeyDown += (s, e) => _input.HandleKeyDown(e);
            this.KeyUp += (s, e) => _input.HandleKeyUp(e);

            CompositionTarget.Rendering += (s, e) =>
            {
                _input.Update();
                Viewport.ApplyCamera(_state.Camera);
            };

            Loaded += (_, _) =>
            {
                foreach (var obj in GridMenu.Items)
                {
                    if (obj is MenuItem item &&
                        item.Header?.ToString() == "16")
                    {
                        item.IsCheckable = true;
                        item.IsChecked = true;
                    }
                }
            };

            _input.SceneChanged += () =>
            {
                Viewport.Refresh();
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

                _state.CurrentMap = map;
                _state.CurrentFilePath = dialog.FileName;
                _state.IsDirty = false;

                Viewport.LoadMap(map);
            }
        }
        private void MoveSelectedBrush(Vector3 delta)
        {
            if (_state.SelectedBrush == null)
                return;

            MapMaker.Core.Editing.BrushMover.Move(_state.SelectedBrush, delta);

            _state.IsDirty = true;

            Viewport.Refresh();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_state.CurrentFilePath))
                return;

            MapExporter.Save(_state.CurrentMap, _state.CurrentFilePath);

            _state.IsDirty = false;
        }
        private void ObjectSelect_Click(object sender, RoutedEventArgs e)
        {
            _state.SelectionMode = MapMaker.Editor.State.SelectionMode.Object;
        }

        private void FaceSelect_Click(object sender, RoutedEventArgs e)
        {
            _state.SelectionMode = MapMaker.Editor.State.SelectionMode.Face;
        }
        private void GridSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item)
                return;

            if (!float.TryParse(item.Header?.ToString(), out var gridSize))
                return;

            _state.Grid.SetSize(gridSize);

            Viewport.SetGridSize(gridSize);

            GridSizeText.Text = $"Grid: {gridSize}";
            
            UpdateGridMenuChecks(item);
        }
        private void UpdateGridMenuChecks(MenuItem selectedItem)
        {
            foreach (var obj in GridMenu.Items)
            {
                if (obj is MenuItem menuItem)
                {
                    menuItem.IsCheckable = true;
                    menuItem.IsChecked = false;
                }
            }

            selectedItem.IsChecked = true;
        }
        private void SelectTool_Click(object sender, RoutedEventArgs e)
        {
            _state.CurrentTool = EditorTool.Select;
        }
        private void MoveTool_Click(object sender, RoutedEventArgs e)
        {
            _state.CurrentTool = EditorTool.Move;
        }
    }
}