using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using CodeConverter.PluginStore;

namespace CodeConverter
{
    public class FileModel : ObservableObject
    {
        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        private string filePath;

        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                RaisePropertyChanged("FilePath");
            }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        bool isOriginalCodeChanged;
        private string _originalContent;

        public string OriginalContent
        {
            get
            {
                if (string.IsNullOrEmpty(_originalContent))
                {
                    _originalContent = File.ReadAllText(FilePath);
                }
                return _originalContent;
            }
            set
            {
                _originalContent = value;
                isOriginalCodeChanged = true;
                Task.Factory.StartNew(() =>
                    {
                        TargetContent = ConvertCode(OriginalContent);
                    });
                RaisePropertyChanged("OriginalContent");
            }
        }

        private string _targetContent;
        public string TargetContent
        {
            get
            {
                if (string.IsNullOrEmpty(_targetContent) || isOriginalCodeChanged)
                {
                    _targetContent = ConvertCode(OriginalContent);
                    isOriginalCodeChanged = false;
                }
                return _targetContent;
            }
            set
            {
                _targetContent = value;
                RaisePropertyChanged("TargetContent");
            }
        }

        public string ConvertCode(string proto)
        {
            if (MainWindow.VmLocator.Main.CurrentPlugin != null)
            {
                return MainWindow.VmLocator.Main.CurrentPlugin.ConvertCode(proto);
            }
            return null;
        }
    }
}
