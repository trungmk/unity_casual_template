using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IUIExecutionHandler
    {
        ViewExecution SetupShowingExecution(int viewKey, Type viewType = null, params object[] args);

        ViewExecution SetupWidgetShowingExecution(int viewKey, Type viewType = null, params object[] args);

        ViewExecution SetupHidingExecution(Type viewType, bool isDestroy = false, params object[] args);

        ViewExecution SetupHidingExecution(int viewKey, bool isDestroy = false, params object[] args);

        ViewExecution SetupWidgetHidingExecution(WidgetView view, bool isDestroy = false, params object[] args);

        void RunShowingExecution(ShowingUIHandler openingHandler, bool isWidget = false);

        void RunHidingExecution(HidingUIHandler hidingHandler, bool isWidget = false);

        void RemoveViewExecutionByKey(int viewKey);
    }
}
