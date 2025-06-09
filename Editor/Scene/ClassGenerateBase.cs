using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Core
{
    public class ClassGenerateBase
    {
        public StringBuilder ClassData = new StringBuilder();

        public virtual void SetupCode(object codeData)
        {

        }

        public virtual string GenClass()
        {
            return string.Empty;
        }

        public virtual void SetupFunction()
        {

        }

        public virtual void SetupProperty()
        {

        }
    }
}


