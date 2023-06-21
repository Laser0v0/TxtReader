using System;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.International.Converters.PinYinConverter;


namespace TxtReader
{
    public static class StaticMethods
    {
        public static string getPinYin(char ch)
        {
            string py = string.Join(" ",
                new ChineseChar(ch).Pinyins).ToLower();
            return $"{ch}：{py}";
        }

        // 显示转义字符
        public static string toLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

    }
}
