using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using CodeConverter.PluginBase;
using CodeConverter.PluginStore;

namespace CodeConverter.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private OpenFileDialog selectFileDialog,chooseExeDialog;
        private FolderBrowserDialog selectFolderDialog;
        private string settingFilePath = AppDomain.CurrentDomain.BaseDirectory + "Setting.ini";
        private bool isLoading = false;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _FilePath = string.Empty;
            NotepadAppPath = string.Empty;

            selectFileDialog = new OpenFileDialog();
            selectFileDialog.FileName = string.Empty;
            selectFileDialog.Filter = "*.*|*.*";

            selectFolderDialog = new FolderBrowserDialog();
            selectFolderDialog.SelectedPath = string.Empty;

            chooseExeDialog = new OpenFileDialog();
            chooseExeDialog.FileName = string.Empty;
            chooseExeDialog.Filter = "Exe File|*.exe";
           
            SelectFileCommand = new RelayCommand(() =>
            {
                if (CurrentPlugin != null)
                {
                    selectFileDialog.Filter = CurrentPlugin.OriginalFileType + "|" + CurrentPlugin.OriginalFileType;
                }
                if (selectFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilePath = selectFileDialog.FileName;
                }
            });

            SelectFolderCommand = new RelayCommand(() =>
            {
                if (selectFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    FilePath = selectFolderDialog.SelectedPath;
                }
            });

            SetAppCommand = new RelayCommand(() =>
            {
                if (chooseExeDialog.ShowDialog() == DialogResult.OK)
                {
                    NotepadAppPath = chooseExeDialog.FileName;
                    SaveConfig();
                }
            });

            ExportCSFile = new RelayCommand(() =>
                {
                   if(FileList!=null)
                   {
                       try
                       {
                           var result = FileList.FirstOrDefault(s => s.IsSelected);
                           File.WriteAllText(System.IO.Path.GetDirectoryName(result.FilePath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(result.FileName) + ".cs", result.TargetContent);
                           //System.Windows.MessageBox.Show("Export successfully");
                       }
                       catch (Exception ex)
                       {
                           System.Windows.MessageBox.Show(ex.ToString());
                       }
                   }
                    
                });
            ExportAll = new RelayCommand(() =>
            {
                if (FileList != null)
                {
                    try
                    {
                        foreach (var item in FileList)
                        {
                            File.WriteAllText(System.IO.Path.GetDirectoryName(item.FilePath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(item.FileName) + ".cs", item.TargetContent);
                        }
                        System.Windows.MessageBox.Show("Export successfully");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.ToString());
                    }
                }

            });
            RemoveSerialize = new RelayCommand(() =>
            {
                foreach (var item in FileList)
                {
                    try
                    {
                        var csFilePath = Path.GetDirectoryName(item.FilePath) + "\\" + Path.GetFileNameWithoutExtension(item.FilePath) + ".cs";
                        var fileContent = File.ReadAllLines(csFilePath);
                        List<string> tempList = new List<string>();
                        bool isFoundMethod = false;
                        for (int i = 0; i < fileContent.Length; i++)
                        {
                            if (fileContent[i].Contains("public Google.ProtocolBuffers.ByteString ToByteString()"))
                            {
                                isFoundMethod = true;
                                i += 3;
                            }
                            else
                            {
                                tempList.Add(fileContent[i]);
                            }
                        }
                        if (isFoundMethod)
                        {
                            File.WriteAllLines(csFilePath, tempList);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.ToString());
                    }
                } 
                System.Windows.MessageBox.Show("Remove Successfully");
            });
            OpenFolder = new RelayCommand(() =>
            {
                foreach (var item in FileList)
                {
                    if (item.IsSelected)
                    {
                        System.Diagnostics.Process.Start(Path.GetDirectoryName(item.FilePath));
                    }
                }
            });
        }

        public void Load()
        {
            this.PluginList = PluginManager.Root.PluginList;
            if (File.Exists(settingFilePath))
            {
                isLoading = true;
                string[] lines = File.ReadAllLines(settingFilePath);
                if (lines != null)
                {
                    if (lines.Length > 0)
                    {
                        NotepadAppPath = lines[0];
                    }
                    if (lines.Length > 1)
                    {
                        FilePath = lines[1];
                    }
                    if (lines.Length > 2)
                    {
                        CurrentPlugin = this.PluginList.FirstOrDefault(x => x.Name == lines[2]);
                    }
                }
                isLoading = false;
            }
        }

        private void SaveConfig()
        {
            if (!isLoading)
            {
                string pluginName = CurrentPlugin != null ? CurrentPlugin.Name : "";
                File.WriteAllLines(settingFilePath, new string[] { NotepadAppPath, FilePath, pluginName });
            }
        }

        private void GetFile(string filePath)
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                bool isFolder = false;
                if (System.IO.Directory.Exists(filePath))
                {
                    isFolder = true;
                }
                else if (System.IO.File.Exists(filePath))
                {
                    isFolder = false;
                }
                else
                {
                    FileList = null;
                    return;
                }

                if (isFolder)
                {
                    var dir = new DirectoryInfo(filePath);
                    List<FileModel> lst = new List<FileModel>();
                    GetFileList(dir, ref lst);
                    FileList = lst;
                }
                else
                {
                    FileList = new List<FileModel>(new FileModel[] { new FileModel() { FileName = Path.GetFileName(filePath), FilePath = filePath } });
                }
            });
        }

        private void GetFileList(DirectoryInfo diroot,ref List<FileModel> list)
        {
            if (CurrentPlugin == null)
                return;
            foreach (FileInfo file in diroot.GetFiles(CurrentPlugin.OriginalFileType))
            {
                list.Add(new FileModel() { FileName = file.Name, FilePath = file.FullName});
            }
            foreach (DirectoryInfo dirSub in diroot.GetDirectories())
            {
                GetFileList(dirSub, ref list);
            }
        }

        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand SelectFolderCommand { get; set; }
        public RelayCommand SetAppCommand { get; set; }
        public RelayCommand ExportCSFile { get; set; }
        public RelayCommand ExportAll { get; set; }

        public RelayCommand RemoveSerialize { get; set; }
        public RelayCommand OpenFolder { get; set; }
        private string filter;

        public string Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                SelectByFilter(filter);
                RaisePropertyChanged("Filter");
            }
        }

        public string NotepadAppPath { get; set; }

        private string _FilePath;

        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                _FilePath = value;
                GetFile(FilePath);
                SaveConfig();
                RaisePropertyChanged("FilePath");
            }
        }

        private List<FileModel> _FileList;

        public List<FileModel> FileList
        {
            get { return _FileList; }
            set
            {
                _FileList = value;
                RaisePropertyChanged("FileList");
            }
        }

        private IPlugin _currentPlugin;

        public IPlugin CurrentPlugin
        {
            get { return _currentPlugin; }
            set
            {
                var temp = _currentPlugin;
                _currentPlugin = value;
                if (temp != _currentPlugin)
                {
                    ConverMethodChanged();
                }
                RaisePropertyChanged("CurrentPlugin");
            }
        }

        private List<IPlugin> _pluginList;

        public List<IPlugin> PluginList
        {
            get { return _pluginList; }
            set
            {
                _pluginList = value;
                RaisePropertyChanged("PluginList");
            }
        }

        private void ConverMethodChanged()
        {
            GetFile(FilePath);

            if (FileList != null && FileList.Count > 0)
            {
                FileList.ForEach(x => x.TargetContent = null);
            }
        }

        private void SelectByFilter(string filter)
        {
            if (FileList == null)
                return;
            var result = (from f in FileList
                          where f.FileName.Contains(filter)
                          select f).ToList();
            FileList.ForEach(s => s.IsSelected = false);
            result.ForEach(s => s.IsSelected = true);
        }

        public void StartNotepad(string file)
        {
            if (System.IO.File.Exists(NotepadAppPath))
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = NotepadAppPath;
                p.StartInfo.Arguments = file;
                p.Start();
            }
            else
            {
                if (File.Exists(file))
                {
                    System.Diagnostics.Process.Start(file);
                }
            }
        }
    }
}