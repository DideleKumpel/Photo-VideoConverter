using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
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
    internal class ConvertSettingsViewModel : ObservableObject
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public ObservableCollection<string> VideoFormats { get; set; }
        public ObservableCollection<string> ImageFormats { get; set; }
        private string _selcetedVideFormat;
        public string SelectedVideoFormat { 
            get {  return _selcetedVideFormat; }
            set {
                _selcetedVideFormat = value;
                OnPropertyChanged(nameof(SelectedVideoFormat));
            }
        }
        private string _selcetedImagineFormat;
        public string SelectedImageFormat {
            get { return _selcetedImagineFormat; }
            set
            {
                _selcetedImagineFormat = value;
                OnPropertyChanged(nameof(_selcetedImagineFormat));
            }
        }

        public string ErrorMessageVisibility { get; set; }
        public string ErrorMessage { get; set; }

        public RelayCommand ChoseInputFolderCommand { get; }
        public RelayCommand ChoseOutputFolderCommand { get; }
        public RelayCommand ConvertCommand { get;}
        public RelayCommand SwitchToMenuCommand { get;}

        public ConvertSettingsViewModel()
        {
            InputPath = "none";
            OutputPath = "none";

            VideoFormats = new ObservableCollection<string> { "mp4", "avi", "mov", "mkv", "flv" };
            ImageFormats = new ObservableCollection<string> { "png", "jpg", "webp", "bmp", "gif" };

            ErrorMessageVisibility = "Collapsed";
            ErrorMessage = "";

            ChoseInputFolderCommand = new RelayCommand(ChoseInputFolder);
            ChoseOutputFolderCommand = new RelayCommand(ChoseOutputFolder);
            ConvertCommand = new RelayCommand(SwitchToMenu);
            SwitchToMenuCommand = new RelayCommand(SwitchToMenu);
        }

        private void ChoseInputFolder()
        {
            var folderDialog = new OpenFolderDialog();
            if( folderDialog.ShowDialog() == true)
            {
                InputPath = folderDialog.FolderName;
                OnPropertyChanged(nameof(InputPath));
            }
        }

        private void ChoseOutputFolder()
        {
            var folderDialog = new OpenFolderDialog();
            if ( folderDialog.ShowDialog() == true)
            {
                OutputPath = folderDialog.FolderName;
                OnPropertyChanged(nameof(OutputPath));
            }
        }

        private void StartConvertion()
        {
            //toDo
        }

        private bool CanStartConvertion()
        {
            return false;
        }

        private void SwitchToMenu()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel();
        }


    }
}
