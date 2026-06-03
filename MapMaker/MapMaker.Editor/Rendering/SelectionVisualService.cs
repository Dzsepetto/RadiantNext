using MapMaker.Core.Models;
using MapMaker.Editor.Editor;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MapBrush = MapMaker.Core.Models.Brush;

namespace MapMaker.Editor.Rendering;

public sealed class SelectionVisualService
{
    private readonly EditorState _state;
    private readonly Dictionary<GeometryModel3D, Face> _modelToFace;
    private readonly Dictionary<Face, GeometryModel3D> _faceToModel;
    private readonly Dictionary<MapBrush, List<GeometryModel3D>> _brushToModels;

    private static readonly Material DefaultMaterial =
        new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(180, 180, 180)));

    private static readonly Material FaceSelectedMaterial =
        new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));

    private static readonly Material BrushSelectedMaterial =
        new DiffuseMaterial(new SolidColorBrush(Colors.Orange));

    public SelectionVisualService(
        EditorState state,
        Dictionary<GeometryModel3D, Face> modelToFace,
        Dictionary<Face, GeometryModel3D> faceToModel,
        Dictionary<MapBrush, List<GeometryModel3D>> brushToModels)
    {
        _state = state;
        _modelToFace = modelToFace;
        _faceToModel = faceToModel;
        _brushToModels = brushToModels;
    }

    public void Clear()
    {
        foreach (var model in _modelToFace.Keys)
            model.Material = DefaultMaterial;
    }

    public void ApplySelection()
    {
        Clear();

        if (_state.SelectedFace != null &&
            _faceToModel.TryGetValue(_state.SelectedFace, out var faceModel))
        {
            faceModel.Material = FaceSelectedMaterial;
        }

        if (_state.SelectedBrush != null &&
            _brushToModels.TryGetValue(_state.SelectedBrush, out var brushModels))
        {
            foreach (var model in brushModels)
                model.Material = BrushSelectedMaterial;
        }
    }
}