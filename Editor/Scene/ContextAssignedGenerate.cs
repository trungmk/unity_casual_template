using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class ContextAssignedGenerate : ClassGenerateBase
    {
        private const string CONTEXT_PREFIX_NAME = "CONTEXT_";

        public override void SetupCode(object codeData)
        {
            SceneSO context = codeData as SceneSO;
            string contextName = CONTEXT_PREFIX_NAME + context.SceneName.ToUpper();
            string contextRegis = string.Format("\t\tSceneHandler.AddContext ({0}, \"{1}\", \"{2}\");\n",
                contextName.GetHashCode(),
                context.SceneName,
                context.ScenePath);

            ClassData.AppendLine(contextRegis);
        }

        public override string GenClass()
        {
            string classString = "\n\npublic static class ContextRegistration\n" +
                                 "{\n" +
                                 "\t[UnityEngine.RuntimeInitializeOnLoadMethod]\n" +
                                 "\tstatic void AssignContext()\n\t{\n" +
                                 "@Func" +
                                 "\t}\n" +
                                 "}";

            return classString.Replace("@Func", ClassData.ToString());
        }
    }
}

