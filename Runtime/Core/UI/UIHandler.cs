using System.Collections.Generic;
using System;
using System.Collections;

namespace Core
{
    public enum UILayer
    {
        None = 0, Dialog, Panel, ScreenTransition, Widget
    }
    
    public class UIData
    {
        public Type ViewType;

        public string FullPath;

        public string ResourcesPath;

        public string Name;

        public UILayer Layer;

        public int Key;
    }

    public static class UIHandler
    {
        private static readonly List<UIData> _viewGroup = new List<UIData>(); // need change this to list

        public static void AddView(int uiKey, string name, Type viewType, string path, string resourcesPath, UILayer layer)
        {
            _viewGroup.Add(new UIData
            {
                FullPath = path,
                ViewType = viewType,
                Name = name,
                ResourcesPath = resourcesPath,
                Layer = layer,
                Key = uiKey
            });
        }

        public static UIData GetUIData<T>() where T : BaseView
        {
            for (int i = 0; i < _viewGroup.Count; i++)
            {
                if (_viewGroup[i].ViewType == typeof(T))
                {
                    return _viewGroup[i];
                }
            }

            return null;
        }

        public static UIData GetUIData(int uiKey)
        {
            for (int i = 0; i < _viewGroup.Count; i++)
            {
                if (_viewGroup[i].Key == uiKey)
                {
                    return _viewGroup[i];
                }
            }

            return null;
        }
    }
}