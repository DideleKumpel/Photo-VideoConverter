﻿using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Photo_VideoConverter.Model;
using Photo_VideoConverter.ViewModel;

namespace Photo_VideoConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainMenuViewModel();

            //ConverterSettingsModel model = new ConverterSettingsModel()
            //{
            //    InputPath = "\"C:\\Users\\CEM\\Desktop\\cyk\\received_3698267010412707.mp4\"",
            //    OutputPath = "\"C:\\Users\\CEM\\Desktop\\pyk\"",
            //    OutputVideoFormat = "mkv",
            //    OutputAudioCodec = "ACC",
            //    OutputVideoCodec = "H.265"

            //};
            //ConvertStatusViewModel ViewModel = new ConvertStatusViewModel(model);
            //DataContext = ViewModel;
            //ViewModel.ConversationSetup();
        }
    }
}