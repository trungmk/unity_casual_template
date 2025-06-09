using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core
{
    public class UIAssigningGenerate : ClassGenerateBase
    {
//        private readonly StringBuilder _enumBuilder = new StringBuilder();
    
        private readonly List<string> _groupLayer = new List<string>();

        private readonly UIConfigEditor _uiConfigEditor;

        private readonly List<string> _defaultLayerNames = new List<string>
        {
            "None", "Dialog", "Panel", "ScreenTransition"
        };

        public UIAssigningGenerate(UIConfigEditor configEditor)
        {
            _uiConfigEditor = configEditor;
        }

        public void SetupDefaultCode()
        {
            for (int i = 0; i < _defaultLayerNames.Count; i++)
            {
                string enumRegis = $"\t{@_defaultLayerNames[i]},\n";
//                _enumBuilder.AppendLine(enumRegis);
            }
        }
    
        public override void SetupCode(object codeData)
        {
            UIData view = codeData as UIData;
    
            string resourcesPath = GetResourcePath(view.FullPath);
            string group = GetGroup(resourcesPath);


            if(!_groupLayer.Contains(group))
            {
                _groupLayer.Add(group);
            }
    
            string uiRegis =
                $"\t\tUIHandler.AddView ({view.Name.GetHashCode()}, \"{view.Name}\", typeof({view.ViewType.FullName}), \"{view.FullPath}\", \"{resourcesPath}\", {"UILayer." + @group});\n";
    
            ClassData.AppendLine(uiRegis);
        }
    
        public override string GenClass()
        {
//            string enumString = "\n\npublic enum UILayer\n" +
//                                "{\n" +
//                                "@Enum" +
//                                "}";
//            enumString = enumString.Replace("@Enum", _enumBuilder.ToString());
    
            string classString = "\n\npublic class UIRegistration\n" +
                                "{\n" +
                                "\t[UnityEngine.RuntimeInitializeOnLoadMethod]\n" +
                                "\tstatic void AssignUI()\n\t{\n" +
                                "@Func" +
                                "\t}\n" +
                                "}";
            classString = classString.Replace("@Func", ClassData.ToString());
     
            return classString;
        }
    
        private string GetResourcePath(string fullPath)
        {
            string pathReplaced = fullPath.Replace(_uiConfigEditor.UIPrefabPath, string.Empty);
            string path = pathReplaced.Replace(".prefab", string.Empty);
            return path;
        }
    
        private string GetGroup(string resourcesPath)
        {
            char filter = '/';
            string[] pathSplited = resourcesPath.Split(filter);
    
            // 0 : GUI, 1 : Group, 2... : sub folders
            string group = pathSplited[1];
            return group;
        }
    }
}

