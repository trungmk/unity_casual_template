using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Core
{
    public class UINameGenerate : ClassGenerateBase
    {
        public UIConfigEditor _uiConfigEditor;

        public UINameGenerate(UIConfigEditor uiConfigEditor)
        {
            _uiConfigEditor = uiConfigEditor;
        }
        
        public override void SetupCode(object codeData)
        {
            UIData view = codeData as UIData;
            string name = NormalizeStringUpper(view.Name);
            ClassData.AppendLine(string.Format("\tpublic const int {0} = {1};\n",
                name, view.Name.GetHashCode()));
        }

        public override string GenClass()
        {
            string classString = "using Core;\n" +
                                 "\npublic struct UIName\n" +
                                 "{\n" +
                                 "@ClassData" +
                                 "}";

            return classString.Replace("@ClassData", ClassData.ToString());
        }

        string NormalizeStringUpper(string viewName)
        {
            Regex lineSplitter = new Regex(@"[A-Z]");
            char[] charArr = viewName.ToCharArray();
            List<char> charList = new List<char>();

            for (int i = 0; i < charArr.Length; i++)
            {
                if(lineSplitter.IsMatch(charArr[i].ToString()) && i != 0)
                {
                    charList.Add('_');
                }

                charList.Add(charArr[i]);
            }

            string result = string.Empty;
            foreach (var c in charList)
            {
                result += c;
            }

            return result.ToUpper();
        }
    }
}

