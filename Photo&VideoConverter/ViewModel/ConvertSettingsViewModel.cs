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
using Photo_VideoConverter.Model;

namespace Photo_VideoConverter.ViewModel
{
    internal class ConvertSettingsViewModel : ObservableObject
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public ObservableCollection<string> VideoFormats { get; set; }
        public ObservableCollection<string> ImageFormats { get; set; }
        private string _selcetedVideoFormat;
        public string SelectedVideoFormat { 
            get {  return _selcetedVideoFormat; }
            set {
                _selcetedVideoFormat = value;
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

        private bool _skipRadioBtn;
        public bool SkipRadioBtn
        {
            get
            {
                return _skipRadioBtn;
            }
            set
            {
                _skipRadioBtn = value;
                OnPropertyChanged(nameof(SelectedVideoFormat));
                ConvertCommand?.NotifyCanExecuteChanged();
            }
        }

        public RelayCommand ChoseInputFolderCommand { get; }
        public RelayCommand ChoseOutputFolderCommand { get; }
        public AsyncRelayCommand ConvertCommand { get;}
        public RelayCommand SwitchToMenuCommand { get;}

        public ConvertSettingsViewModel()
        {
            InputPath = "none";
            OutputPath = "none";
            SkipRadioBtn = true;

            VideoFormats = new ObservableCollection<string> { "mp4", "avi", "mov", "flv", "mpeg" };
            ImageFormats = new ObservableCollection<string> { "png", "jpg", "webp", "bmp" };

            ErrorMessageVisibility = "Collapsed";
            ErrorMessage = "";

            ChoseInputFolderCommand = new RelayCommand(ChoseInputFolder);
            ChoseOutputFolderCommand = new RelayCommand(ChoseOutputFolder);
            ConvertCommand = new AsyncRelayCommand(StartConvertion, CanStartConvertion);
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

        private async Task StartConvertion()
        {
            string VideoCodec;
            string AudioCodec; 
            switch (_selcetedVideoFormat)  //setting up codecs for viedo format
            {
                case "mp4":
                    VideoCodec = "h264";
                    AudioCodec = "ac3";
                    break;
                case "avi":
                    VideoCodec = "h264";
                    AudioCodec = "ac3";
                    break;
                case "mov":
                    VideoCodec = "h264";
                    AudioCodec = "mp3";
                    break;
                case "flv":
                    VideoCodec = "h264";
                    AudioCodec = "mp3";
                    break;
                case "mpeg":
                    VideoCodec = "mpeg2video";
                    AudioCodec = "mp2";
                    break;
                default:
                    MessageBox.Show("Error occured while setting codecs try \nagain or select different video input format.");
                    return;
            }


            ConverterSettingsModel setting = new ConverterSettingsModel {
                InputPath = this.InputPath,
                OutputPath = this.OutputPath,
                OutputImageFormat = _selcetedImagineFormat,
                OutputVideoFormat = _selcetedVideoFormat,
                OutputVideoCodec = VideoCodec,
                OutputAudioCodec = AudioCodec,
                SkipUnknowExtension = SkipRadioBtn
            };

            ConvertStatusViewModel ViewModel = new ConvertStatusViewModel(setting);
            
            Application.Current.MainWindow.DataContext = ViewModel;
            await ViewModel.ConversationSetupAsync();
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
