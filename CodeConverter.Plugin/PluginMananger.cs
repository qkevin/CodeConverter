using CodeConverter.PluginBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter.PluginStore
{
    public class PluginManager
    {
        private static PluginManager _instance;

        public static PluginManager Root
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PluginManager();
                }
                return _instance;
            }
        }

        public string PluginFolder { get; set; }

        public List<IPlugin> PluginList { get; set; }

        public bool InitializeAllPlugins(ref string errorMsg)
        {
            PluginList = new List<IPlugin>();
            if (Directory.Exists(PluginFolder))
            {
                try
                {
                    var files = System.IO.Directory.GetFiles(PluginFolder, "*.dll");
                    foreach (var file in files)
                    {
                        var ass = Assembly.LoadFrom(file);
                        var types = ass.GetExportedTypes();
                        var name = typeof(IPlugin).Module.FullyQualifiedName;
                        foreach (var type in types)
                        {
                            var newName = type.Module.FullyQualifiedName;
                            var pluginType = typeof(IPlugin);
                            if (pluginType.IsAssignableFrom(type))
                            {
                                var plugin = Activator.CreateInstance(type) as IPlugin;
                                PluginList.Add(plugin);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    errorMsg = ex.ToString();
                    return false;
                }
            }
            return true;
        }

    }
}
