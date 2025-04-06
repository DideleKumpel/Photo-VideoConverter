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
                ConvertCommand.NotifyCanExecuteChanged();
            }
        }
        private string _selcetedImagineFormat;
        public string SelectedImageFormat {
            get { return _selcetedImagineFormat; }
            set
            {
                _selcetedImagineFormat = value;
                OnPropertyChanged(nameof(_selcetedImagineFormat));
                ConvertCommand.NotifyCanExecuteChanged();
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
            ConvertCommand = new RelayCommand(SwitchToMenu, CanStartConvertion);
            SwitchToMenuCommand = new RelayCommand(SwitchToMenu);
        }

        private void ChoseInputFolder()
        {
            var folderDialog = new OpenFolderDialog();
            if( folderDialog.ShowDialog() == true)
            {
                InputPath = folderDialog.FolderName;
                OnPropertyChanged(nameof(InputPath));
                ConvertCommand.NotifyCanExecuteChanged();
            }
        }

        private void ChoseOutputFolder()
        {
            var folderDialog = new OpenFolderDialog();
            if ( folderDialog.ShowDialog() == true)
            {
                OutputPath = folderDialog.FolderName;
                OnPropertyChanged(nameof(OutputPath));
                ConvertCommand.NotifyCanExecuteChanged();
            }
        }

        private void StartConvertion()
        {
            //toDo
        }

        private bool CanStartConvertion()
        {
            bool CanConvert = true;
            string Errors = null;
            if (InputPath == null || InputPath == "none")
            {
                CanConvert = false;
                if (string.IsNullOrEmpty(Errors))
                {
                    Errors = "Chose input folder.";
                }
                else
                {
                    Errors += "\n Chose input folder.";
                }
            }
            if (OutputPath == null || OutputPath == "none")
            {
                CanConvert = false;
                if (string.IsNullOrEmpty(Errors))
                {
                    Errors = "Chose output folder.";
                }
                else
                {
                    Errors += "\n Chose output folder.";
                }
            }
            if (CanConvert == true && InputPath == OutputPath)
            {
                CanConvert = false;
                if (string.IsNullOrEmpty(Errors))
                {
                    Errors = "Input and output folders can't be the same.";
                }
                else
                {
                    Errors += "\n Input and output folders can't be the same.";
                }
            }
            if (SelectedVideoFormat == null)
            {
                CanConvert = false;
                if (string.IsNullOrEmpty(Errors))
                {
                    Errors = "Chose output video format.";
                }
                else
                {
                    Errors += "\n Chose output video format.";
                }
            }
            if (SelectedImageFormat == null) {
                CanConvert = false;
                if (string.IsNullOrEmpty(Errors))
                {
                    Errors = "Chose output imagine format.";
                }
                else
                {
                    Errors += "\n Chose output imagine format.";
                }
            }
            if (!CanConvert)
            {
                ErrorMessage = Errors;
                ErrorMessageVisibility = "Visible";
            }
            else
            {
                ErrorMessage = "";
                ErrorMessageVisibility = "Collapsed";
            }
            OnPropertyChanged(nameof(ErrorMessage));
            OnPropertyChanged(nameof(ErrorMessageVisibility));
            return CanConvert;
        }

        private void SwitchToMenu()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel();
        }


    }
}
