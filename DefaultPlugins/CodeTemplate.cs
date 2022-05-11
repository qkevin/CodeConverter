using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefaultPlugins
{
    class CodeTemplate
    {
        public static readonly string ToByteStringMethod = @"public Google.ProtocolBuffers.ByteString ToByteString()
        {
            return this.Serialize();
        }
";

        private static readonly string MergeFromMethod = @"public * MergeFrom(Google.ProtocolBuffers.ByteString value)
        {
            return Deserialize<*>(value.ToByteArray());
        }
        public * MergeFrom(byte[] value)
        {
            return Deserialize<*>(value);
        }
";

        private static readonly string ParseFromMethod = @"public static * ParseFrom(byte[] value)
        {
            return Deserialize<*>(value);
        }
";

        private static readonly string GetItemValueMethod = @"public valueType Get*(int i)
        {
            return *List[i];
        }
";

        public static string GenerateMergeFromMethod(string className)
        {
            return MergeFromMethod.Replace("*", className);
        }
        public static string GenerateParseFromMethod(string className)
        {
            return ParseFromMethod.Replace("*", className);
        }

        public static string GenerateGetItemValueMethod(string propertyName, string returnType)
        {
            return GetItemValueMethod.Replace("*", propertyName).Replace("valueType", returnType); ;
        }
    }
}
