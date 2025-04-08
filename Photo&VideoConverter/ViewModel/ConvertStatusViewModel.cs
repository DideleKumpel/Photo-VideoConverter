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
        private string InputFolderName;
        private string ErrorLogPath;
        public double ProgressIndycator { get; set; }
        ObservableCollection<FileDisplayModel> SuccesConversionFileList { get; set; }
        ObservableCollection<FileDisplayModel> FailedConversionFileList { get; set; }

        //tokens to cancel or abort conversion
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _abortImportToken;

        //data for progress bar 
        private int NumberOfFilesToCnvert;
        private double SigleFileConvertToProgress;
        // Variables to track work of CountAllFilesRecursively method
        private static bool _promptShown = false;
        private static bool _userWantsToContinue = false;

        //COMMANDS
        public RelayCommand CancelConversionCommand { get; }
        public RelayCommand AbortConversionCommand { get; }

        public ConvertStatusViewModel(ConverterSettingsModel settings) {
            this._settings = settings;
            NumberOfFilesToCnvert = 0;

            CancelConversionCommand = new RelayCommand(CancelConversion);
            AbortConversionCommand = new RelayCommand(AbortConversion);
        }

        public async Task ConversationSetup()
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
                await StartConversion(_settings.InputPath, _settings.OutputPath, _cancellationTokenSource, _abortImportToken);
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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

        private async Task StartConversion(string InputFolder ,string OutputFolder ,CancellationTokenSource cancellationToken, CancellationTokenSource abortImportToken)
        {
            // Logic to start the conversion process
            // Use _cancellationTokenSource and _abortImportToken for cancellation and aborting
            string[] FileList = Directory.GetFiles(InputFolder);
            foreach (string File in FileList) 
            {
                string Extencsion = Path.GetExtension(File);
                if (Extencsion == $".{_settings.OutputImageFormat}" || Extencsion == $".{_settings.OutputVideoFormat}") // if file is already in right format we just coppy it
                {
                    string CopyFile = Path.Combine(OutputFolder , Path.GetFileName(File));
                    System.IO.File.Copy(File, CopyFile, true);
                    continue;
                }
                if( Extencsion == ".mp4" || Extencsion == ".avi" || Extencsion == ".mov" || Extencsion == ".mkv" || Extencsion == ".flv" || Extencsion == ".webm")
                {
                    await ConvertVideo(File, OutputFolder);
                }else if (Extencsion == ".png" || Extencsion == ".jpg" || Extencsion == ".webp" || Extencsion == ".bmp")
                {
                    await ConvertImage(File, OutputFolder);
                }
                else
                {
                    Console.WriteLine("Unknow file format");
                }
            }
            string[] DirectoryList = Directory.GetDirectories(InputFolder);
            foreach (string Directory in DirectoryList) {
                string DirectoryName = Path.GetFileName(Directory);
                //Make folder in output folder if doesnt exits
                string OutputDirectory = Path.Combine(OutputFolder, DirectoryName);
                if (!System.IO.Directory.Exists(OutputDirectory)) //check if subdirectroy for output exist
                {
                    System.IO.Directory.CreateDirectory(OutputDirectory); //if dosent exist create 
                }

                await StartConversion(Directory, OutputDirectory, cancellationToken, abortImportToken);  //call method recursevy for conversion of files in subdirectory
            }

        }

        private async Task ConvertVideo(string inputFilePath, string outputDirectory)
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);

                string outputFilePath = Path.Combine(outputDirectory, (fileNameWithoutExtension + "." + _settings.OutputVideoFormat));

                var Converter = new FFMpegConverter();

                var ConverterSetting = new ConvertSettings();
                //setting up out put viedo and audio codec
                ConverterSetting.VideoCodec = _settings.OutputVideoCodec;
                ConverterSetting.AudioCodec = _settings.OutputAudioCodec;

            
                Converter.ConvertMedia(inputFilePath, null , outputFilePath, _settings.OutputVideoFormat, ConverterSetting);
            }
            catch (FFMpegException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"FFmpeg error : {ex.Message}");
                throw ex;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine("File not found");
                throw ex;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"Directory not found: {ex.Message}");
                throw ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"Brak uprawnień: {ex.Message}");
                throw ex;
            }
            catch (IOException ex)
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"IO error: {ex.Message}");
                throw ex;
            }
            catch (Exception ex) // Ogólny fallback
            {
                Console.WriteLine("CONVERT ERROR");
                Console.WriteLine($"File: {inputFilePath}");
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw ex;
            }
        }
        private async Task ConvertImage(string inputFilePath, string outputFilePath)
        {
            // Logic to convert video files
            // Use cancellationToken to check for cancellation
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
            _cancellationTokenSource?.Cancel();
            // To do Delete progress files
            Application.Current.MainWindow.DataContext = new ConvertSettingsViewModel();
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
            _abortImportToken?.Cancel();
        }
    }
}
