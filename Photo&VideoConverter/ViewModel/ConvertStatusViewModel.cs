using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Photo_VideoConverter.Model;

namespace Photo_VideoConverter.ViewModel
{
    internal class ConvertStatusViewModel : ObservableObject
    {
        private ConverterSettingsModel _settings;
        private string InputFolderName;
        public double ProgressIndycator { get; set; }
        ObservableCollection<FileDisplayModel> SuccesConversionFileList { get; set; }
        ObservableCollection<FileDisplayModel> FailedConversionFileList { get; set; }

        //tokens to cancel or abort conversion
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _abortImportToken;

        private int NumberOfFilesToCnvert { get; set; }
        // Variables to truck work of CountAllFilesRecursively method
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

        public void ConversationSetup()
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
            // Check if the user wants to continue with the conversion
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

            // Start the conversion process
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

        private void StartConversion(CancellationToken cancellationToken, CancellationToken abortImportToken)
        {
            // Logic to start the conversion process
            // Use _cancellationTokenSource and _abortImportToken for cancellation and aborting
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
