using System;
using System.Collections.Generic;
using System.Linq;
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
using CodeConverter.ViewModel;
using CodeConverter.PluginStore;

namespace CodeConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string PluginPath = AppDomain.CurrentDomain.BaseDirectory + "Plugins";
        static MainWindow()
        {
            VmLocator = new ViewModelLocator();
        }

        public static ViewModelLocator VmLocator { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            string errorMsg = string.Empty;
            PluginManager.Root.PluginFolder = PluginPath;
            if (PluginManager.Root.InitializeAllPlugins(ref errorMsg))
            {
                VmLocator.Main.Load();
            }
            else
            {
                MessageBox.Show(string.Format("Load plugins failed, exception:{0}", errorMsg), "Warn");
            }
        }

        private void lstView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstView.SelectedItem != null)
            {
                FileModel fm = lstView.SelectedItem as FileModel;
                VmLocator.Main.StartNotepad(fm.FilePath);
            }
        }

        private void linkText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VmLocator.Main.StartNotepad(linkText.Text);
        }
    }
}
