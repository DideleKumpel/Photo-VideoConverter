using System.Collections.ObjectModel;
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

namespace Photo_VideoConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _readDirectory = string.Empty;
        private string _writeDirectory = string.Empty;

        private ObservableCollection<string> _VideoFormatList = new ObservableCollection<string> { "mp4", "avi", "mov", "mkv", "flv" };
        private ObservableCollection<string> _PhotoFormatList = new ObservableCollection<string> { "png", "jpg", "webp", "bmp", "gif" };

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}