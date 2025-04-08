using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NReco.VideoConverter;
using Photo_VideoConverter.Model;

namespace Photo_VideoConverter.ViewModel
{
    internal class ConvertStatusViewModel : ObservableObject
    {
        private ConverterSettingsModel _settings;
        private FFMpegConverter Converter;
        private ConvertSettings ConverterSetting;
        public double ProgressIndycator { get; set; } //progress bar for whole conversion
        public double FileProgressIndycator { get; set; } // progress bar for single file conversion
        public string CurrentConvertionFile { get; set; } // name of the file that is currently being converted
        public ObservableCollection<FileDisplayModel> SuccesConversionFileList { get; set; }
        public int NumOfSucceses { get; set; }
        public ObservableCollection<FileDisplayModel> FailedConversionFileList { get; set; }
        public int NumOfFailures { get; set; }
        public string FinishConversionBtnVisibility { get; set; }

        //tokens to cancel or abort conversion
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _abortImportToken;

        //data for progress bar 
        private int NumberOfFilesToCnvert;
        private double SigleFileConvertToProgress;
        // Variables to track work of CountAllFilesRecursively method
        private static bool _promptShown = false;
        private static bool _userWantsToContinue = false;

        //FLAGS
        private bool ConvertionInProgress;

        //COMMANDS
        public RelayCommand CancelConversionCommand { get; }
        public RelayCommand AbortConversionCommand { get; }
        public RelayCommand FinishConversionCommand { get; }

        public ConvertStatusViewModel(ConverterSettingsModel settings) {
            this._settings = settings;
            NumberOfFilesToCnvert = 0;

            ProgressIndycator = 0;
            ConvertionInProgress = true;

            FinishConversionBtnVisibility = "Collapsed";

            SuccesConversionFileList = new ObservableCollection<FileDisplayModel>();
            NumOfSucceses = 0;
            FailedConversionFileList = new ObservableCollection<FileDisplayModel>();
            NumOfFailures = 0;

            Converter = new FFMpegConverter();
            //set up converter settings
            ConverterSetting = new ConvertSettings();
            //setting up out put viedo and audio codec
            ConverterSetting.VideoCodec = _settings.OutputVideoCodec;
            ConverterSetting.AudioCodec = _settings.OutputAudioCodec;

            CancelConversionCommand = new RelayCommand(CancelConversion);
            AbortConversionCommand = new RelayCommand(AbortConversion);
            FinishConversionCommand = new RelayCommand(FinishConversion, CanFinishConversion);
        }

