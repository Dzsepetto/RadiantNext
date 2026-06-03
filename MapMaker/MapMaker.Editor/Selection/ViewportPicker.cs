using MapMaker.Core.Models;
using MapMaker.Editor.Editor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MapBrush = MapMaker.Core.Models.Brush;

namespace MapMaker.Editor.Selection;

public sealed class ViewportPicker
{
    private readonly EditorState _state;
    private readonly Viewport3D _viewport;
    private readonly Dictionary<GeometryModel3D, Face> _modelToFace;
    private readonly Dictionary<GeometryModel3D, MapBrush> _modelToBrush;

    public ViewportPicker(
        EditorState state,
        Viewport3D viewport,
        Dictionary<GeometryModel3D, Face> modelToFace,
        Dictionary<GeometryModel3D, MapBrush> modelToBrush)
    {
        _state = state;
        _viewport = viewport;
        _modelToFace = modelToFace;
        _modelToBrush = modelToBrush;
    }

    public PickResult Pick(Point screenPosition)
    {
        PickResult result = PickResult.None();

        VisualTreeHelper.HitTest(
            _viewport,
            null,
            hit =>
            {
                if (hit is not RayMeshGeometry3DHitTestResult meshHit)
                    return HitTestResultBehavior.Continue;

                if (meshHit.ModelHit is not GeometryModel3D model)
                    return HitTestResultBehavior.Continue;

                if (_state.SelectionMode == Editor.SelectionMode.Face &&
                    _modelToFace.TryGetValue(model, out var face))
                {
                    result = PickResult.FromFace(face);
                    return HitTestResultBehavior.Stop;
                }

                if (_state.SelectionMode == Editor.SelectionMode.Object &&
                    _modelToBrush.TryGetValue(model, out var brush))
                {
                    result = PickResult.FromBrush(brush);
                    return HitTestResultBehavior.Stop;
                }

                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(screenPosition));

        return result;
    }
}