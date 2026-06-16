using MapMaker.Editor.Diagnostics;
using MapMaker.Editor.Documents;
using MapMaker.Editor.Editor;
using MapMaker.Editor.Input;
using MapMaker.Editor.Logging;
using MapMaker.Editor.Services;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MapMaker.Editor.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// TODO: Refactore whole file to decrese size and improve readability
    public partial class MainWindow : Window
    {
        private EditorState _state = new();
        private InputController _input;
        private readonly MapDocumentService _document = new();
        private readonly EditorDiagnosticsService _diagnostics = new();
        private readonly EditorLogService _log = new();
        private bool _canClose = false;

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

            LoadingService.OnLoadingChanged += (isLoading, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (isLoading)
                    {
                        LoadingText.Text = message;
                        LoadingOverlay.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                });
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
                UpdateUndoRedoUI();
            };

            _input.SceneChanged += async (modifiedObject) =>
            {
                await TriggerSceneUpdateAsync(modifiedObject);
            };

            Closing += MainWindow_Closing;
        }

        #region Undo / Redo UI 

        private async void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (_state.History.CanUndo)
            {
                _state.History.Undo();
                _state.IsDirty = true;

                await TriggerSceneUpdateAsync(null);
            }
        }

        private async void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (_state.History.CanRedo)
            {
                _state.History.Redo();
                _state.IsDirty = true;

                await TriggerSceneUpdateAsync(null);
            }
        }

        #endregion

        private async Task TriggerSceneUpdateAsync(object? modifiedObject)
        {
            if (_state.IsDirty)
            {
                _document.MarkDirty();
                _state.IsDirty = _document.IsDirty;
                UpdateWindowTitle();
            }

            Viewport.RefreshModifiedObject(modifiedObject);

            await RunDiagnosticsAsync();
            UpdateUndoRedoUI();
        }

        private void UpdateUndoRedoUI()
        {
            if (BtnUndo != null) BtnUndo.IsEnabled = _state.History.CanUndo;
            if (BtnRedo != null) BtnRedo.IsEnabled = _state.History.CanRedo;

            if (MenuUndo != null) MenuUndo.IsEnabled = _state.History.CanUndo;
            if (MenuRedo != null) MenuRedo.IsEnabled = _state.History.CanRedo;
        }


        #region Aszinkron Fájlkezelés és Loading

        private async void New_Click(object sender, RoutedEventArgs e)
        {
            if (!await ConfirmSaveIfDirtyAsync())
                return;

           await LoadingService.Show("Creating new map...");


            try
            {
                await Task.Run(() => _document.New());

                SyncStateFromDocument();
                _state.History.Clear();

                Viewport.LoadMap(_document.CurrentMap!);
                UpdateWindowTitle();
                UpdateUndoRedoUI();
                await RunDiagnosticsAsync();
            }
            finally
            {
                LoadingService.Hide();
            }
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            if (!await ConfirmSaveIfDirtyAsync())
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "Map files (*.map)|*.map"
            };

            if (dialog.ShowDialog() != true)
                return;

           await LoadingService.Show("Loading map file, please wait...");

            try
            {
                string filePath = dialog.FileName;
                await Task.Run(() => _document.Load(filePath));

                SyncStateFromDocument();
                _state.History.Clear();

                Viewport.LoadMap(_document.CurrentMap!);

                UpdateWindowTitle();
                UpdateUndoRedoUI();
                await RunDiagnosticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load map.\n\n{ex.Message}",
                    "Load failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoadingService.Hide();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentDocumentAsync();
        }

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentDocumentAsAsync();
        }

        private async Task<bool> SaveCurrentDocumentAsync()
        {
            if (!_document.HasDocument)
                return true;

            if (_document.NeedsSaveAs)
                return await SaveCurrentDocumentAsAsync();

            return await TrySaveAsync(() => Task.Run(() => _document.Save()), "Saving world data, please wait...");
        }

        private async Task<bool> SaveCurrentDocumentAsAsync()
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

            return await TrySaveAsync(() => Task.Run(() => _document.SaveAs(dialog.FileName)), "Saving world data as...");
        }

        private async Task<bool> TrySaveAsync(Func<Task> saveAction, string loadingMessage)
        {
            await LoadingService.Show(loadingMessage);

            try
            {
                await saveAction();

                SyncStateFromDocument();
                UpdateWindowTitle();

                MessageBox.Show(
                    "Map saved successfully.",
                    "Save successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save map.\n\n{ex.Message}",
                    "Save failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
            finally
            {
                LoadingService.Hide();
            }
        }

        private async Task<bool> ConfirmSaveIfDirtyAsync()
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

            return await SaveCurrentDocumentAsync();
        }

        private async void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_document.IsDirty || _canClose)
                return;

            e.Cancel = true;

            bool shouldClose = await ConfirmSaveIfDirtyAsync();
            if (shouldClose)
            {
                _canClose = true;
                Close();
            }
        }

        #endregion

        #region Eszközök és UI Kezelők

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

        #endregion

        #region Állapot és Diagnosztika

        private void SyncStateFromDocument()
        {
            if (_document.CurrentMap != null)
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

        private async Task RunDiagnosticsAsync()
        {
            var currentMap = _document.CurrentMap;
            if (currentMap == null) return;

            var result = await Task.Run(() => _diagnostics.Analyze(currentMap));
            _log.LogDiagnostics(result);
        }

        #endregion
    }
}