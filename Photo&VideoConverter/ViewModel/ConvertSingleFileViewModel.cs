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
using Photo_VideoConverter.Model;
using NReco.VideoConverter;
using System.IO;
using System.Runtime;
using SixLabors.ImageSharp;
using System.Threading;

namespace Photo_VideoConverter.ViewModel
{
    internal class ConvertSingleFileViewModel : ObservableObject
    {
        private ConverterSettingsModel _convertSettings;
        private FFMpegConverter Converter;
        private ConvertSettings ConverterSetting;

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
                ConvertCommand.NotifyCanExecuteChanged();
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
                ConvertCommand?.NotifyCanExecuteChanged();
            }
        }
        public Visibility ChoseOutputFileBtnVisibility { get; set; }

        public string ErrorMessage { get; set; }
        public Visibility ErrorMessageVisibility { get; set; }

        public Visibility ConverterProgressStatusVisibility { get; set; }
        public double ProgressIndycator { get; set; }

        private bool ConvertingInProgress { get; set; }

        //COMMANDS
        public RelayCommand SelectInputFileCommand { get; }
        public RelayCommand SelectOutputFolderCommand { get; }
        public RelayCommand CancelConvertionCommand { get; }
        public AsyncRelayCommand ConvertCommand { get; }
        public RelayCommand SwitchToMenuCommand { get; }


        public ConvertSingleFileViewModel()
        {
            OverwriteSettings = true;
            SaveCopyInDiffrentLocationSetting = false;


            ConverterProgressStatusVisibility = Visibility.Collapsed;
            ConvertingInProgress = false;

            SelectInputFileCommand = new RelayCommand(SelectInputFile);
            SelectOutputFolderCommand = new RelayCommand(SelectOutputFolder);
            ConvertCommand = new AsyncRelayCommand(StartConvertion, CanStartConvertion);
            CancelConvertionCommand = new RelayCommand(CancelConversion, CanCancelConversion);
            SwitchToMenuCommand = new RelayCommand(SwitchToMenu, CanSwitchToMenu);
        }

        private void SelectInputFile()
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
            ConvertCommand.NotifyCanExecuteChanged();
        }

        private void UpdateAvailableOutputFormats()
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
        private void SelectOutputFolder()
        {
            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                OutputPath = folderDialog.FolderName;
                OnPropertyChanged(nameof(OutputPath));
            }
            ConvertCommand.NotifyCanExecuteChanged();
        }

        private async Task StartConvertion()
        {
            _convertSettings = new ConverterSettingsModel();
            _convertSettings.OverWriteExistingFiles = OverwriteSettings;
            _convertSettings.InputPath = InputPath;
            if (!SaveCopyInDiffrentLocationSetting || OverwriteSettings)   //if user save file in the same location or overwrite it
            {
                _convertSettings.OutputPath = Path.GetDirectoryName(InputPath);
            }
            else
            {
                _convertSettings.OutputPath = OutputPath;
            }

            bool IsVideo = false;
            if (InputPath.EndsWith(".mp4") || InputPath.EndsWith(".avi") || InputPath.EndsWith(".mov") || InputPath.EndsWith(".flv") || InputPath.EndsWith(".mpeg"))
            {
                IsVideo = true;
                _convertSettings.OutputVideoFormat = SelectedFormat;
                switch (_convertSettings.OutputVideoFormat)  //setting up codecs for viedo format
                {
                    case "mp4":
                        _convertSettings.OutputVideoCodec = "h264";
                        _convertSettings.OutputAudioCodec = "ac3";
                        break;
                    case "avi":
                        _convertSettings.OutputVideoCodec = "h264";
                        _convertSettings.OutputAudioCodec = "ac3";
                        break;
                    case "mov":
                        _convertSettings.OutputVideoCodec = "h264";
                        _convertSettings.OutputAudioCodec = "mp3";
                        break;
                    case "flv":
                        _convertSettings.OutputVideoCodec = "h264";
                        _convertSettings.OutputAudioCodec = "mp3";
                        break;
                    case "mpeg":
                        _convertSettings.OutputVideoCodec = "mpeg2video";
                        _convertSettings.OutputAudioCodec = "mp2";
                        break;
                    default:
                        MessageBox.Show("Error occured while setting codecs try \nagain or select different video input format.");
                        return;
                }
            }
            else if (InputPath.EndsWith(".png") || InputPath.EndsWith(".jpg") || InputPath.EndsWith(".webp") || InputPath.EndsWith(".bmp"))
            {
                IsVideo = false;
                _convertSettings.OutputImageFormat = SelectedFormat;
            }
            else
            {
                MessageBox.Show("Error occured while setting formats try \nagain or select different file.");
                return;
            }
            string Extension = System.IO.Path.GetExtension(InputPath);
            if (Extension == $".{SelectedFormat}")
            {
                MessageBox.Show("Input and output formats are the same.\nPlease select different output format.");
                return;
            }
            try
            {
                ConvertingInProgress = true;
                CancelConvertionCommand.NotifyCanExecuteChanged();
                SwitchToMenuCommand.NotifyCanExecuteChanged();
                ConverterProgressStatusVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(ConverterProgressStatusVisibility));

                if (IsVideo)
                {
                    Converter = new FFMpegConverter();
                    //set up converter settings
                    ConverterSetting = new ConvertSettings();
                    //setting up out put viedo and audio codec
                    ConverterSetting.VideoCodec = _convertSettings.OutputVideoCodec;
                    ConverterSetting.AudioCodec = _convertSettings.OutputAudioCodec;
                    await ConvertVideoAsync(InputPath, _convertSettings.OutputPath);
                }
                else
                {
                    await ConvertImageAsync(InputPath, _convertSettings.OutputPath);
                }
                if (_convertSettings.OverWriteExistingFiles)
                {
                    System.IO.File.Delete(_convertSettings.InputPath); //if converted without errors delete old file
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured while converting file.\n{ex.Message}");
                ConvertingInProgress = false;
                CancelConvertionCommand.NotifyCanExecuteChanged();
                return;
            }
            MessageBox.Show("Convertion complete.");
            InputPath = null;
            OnPropertyChanged(nameof(InputPath));
            ConvertCommand.NotifyCanExecuteChanged();
            ConvertingInProgress = false;
            SwitchToMenuCommand.NotifyCanExecuteChanged();
            CancelConvertionCommand.NotifyCanExecuteChanged();
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
                    Errors = "Chose input file.";
                }
                else
                {
                    Errors += "\n Chose input file.";
                }
            }
            if (SaveCopyInDiffrentLocationSetting)
            {
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
            }
            if (SelectedFormat == null)
            {
                CanConvert = false;
                if (string.IsNullOrEmpty(Errors))
                {
                    Errors = "Chose output format.";
                }
                else
                {
                    Errors += "\n Chose output format.";
                }
            }
            if (!CanConvert)
            {
                ErrorMessage = Errors;
                ErrorMessageVisibility = Visibility.Visible;
            }
            else
            {
                ErrorMessage = "";
                ErrorMessageVisibility = Visibility.Collapsed;
            }
            OnPropertyChanged(nameof(ErrorMessage));
            OnPropertyChanged(nameof(ErrorMessageVisibility));
            return CanConvert;
        }


        private async Task ConvertVideoAsync(string inputFilePath, string outputDirectory)
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);

                string outputFilePath = Path.Combine(outputDirectory, (fileNameWithoutExtension + "." + _convertSettings.OutputVideoFormat));
                await Task.Run(() =>
                {

                    Converter.ConvertProgress += HandleConversionProgress;      //assing metod to event to track progress of file convertion


                    Converter.ConvertMedia(inputFilePath, null, outputFilePath, _convertSettings.OutputVideoFormat, ConverterSetting);
                });
            }
            catch (FFMpegException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"FFmpeg error : {ex.Message}");
                string ErrorMessage = $"\n CONVERT ERROR \nFile: {inputFilePath} \n -E- FFmpeg error : {ex.Message}";
                Exception Error = new Exception(ErrorMessage);
                throw Error;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine("E- File not found");
                string ErrorMessage = $"\n CONVERT ERROR \nFile: {inputFilePath} \n -E- File not found";
                Exception Error = new Exception(ErrorMessage);
                throw Error;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"E- Directory not found: {ex.Message}");
                string ErrorMessage = $"\n CONVERT ERROR \nFile: {inputFilePath} \n -E- Directory not found: {ex.Message}";
                Exception Error = new Exception(ErrorMessage);
                throw Error;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"E- Acces denied: {ex.Message}");
                string ErrorMessage = $"\n CONVERT ERROR \nFile: {inputFilePath} \n -E- Acces denied: {ex.Message}";
                Exception Error = new Exception(ErrorMessage);
                throw Error;
            }
            catch (IOException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"E- IO error: {ex.Message}");
                string ErrorMessage = $"\n CONVERT ERROR \nFile: {inputFilePath} \n -E- IO error: {ex.Message}";
                Exception Error = new Exception(ErrorMessage);
                throw Error;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"E- Unexpected error: {ex.Message}");
                string ErrorMessage = $"\n CONVERT ERROR \nFile: {inputFilePath} \n -E- Unexpected error: {ex.Message}";
                Exception Error = new Exception(ErrorMessage);
                throw Error;
            }
        }
        private async Task ConvertImageAsync(string inputFilePath, string outputDirectory)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
            string outputFilePath = Path.Combine(outputDirectory, (fileNameWithoutExtension + "." + _convertSettings.OutputImageFormat));

            var image = await Image.LoadAsync(inputFilePath);
            await image.SaveAsync(outputFilePath);
            image.Dispose(); // Dispose of the image to free up resources
        }

        private void HandleConversionProgress(object sender, ConvertProgressEventArgs args)
        {
            ProgressIndycator = (args.Processed.TotalSeconds / args.TotalDuration.TotalSeconds) * 100;
            OnPropertyChanged(nameof(ProgressIndycator));
        }

        private void CancelConversion()
        {
            // Logic to cancel the conversion process
            var CovertsionComformation = MessageBox.Show(
                        "Are you sure you want to cancel the conversion?",
                        "",
                         MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
            if (CovertsionComformation == MessageBoxResult.No)
            {
                return;
            }
            Converter.Stop();
        }

        private bool CanCancelConversion()
        {
            return ConvertingInProgress;
        }

        private void SwitchToMenu()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel();
        }
        private bool CanSwitchToMenu()
        {
            return !ConvertingInProgress;
        }
    }
}
