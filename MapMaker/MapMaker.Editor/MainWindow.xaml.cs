using MapMaker.Core.IO;
using Microsoft.Win32;
using MapMaker.Editor.Views;
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
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Map files (*.map)|*.map";

            if (dialog.ShowDialog() == true)
            {
                var map = MapParser.Load(dialog.FileName);
                MessageBox.Show($"Entities: {map.Entities.Count}");
                Viewport.LoadMap(map);
            }
        }
    }
}