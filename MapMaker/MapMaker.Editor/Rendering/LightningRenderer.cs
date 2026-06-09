using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MapMaker.Editor.Rendering
{
    public static class LightingRenderer
    {
        public static void AddLighting(Model3DGroup scene)
        {
            scene.Children.Add(new AmbientLight(Color.FromRgb(70, 70, 70)));

            scene.Children.Add(new DirectionalLight(
                Color.FromRgb(160, 160, 160),
                new Vector3D(-1, -2, -1.5)));

            scene.Children.Add(new DirectionalLight(
                Color.FromRgb(60, 60, 60),
                new Vector3D(1, 1, 1)));
        }
    }
}