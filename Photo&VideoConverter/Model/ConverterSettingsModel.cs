using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_VideoConverter.Model
{
    internal class ConverterSettingsModel
    {
        public string InputPath;
        public string OutputPath;
        public string OutputVideoFormat;
        public string OutputVideoCodec;
        public string OutputAudioCodec;
        public string OutputImageFormat;
        public bool SkipUnknowExtension;
    }
}
