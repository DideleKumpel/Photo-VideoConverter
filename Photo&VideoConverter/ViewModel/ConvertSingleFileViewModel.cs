using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //COMMANDS
        public RelayCommand SelectInputFileCommand { get; }
        public RelayCommand SelectOutputFolderCommand { get; }
        public AsyncRelayCommand ConvertCommand { get; }

        public ConvertSingleFileViewModel()
        {
            SelectInputFileCommand = new RelayCommand(SelectInputFile);
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
            }
        }
    }
}