        public async Task ConversationSetupAsync()
        {


            NumberOfFilesToCnvert = CountAllFilesRecursively(_settings.InputPath);
            if (NumberOfFilesToCnvert == 0)
            {
                MessageBox.Show("No files found in the selected folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (NumberOfFilesToCnvert > 1000)   //a lot of files user warning
            {
                var ALotFilesWarning = MessageBox.Show(
                    "The folder contains more than 1000 files.\nDo you want to continue?",
                    "Large Number of Files",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information
                );
                if (ALotFilesWarning == MessageBoxResult.No)
                {
                    Application.Current.MainWindow.DataContext = new ConvertSettingsViewModel();
                    return;
                }
            }
            //Check if the user wants to continue with the conversion
            var CovertsionComformation = MessageBox.Show(
                        $"Found {NumberOfFilesToCnvert} files to convert.\nDo you want to start the conversion?",
                        "Files Found",
                         MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
            if (CovertsionComformation == MessageBoxResult.No)
            {
                Application.Current.MainWindow.DataContext = new ConvertSettingsViewModel();
                return;
            }

            SigleFileConvertToProgress = 100.0 / NumberOfFilesToCnvert;

            //Creating a new folder "Input foldername + Coverted" in the output folder to dump all the converted files
            _settings.OutputPath = Path.Combine(_settings.OutputPath, Path.GetFileName(_settings.InputPath) + "Converted");
            if (!Directory.Exists(_settings.OutputPath))
            {
                Directory.CreateDirectory(_settings.OutputPath);
            }

            _cancellationTokenSource = new CancellationTokenSource();  // initialize the cancellation token
            _abortImportToken = new CancellationTokenSource();

            try
            {
                await StartConversionAsync(_settings.InputPath, _settings.OutputPath, _cancellationTokenSource, _abortImportToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            ConvertionInProgress = false;
            FinishConversionBtnVisibility = "visible";
            OnPropertyChanged(nameof(FinishConversionBtnVisibility));
            FinishConversionCommand.NotifyCanExecuteChanged();
            MessageBox.Show("Conversion completed. \n Click \"finish\" to go back to main menu", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private int CountAllFilesRecursively(string FolderPath, int Depth = 0)
        {
            int fileCount = 0;

            try
            {
                // Count files in the current directory
                fileCount += Directory.GetFiles(FolderPath).Length;

                // If depth limit exceeded and user hasn't been asked yet
                if (Depth >= 10 && !_promptShown)
                {
                    _promptShown = true;

                    var result = MessageBox.Show(
                        "The folder contains more than 10 levels of subdirectories.\nDo you want to continue?",
                        "Deep Folder Structure",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        _userWantsToContinue = true;
                    }
                    else
                    {
                        return fileCount; // Return files counted so far
                    }
                }

                // Recurse into subdirectories (if under Depth limit or user approved)
                foreach (string subDirectory in Directory.GetDirectories(FolderPath))
                {
                    if (Depth < 10 || _userWantsToContinue)
                    {
                        fileCount += CountAllFilesRecursively(subDirectory, Depth + 1);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied to folder:\n{FolderPath}\n\n{ex.Message}", "Access Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while processing folder:\n{FolderPath}\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return fileCount;
        }

        private async Task StartConversionAsync(string InputFolder, string OutputFolder, CancellationTokenSource cancellationToken, CancellationTokenSource abortImportToken)
        {
            // Logic to start the conversion process
            // Use _cancellationTokenSource and _abortImportToken for cancellation and aborting
            string[] FileList = Directory.GetFiles(InputFolder);
            foreach (string File in FileList)
            {
                try
                {
                    if (cancellationToken.Token.IsCancellationRequested)
                    {
                        // Handle cancellation 
                        System.IO.Directory.Delete(_settings.OutputPath, recursive: true);  //delete the output folder
                        Application.Current.MainWindow.DataContext = new ConvertSettingsViewModel();
                        return; // Exit the loop if cancellation is requested
                    }
                    if (abortImportToken.Token.IsCancellationRequested)
                    {
                        ConvertionInProgress = false;
                        return;
                    }
                    CurrentConvertionFile = File;
                    OnPropertyChanged(nameof(CurrentConvertionFile)); //update the UI with the current file being converted
                    //Check file extesnion
                    string Extencsion = Path.GetExtension(File);
                    if (Extencsion == $".{_settings.OutputImageFormat}" || Extencsion == $".{_settings.OutputVideoFormat}") // if file is already in right format we just coppy it
                    {
                        string CopyFile = Path.Combine(OutputFolder, Path.GetFileName(File));
                        System.IO.File.Copy(File, CopyFile, true);
                    } else if (Extencsion == ".mp4" || Extencsion == ".avi" || Extencsion == ".mov" || Extencsion == ".mkv" || Extencsion == ".flv" || Extencsion == ".webm")
                    {
                        await ConvertVideoAsync(File, OutputFolder);
                    }
                    else if (Extencsion == ".png" || Extencsion == ".jpg" || Extencsion == ".webp" || Extencsion == ".bmp")
                    {
                        ConvertImage(File, OutputFolder);
                    }
                    else
                    {
                        string ErrorMessage = $"\n CONVERT ERROR \nFile: {File} \n -E- Unknow file format";
                        Exception Error = new Exception(ErrorMessage);
                        throw Error;
                    }
                    //On succesed conversation
                    NumOfSucceses++;
                    OnPropertyChanged(nameof(NumOfSucceses));
                    FileDisplayModel SuccesConverionInfo = new FileDisplayModel
                    {
                        Name = Path.GetFileNameWithoutExtension(File),
                        Type = Path.GetExtension(File),
                        Path = File,
                        ErrorMessage = ""
                    };

                    SuccesConversionFileList.Add(SuccesConverionInfo);
                    OnPropertyChanged(nameof(SuccesConversionFileList));

                }
                catch (Exception ex)
                {
                    //LogError to txt file
                    LogError(_settings.OutputPath, ex.Message);
                    NumOfFailures++;
                    OnPropertyChanged(nameof(NumOfFailures));

                    string CouseOfError = "unknow";
                    // Error couse is in message behind "-E-" we take it out to display it in datagrid for Errors
                    int index = ex.Message.IndexOf("-E-");
                    if (index >= 0)
                    {
                        CouseOfError = ex.Message.Substring(index + 3);
                    }
                    // make FailModel to display error info in DataGrid
                    FileDisplayModel FailedConverionInfo = new FileDisplayModel
                    {
                        Name = Path.GetFileNameWithoutExtension(File),
                        Type = Path.GetExtension(File),
                        Path = File,
                        ErrorMessage = CouseOfError
                    };

                    FailedConversionFileList.Add(FailedConverionInfo);
                    OnPropertyChanged(nameof(FailedConversionFileList));
                }

                //update progress bar for whole convertion
                ProgressIndycator += SigleFileConvertToProgress;
                OnPropertyChanged(nameof(ProgressIndycator));

            }
            string[] DirectoryList = Directory.GetDirectories(InputFolder);
            foreach (string Directory in DirectoryList)
            {
                string DirectoryName = Path.GetFileName(Directory);
                //Make folder in output folder if doesnt exits
                string OutputDirectory = Path.Combine(OutputFolder, DirectoryName);
                if (!System.IO.Directory.Exists(OutputDirectory)) //check if subdirectroy for output exist
                {
                    System.IO.Directory.CreateDirectory(OutputDirectory); //if dosent exist create 
                }

                await StartConversionAsync(Directory, OutputDirectory, cancellationToken, abortImportToken);  //call method recursevy for conversion of files in subdirectory
            }

        }

        private async Task ConvertVideoAsync(string inputFilePath, string outputDirectory)
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);

                string outputFilePath = Path.Combine(outputDirectory, (fileNameWithoutExtension + "." + _settings.OutputVideoFormat));
                await Task.Run(() =>
                {

                    Converter.ConvertProgress += HandleConversionProgress;      //assing metod to event to track progress of file convertion


                    Converter.ConvertMedia(inputFilePath, null, outputFilePath, _settings.OutputVideoFormat, ConverterSetting);
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
        private void ConvertImage(string inputFilePath, string outputFilePath)
        {
            // Logic to convert video files
            // Use cancellationToken to check for cancellation
        }

        private void LogError(string LogFileDictionary, string ErrorMessage)
        {
            string LogFileName = "ErrorLog.txt";
            string LogFilePath = Path.Combine(LogFileDictionary, LogFileName);
            File.AppendAllText(LogFilePath, ErrorMessage);
        }

        private void HandleConversionProgress(object sender, ConvertProgressEventArgs args)
        {
            FileProgressIndycator = (args.Processed.TotalSeconds / args.TotalDuration.TotalSeconds) * 100;
            OnPropertyChanged(nameof(FileProgressIndycator));
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
            _cancellationTokenSource?.Cancel();
        }
        private void AbortConversion()
        {
            // Logic to abort the conversion process
            var CovertsionComformation = MessageBox.Show(
                        "Are you sure you want to abort the conversion?",
                        "",
                         MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
            if (CovertsionComformation == MessageBoxResult.No)
            {
                return;
            }
            Converter.Stop();
            _abortImportToken?.Cancel();
        }
        private void FinishConversion()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel();
        }
        private bool CanFinishConversion()
        {
            return !ConvertionInProgress;
        }
    }
}
