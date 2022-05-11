using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter.PluginBase
{
    public interface IPlugin
    {
        #region Properties

        string ID { get;}

        string Name { get;}

        string Category { get;}

        string Manufacture { get;}

        string Description { get;}

        string WebAdress { get;}

        string OriginalFileType { get; }

        string TargetFileType { get; }

        string Version { get; }

        #endregion

        #region Methods

        string ConvertCode(string originalCode);

        #endregion
    }
}
