using CodeConverter.PluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefaultPlugins
{
   public class ProtoConverter: IPlugin
    {
       private readonly string id = "{AEA75564-4459-42AF-B952-26C0C0655541}";
        public string ID
        {
            get { return id; }
        }

        public string Name
        {
            get { return "Proto Converter"; }
        }

        public string Category
        {
            get { return "Proto"; }
        }

        public string Manufacture
        {
            get { return }
        }

        public string Description
        {
            get { return "Google protocal buffer to c# code"; }
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
            get { return "*.proto"; }
        }



        public string Version
        {
            get { return "1.0.0"; }
        }

        public string ConvertCode(string proto)
        {
            int lineNumber = 0;
            string lineContent = string.Empty;
            string currentMessageName = string.Empty;
            List<string> currentRepeatList = new List<string>();
            List<string> currentPropertyList = new List<string>();
            try
            {
                string[] lineArray = proto.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var csStringBuilder = new StringBuilder();
                int index = 1;
                bool messageBegined = false;
                bool isEnumBegined = false;
                foreach (var item in lineArray)
                {
                    lineNumber++;
                    lineContent = item;
                    if (item.Trim().StartsWith("package ") && !item.Trim().StartsWith("//"))
                    {
                        var elements = item.Split(new char[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        csStringBuilder.Append("using System;\r\nusing System.Linq;\r\nusing System.Collections.Generic;\r\nusing Liquid.Typhon.DataLayer.DataCloudClient;\r\nnamespace " + elements[1] + "{\r\n");
                    }
                    else if (item.Trim().StartsWith("message ") && !item.Trim().StartsWith("//"))
                    {
                        if (isEnumBegined)
                        {
                            csStringBuilder.Append("}\r\n");
                            isEnumBegined = false;
                        }
                        if (messageBegined)
                        {
                            AddClassTailLines(ref csStringBuilder, currentMessageName, currentRepeatList, currentPropertyList);
                            messageBegined = false;
                        }
                        currentPropertyList.Clear();
                        currentRepeatList.Clear();
                        var elements = item.Split(new char[] { ' ', '{' }, StringSplitOptions.RemoveEmptyEntries);
                        currentMessageName = elements[1];
                        csStringBuilder.Append("[ProtoBuf.ProtoContract]\r\npublic partial class " + elements[1] + " : BaseProto\r\n{\r\n");
                        csStringBuilder.Append("#region Properties\r\n");
                        index = 1;
                        messageBegined = true;
                    }
                    else if (item.Trim().StartsWith("enum ") && !item.Trim().StartsWith("//"))
                    {
                        if (isEnumBegined)
                        {
                            csStringBuilder.Append("}\r\n");
                            isEnumBegined = false;
                        }
                        if (messageBegined)
                        {
                            AddClassTailLines(ref csStringBuilder, currentMessageName, currentRepeatList, currentPropertyList);
                            messageBegined = false;
                        }
                        var elements = item.Split(new char[] { ' ', '{' }, StringSplitOptions.RemoveEmptyEntries);
                        csStringBuilder.Append("public enum " + elements[1] + "\r\n{\r\n");
                        isEnumBegined = true;
                    }
                    else if ((item.Trim().StartsWith("optional ") || item.Trim().StartsWith("required ")) && !item.Trim().StartsWith("//"))
                    {
                        if (isEnumBegined)
                        {
                            csStringBuilder.Append("}\r\n");
                            isEnumBegined = false;
                        }
                        var elements = item.Split(new char[] { ' ', '\t', '=' }, StringSplitOptions.RemoveEmptyEntries);
                        string valueType = ConvertType(elements[1]);
                        string propertyName = elements[2].Substring(0, 1).ToUpper() + elements[2].Substring(1);
                        propertyName = propertyName.Replace("_", "");
                        currentPropertyList.Add(propertyName + "|" + valueType);
                        string defaultValue = valueType == "string" ? "\"\"" : "default(" + valueType + ")";
                        csStringBuilder.Append(string.Format("private {0} _{1} = {2};\r\n", valueType, propertyName, defaultValue));
                        csStringBuilder.Append(string.Format("[ProtoBuf.ProtoMember({0})]\r\n", index++));
                        csStringBuilder.Append("public bool Has" + propertyName + " {get; set;}\r\n");
                        if (item.Trim().StartsWith("optional "))
                        {
                            csStringBuilder.Append(string.Format("[ProtoBuf.ProtoMember({0})]\r\n", index++));
                        }
                        else if (item.Trim().StartsWith("required "))
                        {
                            csStringBuilder.Append(string.Format("[ProtoBuf.ProtoMember({0}, IsRequired = true)]\r\n", index++));
                        }
                        csStringBuilder.Append("public " + valueType + " " + propertyName + " \r\n{\r\nget\r\n{\r\nreturn _" + propertyName + ";\r\n}\r\n");
                        csStringBuilder.Append("set\r\n{\r\n _" + propertyName + "=value;\r\n");
                        csStringBuilder.Append("Has" + propertyName + "=true;\r\n");
                        csStringBuilder.Append("Notify(\"" + propertyName + "\");\r\n}\r\n}\r\n");
                    }
                    else if (item.Trim().StartsWith("repeated ") && !item.Trim().StartsWith("//"))
                    {
                        if (isEnumBegined)
                        {
                            csStringBuilder.Append("}\r\n");
                            isEnumBegined = false;
                        }
                        string property = CreateListPropertiesReturnProperty(ref csStringBuilder, item, ref index);
                        currentRepeatList.Add(property);
                    }
                    else
                    {
                        if (isEnumBegined && !item.Trim().StartsWith("//") && item.Contains("="))
                        {
                            string enumValue = item.Replace(';', ',');
                            csStringBuilder.Append(enumValue + "\r\n");
                        }
                    }
                }
                if (isEnumBegined)
                {
                    csStringBuilder.Append("}\r\n");
                    isEnumBegined = false;
                }
                if (messageBegined)
                {
                    AddClassTailLines(ref csStringBuilder, currentMessageName, currentRepeatList, currentPropertyList);
                }
                csStringBuilder.Append("}\r\n");
                return csStringBuilder.ToString();
            }
            catch (Exception ex)
            {
                //System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() =>
                //{
                //    MessageBox.Show(string.Format("Convert to CS file failed, proto line number {0}, line content [\"{1}\"], error: {2}", lineNumber, lineContent, ex.ToString()));
                //}));
                return null;
            }
        }

        private string ConvertType(string type)
        {
            switch (type.ToLower())
            {
                case "int64":
                    return "long";
                case "int32":
                case "sint32":
                    return "int";
                case "uint64":
                    return "UInt64";
                case "bytes":
                    return "byte[]";
                default:
                    return type;
            }
        }

        private void AddClassTailLines(ref StringBuilder csStringBuilder, string currentMessageName, List<string> currentRepeatList, List<string> currentPropertyList)
        {
            csStringBuilder.Append("#endregion\r\n\r\n");

            CreateConstructorMethod(ref csStringBuilder, currentRepeatList, currentMessageName);

            //Create static constructor
            csStringBuilder.Append("static " + currentMessageName + "()\r\n{\r\n//SetFeedServiceBaseName<" + currentMessageName + ">(\"\");\r\n//SetWrapperTag<" + currentMessageName + ">(ReactorWrapper.ReactorWrapperTag);\r\n//SetThisTypeTag<" + currentMessageName + ">(\"\");\r\n}\r\n");

            AddSetPropertyValueMethod(ref csStringBuilder, currentPropertyList, currentMessageName);

            AddClearListValueMethod(ref csStringBuilder, currentRepeatList, currentMessageName);

            csStringBuilder.Append("#region Custom area\r\n");
            //Create static CreateBuilder method
            csStringBuilder.Append("public static " + currentMessageName + " CreateBuilder()\r\n{\r\nreturn new " + currentMessageName + "();\r\n}\r\n");

            //Create ToBuilder() method
            csStringBuilder.Append("public " + currentMessageName + " ToBuilder()\r\n{\r\nreturn this;\r\n}\r\n");

            csStringBuilder.Append("public " + currentMessageName + " Build()\r\n{\r\nreturn this;\r\n}\r\n");

            //csStringBuilder.Append(CodeTemplate.ToByteStringMethod);
            csStringBuilder.Append(CodeTemplate.GenerateMergeFromMethod(currentMessageName));
            csStringBuilder.Append(CodeTemplate.GenerateParseFromMethod(currentMessageName));

            csStringBuilder.Append(" #endregion\r\n");

            csStringBuilder.Append("}\r\n");
        }

        private void CreateConstructorMethod(ref StringBuilder csStringBuilder, List<string> currentRepeatList, string currentMessageName)
        {
            if (currentRepeatList.Count > 0)
            {
                csStringBuilder.Append("public " + currentMessageName + "()\r\n{\r\n");
                foreach (var repeatValue in currentRepeatList)
                {
                    string[] items = repeatValue.Split('|');
                    csStringBuilder.Append(items[1] + "List = new List<" + items[0] + ">();\r\n");
                }
                csStringBuilder.Append("}\r\n");
            }
        }

        private string CreateListPropertiesReturnProperty(ref StringBuilder csStringBuilder, string item, ref int index)
        {
            var elements = item.Split(new char[] { ' ', '\t', '=' }, StringSplitOptions.RemoveEmptyEntries);
            string valueType = ConvertType(elements[1]);
            string propertyName = elements[2].Substring(0, 1).ToUpper() + elements[2].Substring(1);
            propertyName = propertyName.Replace("_", "");
            string repeateProperty = valueType + "|" + propertyName;
            csStringBuilder.Append(string.Format("private List<{0}> _{1};\r\n", valueType, propertyName));
            csStringBuilder.Append(string.Format("[ProtoBuf.ProtoMember({0})]\r\n", index++));
            csStringBuilder.Append("public List<" + valueType + "> " + propertyName + "List \r\n{\r\nget\r\n{\r\nreturn _" + propertyName + ";\r\n}\r\n");
            csStringBuilder.Append("set\r\n{\r\n _" + propertyName + "=value;\r\n");
            csStringBuilder.Append(string.Format("Notify(\"{0}\");\r\n", propertyName));
            csStringBuilder.Append("}\r\n}\r\n");

            csStringBuilder.Append("public int " + propertyName + "Count\r\n{\r\nget\r\n{\r\nreturn " + propertyName + "List.Count;\r\n}\r\n}\r\n");
            return repeateProperty;
        }

        /// <summary>
        /// Add set property value method
        /// </summary>
        /// <param name="csStringBuilder"></param>
        /// <param name="currentPropertyList"></param>
        /// <param name="currentMessageName"></param>
        private void AddSetPropertyValueMethod(ref StringBuilder csStringBuilder, List<string> currentPropertyList, string currentMessageName)
        {
            if (currentPropertyList.Count > 0)
            {
                csStringBuilder.Append("#region Properties related method\r\n");

                foreach (var repeatValue in currentPropertyList)
                {
                    string[] items = repeatValue.Split('|');
                    //Set property value method
                    csStringBuilder.Append("public " + currentMessageName + " Set" + items[0] + "(" + items[1] + " " + items[0].ToLower() + ")\r\n");
                    csStringBuilder.Append("{\r\n " + items[0] + "=" + items[0].ToLower() + ";\r\nreturn this;\r\n}\r\n");

                    //Clear property value method
                    csStringBuilder.Append("public " + currentMessageName + " Clear" + items[0] + "()\r\n");
                    string defaultValue = items[1] == "string" ? "\"\"" : "default(" + items[1] + ")";
                    csStringBuilder.Append("{\r\n " + items[0] + "=" + defaultValue + ";\r\nHas" + items[0] + "=false;\r\nreturn this;\r\n}\r\n");
                }

                csStringBuilder.Append("#endregion\r\n");
            }
        }

        /// <summary>
        /// Add clear list property value
        /// </summary>
        /// <param name="csStringBuilder"></param>
        /// <param name="currentRepeatList"></param>
        private void AddClearListValueMethod(ref StringBuilder csStringBuilder, List<string> currentRepeatList, string currentMessageName)
        {
            if (currentRepeatList.Count > 0)
            {
                csStringBuilder.Append("#region List properties related method\r\n");

                foreach (var repeatValue in currentRepeatList)
                {
                    string[] items = repeatValue.Split('|');
                    csStringBuilder.Append("public " + currentMessageName + " Clear" + items[1] + "()\r\n{\r\n" + items[1] + "List.Clear();\r\nreturn this;\r\n}\r\n");

                    //Add value
                    csStringBuilder.Append("public " + currentMessageName + " Add" + items[1] + "(" + items[0] + " " + items[1].ToLower() + ")\r\n{\r\n" + items[1] + "List.Add(" + items[1].ToLower() + ");\r\nreturn this;\r\n}\r\n");

                    //Add range value
                    csStringBuilder.Append("public " + currentMessageName + " AddRange" + items[1] + "(IEnumerable<" + items[0] + "> " + items[1].ToLower() + "s)\r\n{\r\n" + items[1] + "List.AddRange(" + items[1].ToLower() + "s);\r\nreturn this;\r\n}\r\n");

                    csStringBuilder.Append(CodeTemplate.GenerateGetItemValueMethod(items[1], items[0]));
                }

                csStringBuilder.Append("#endregion\r\n");
            }
        }
    }
}
