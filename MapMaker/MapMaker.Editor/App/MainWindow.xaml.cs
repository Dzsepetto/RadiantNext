using MapMaker.Core.IO;
using MapMaker.Editor.Input;
using MapMaker.Editor.Logging;
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
using System.IO;
using System.ComponentModel;
using MapMaker.Editor.Documents;
using MapMaker.Editor.Editor;
using MapMaker.Editor.Rendering;

namespace MapMaker.Editor.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EditorState _state = new();
        private InputController _input;
        private readonly MapDocumentService _document = new();
        public MainWindow()
        {
            InitializeComponent();

            _input = new InputController(_state);

            Viewport.SetInput(_input, _state);


            this.PreviewKeyDown += (s, e) => _input.HandleKeyDown(e);
            this.PreviewKeyUp += (s, e) => _input.HandleKeyUp(e);

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
                if (_state.IsDirty)
                {
                    _document.MarkDirty();
                    _state.IsDirty = _document.IsDirty;
                    UpdateWindowTitle();
                }

                Viewport.Refresh();
            };

            Closing += MainWindow_Closing;

        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmSaveIfDirty())
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "Map files (*.map)|*.map"
            };

            if (dialog.ShowDialog() != true)
                return;

            _document.Load(dialog.FileName);

            _state.CurrentMap = _document.CurrentMap!;
            _state.CurrentFilePath = _document.FilePath;
            _state.IsDirty = _document.IsDirty;

            Viewport.LoadMap(_document.CurrentMap!);

            Title = $"MapMaker Radiant - {System.IO.Path.GetFileName(dialog.FileName)}";
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentDocument();
        }

        private bool SaveCurrentDocument()
        {
            if (!_document.HasDocument)
                return true;

            if (_document.NeedsSaveAs)
                return SaveCurrentDocumentAs();

            _document.Save();

            SyncStateFromDocument();
            UpdateWindowTitle();

            return true;
        }

        private bool SaveCurrentDocumentAs()
        {
            if (!_document.HasDocument)
                return true;

            var dialog = new SaveFileDialog
            {
                Filter = "Map files (*.map)|*.map",
                DefaultExt = ".map",
                AddExtension = true
            };

            if (dialog.ShowDialog() != true)
                return false;

            _document.SaveAs(dialog.FileName);

            SyncStateFromDocument();
            UpdateWindowTitle();

            return true;
        }
        private void ObjectSelect_Click(object sender, RoutedEventArgs e)
        {
            _state.SelectionMode = Editor.SelectionMode.Object;
        }

        private void FaceSelect_Click(object sender, RoutedEventArgs e)
        {
            _state.SelectionMode = Editor.SelectionMode.Face;
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
        private void RotateTool_Click(object sender, RoutedEventArgs e)
        {
            _state.CurrentTool = EditorTool.Rotate;
        }

        private bool ConfirmSaveIfDirty()
        {
            if (!_document.IsDirty)
                return true;

            var result = MessageBox.Show(
                "There are unsaved changes. Save before continuing?",
                "Unsaved changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.No)
                return true;

            return SaveCurrentDocument();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!ConfirmSaveIfDirty())
                e.Cancel = true;
        }

        private void SyncStateFromDocument()
        {
            _state.CurrentMap = _document.CurrentMap;
            _state.CurrentFilePath = _document.FilePath;
            _state.IsDirty = _document.IsDirty;
        }

        private void UpdateWindowTitle()
        {
            var fileName = string.IsNullOrWhiteSpace(_document.FilePath)
                ? "Untitled"
                : System.IO.Path.GetFileName(_document.FilePath);

            var dirtyMark = _document.IsDirty ? "*" : "";

            Title = $"MapMaker Radiant - {fileName}{dirtyMark}";
        }
    }
}