using UnityEditor;
using UnityEngine;

namespace Core
{
    public class ContextNameGenerate : ClassGenerateBase
    {
        private const string CONTEXT_PREFIX_NAME = "CONTEXT_";

        public override void SetupCode(object codeData)
        {
            SceneSO context = codeData as SceneSO;
            string contextName = CONTEXT_PREFIX_NAME + context.SceneName.ToUpper();
            ClassData.AppendLine($"\tpublic const int { contextName } = { contextName.GetHashCode() };\n");
        }

        public override string GenClass()
        {
            string classString = "public struct ContextNameGenerated\n" +
                                 "{\n" +
                                 "@Members" +
                                 "}";

            return classString.Replace("@Members", ClassData.ToString());
        }
    }
}

