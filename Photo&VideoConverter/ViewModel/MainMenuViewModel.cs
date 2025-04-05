using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Photo_VideoConverter.ViewModel
{
    internal class MainMenuViewModel : ObservableObject
    {
        RelayCommand SwitchToConvertSettingCommand { get;  }
        RelayCommand SwitchToConvertSignleFileCommand { get; }

        public MainMenuViewModel()
        {
            SwitchToConvertSettingCommand = new RelayCommand(SwitchToConvertSetting);
            SwitchToConvertSignleFileCommand = new RelayCommand(SwitchToConvertSingleFile);
        }

        private void SwitchToConvertSetting()
        {
            // Logic to switch to ConvertSettingViewModel
            //to do when start working on the ConvertSettingViewModel
        }
        private void SwitchToConvertSingleFile()
        {
            // Logic to switch to ConvertSingleFileViewModel
            //to do when start working on the ConvertSingleFileViewModel
        }


    }
}
