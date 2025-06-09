using System;
using System.Collections.Generic;

namespace Core
{
    public class UIExecutionData
    {
        public Action<BaseView> Callback { get; set; }
        
        public Action<List<BaseView>> Callbacks { get; set; }

        public int ViewKey { get; set; }

        public object[] Args { get; set; }
        
        public bool IsWidget { get; set; }
        
        public bool IsDisable { get; set; }
        
        public BaseView View { get; set; }
    }
}


