
namespace Core
{
    public interface IHandlingExecution
    {
        void RunShowingExecution(ShowingUIHandler openingHandler, bool isWidget = false);

        void RunHidingExecution(HidingUIHandler hidingHandler, bool isWidget = false);
    
    }
}


