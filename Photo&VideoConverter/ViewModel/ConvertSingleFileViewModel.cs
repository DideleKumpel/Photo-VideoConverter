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

namespace Photo_VideoConverter.ViewModel
{
    internal class ConvertSingleFileViewModel : ObservableObject
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public ObservableCollection<string> Formats { get; set; }
        private string _selcetedFormat;
        public string SelectedFormat
        {
            get { return _selcetedFormat; }
            set
            {
                _selcetedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
                //ConvertCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _overwriteSettings;
        public bool OverwriteSettings
        {
            get
            {
                return _overwriteSettings;
            }
            set
            {
                _overwriteSettings = value;
                OnPropertyChanged(nameof(OverwriteSettings));
                if (OverwriteSettings)
                {
                    OutputSaveSettingsVisibility = Visibility.Collapsed;   //if true show output folder settings
                }
                else
                {
                    OutputSaveSettingsVisibility = Visibility.Visible;  //hide all outputfolder settings
                }
                OnPropertyChanged(nameof(OutputSaveSettingsVisibility));
            }
        }
        public Visibility OutputSaveSettingsVisibility { get; set; }

        private bool _saveCopyInDiffrentLocationSetting;
        public bool SaveCopyInDiffrentLocationSetting
        {
            get
            {
                return _saveCopyInDiffrentLocationSetting;
            }
            set
            {
                _saveCopyInDiffrentLocationSetting = value;
                OnPropertyChanged(nameof(SaveCopyInDiffrentLocationSetting));
                if (SaveCopyInDiffrentLocationSetting)
                {
                    ChoseOutputFileBtnVisibility = Visibility.Visible;
                }
                else
                {
                    ChoseOutputFileBtnVisibility = Visibility.Collapsed;
                }
                OnPropertyChanged(nameof(ChoseOutputFileBtnVisibility));
            }
        }

        public Visibility ChoseOutputFileBtnVisibility { get; set; }

        //COMMANDS
        public RelayCommand SelectInputFileCommand { get; }
        public RelayCommand SelectOutputFolderCommand { get; }
        public AsyncRelayCommand ConvertCommand { get; }

        public ConvertSingleFileViewModel()
        {
            OverwriteSettings = true;
            SaveCopyInDiffrentLocationSetting = false;

            SelectInputFileCommand = new RelayCommand(SelectInputFile);
            SelectOutputFolderCommand = new RelayCommand(SelectOutputFolder);
        }

        public void SelectInputFile()
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "Video files (*.mp4, *.avi, *.mov, *.flv, *.mpeg)|*.mp4;*.avi;*.mov;*.flv;*.mpeg|" +
                "Image files (*.png, *.jpg, *.webp, *.bmp)|*.png;*.jpg;*.webp;*.bmp";
            fileDialog.Title = "Select a file";
            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                InputPath = fileDialog.FileName;
                OnPropertyChanged(nameof(InputPath));
                UpdateAvailableOutputFormats();
            }
        }

        public void UpdateAvailableOutputFormats()
        {
            SelectedFormat = null;      //reset selected format
            Formats = new ObservableCollection<string>();
            //mach combobox formats to the format of selected file
            if (InputPath.EndsWith(".mp4") || InputPath.EndsWith(".avi") || InputPath.EndsWith(".mov") || InputPath.EndsWith(".flv") || InputPath.EndsWith(".mpeg"))
            {
                Formats.Add("mp4");
                Formats.Add("avi");
                Formats.Add("mov");
                Formats.Add("flv");
                Formats.Add("mpeg");
            }
            else if (InputPath.EndsWith(".png") || InputPath.EndsWith(".jpg") || InputPath.EndsWith(".webp") || InputPath.EndsWith(".bmp"))
            {
                Formats.Add("png");
                Formats.Add("jpg");
                Formats.Add("webp");
                Formats.Add("bmp");
            }
            OnPropertyChanged(nameof(Formats));
        }
        public void SelectOutputFolder()
        {
            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                OutputPath = folderDialog.FolderName;
                OnPropertyChanged(nameof(OutputPath));
            }
        }
    }
}
