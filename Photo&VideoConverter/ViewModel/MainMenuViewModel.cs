using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Photo_VideoConverter.ViewModel;

namespace Photo_VideoConverter.ViewModel
{
    internal class MainMenuViewModel : ObservableObject
    {
        public RelayCommand SwitchToConvertSettingCommand { get;  }
        public RelayCommand SwitchToConvertSignleFileCommand { get; }

        public MainMenuViewModel()
        {
            SwitchToConvertSettingCommand = new RelayCommand(SwitchToConvertSetting);
            SwitchToConvertSignleFileCommand = new RelayCommand(SwitchToConvertSingleFile);
        }

        private void SwitchToConvertSetting()
        {
            Application.Current.MainWindow.DataContext = new ConvertSettingsViewModel();
        }
        private void SwitchToConvertSingleFile()
        {
            Application.Current.MainWindow.DataContext = new ConvertSingleFileViewModel();
        }


    }
}
