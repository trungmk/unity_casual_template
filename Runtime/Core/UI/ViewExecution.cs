
using System;

namespace Core
{
    public class ViewExecution
    {
        public int ViewKey { get; }

        public Type ViewType { get; }

        public IShowingView ShowingHandler { get; }

        public IHidingUI HidingHandler { get; }

        public bool IsPresenting { get; set; }

        public BaseView View { get; set; }

        public ViewExecution(IHandlingExecution handler, int viewKey, Type type)
        {
            ViewKey = viewKey;
            ViewType = type;
            IsPresenting = false;
            ShowingHandler = new ShowingUIHandler(handler, viewExecution: this);
            HidingHandler = new HidingUIHandler(handler, viewExecution: this);
        }

        public void SetHandler(IHandlingExecution handler)
        {
            ShowingHandler.SetHandler(handler);
            HidingHandler.SetHandler(handler);
        }

        public bool CouldShow()
        {
            // if (View != null && !View.gameObject.activeSelf && !View.gameObject.activeInHierarchy)
            // {
            //     IsPresenting = false;
            // }

            return ShowingHandler != null && !IsPresenting;
        }

        public bool CouldHide()
        {
            return HidingHandler != null && IsPresenting;
        }
    }
}

