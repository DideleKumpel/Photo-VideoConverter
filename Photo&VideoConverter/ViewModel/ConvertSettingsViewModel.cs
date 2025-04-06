using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Photo_VideoConverter.ViewModel
{
    internal class ConvertSettingsViewModel 
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public ObservableCollection<string> VideoFormts { get; set; }
        public ObservableCollection<string> ImageFormats { get; set; }
        public string SelectedVideoFormat { get; set; }
        public string SelectedImageFormat { get; set; }

        public RelayCommand ConvertCommand { get;}
        public RelayCommand SwitchToMenuCommand { get;}

        public ConvertSettingsViewModel()
        {
            VideoFormts = new ObservableCollection<string> { "mp4", "avi", "mov", "mkv", "flv" };
            ImageFormats = new ObservableCollection<string> { "png", "jpg", "webp", "bmp", "gif" };
            ConvertCommand = new RelayCommand(SwitchToMenu);
            SwitchToMenuCommand = new RelayCommand(SwitchToMenu);
        }

        private void StartConvertion()
        {
            //toDo
        }

        private void SwitchToMenu()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel();
        }


    }
}
