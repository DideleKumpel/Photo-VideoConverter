using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Photo_VideoConverter.Model;

namespace Photo_VideoConverter.ViewModel
{
    internal class ConvertStatusViewModel
    {
        private ConverterSettingsModel _settings;
        public double ProgressIndycator { get; set; }
        ObservableCollection<FileDisplayModel> SuccesConversionFileList { get; set; }
        ObservableCollection<FileDisplayModel> FailedConversionFileList { get; set; }

        private int NumberOfFilesToCnvert { get; set; }
        // Variables to truck work of CountAllFilesRecursively method
        private static bool _promptShown = false;
        private static bool _userWantsToContinue = false;

        public ConvertStatusViewModel(ConverterSettingsModel settings) {
            this._settings = settings;
            NumberOfFilesToCnvert = 0;
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

            // Start the conversion process
        }
        public int CountAllFilesRecursively(string FolderPath, int Depth = 0)
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
    }
}
