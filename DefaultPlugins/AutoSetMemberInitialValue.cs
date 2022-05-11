using CodeConverter.PluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefaultPlugins
{
   public class AutoSetMemberInitialValue:IPlugin
    {

        public string ID
        {
            get { return "{6DAC75CD-A0ED-43B1-867B-EE7BBD649C0E}"; }
        }

        public string Name
        {
            get {return "AutoSetMemberInitialValue"; }
        }

        public string Category
        {
            get { return "C#"; }
        }

        public string Manufacture
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Auto set member initial values in c# code"; }
        }

        public string WebAdress
        {
            get { return ""; }
        }

        public string TargetFileType
        {
            get
            {
                return "*.cs";
            }
        }

        public string OriginalFileType
        {
            get { return "*.cs"; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        public string ConvertCode(string originalCode)
        {
            string lineContent = string.Empty;
            string currentMessageName = string.Empty;
            List<string> currentRepeatList = new List<string>();
            List<string> currentPropertyList = new List<string>();
            try
            {
                string[] lineArray = originalCode.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                var csStringBuilder = new StringBuilder();
                foreach (var item in lineArray)
                {
                    if (!item.Contains('=') && !item.Contains("using") && !item.Contains("return") &&
                        (item.Contains("[,]") || !item.Contains(',')) && !item.Contains('(') && !item.Contains("goto") &&
                        item.Contains(';') && item.Contains(' '))
                    {
                        var array = item.Split(new char[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (array.Length == 2)
                        {
                            string newLine = string.Empty;
                            for (int i = 0; i < item.IndexOf(array[0]); i++)
                            {
                                newLine += " ";
                            }

                            newLine += array[0] + " " + array[1] + " = " + "default(" + array[0] + ");";
                            csStringBuilder.AppendLine(newLine);
                        }
                        else
                        {
                            csStringBuilder.AppendLine(item);
                        }
                    }
                    else
                    {
                        csStringBuilder.AppendLine(item);
                    }
                }
                return csStringBuilder.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
