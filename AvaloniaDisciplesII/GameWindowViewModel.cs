using AvaloniaDisciplesII.ViewModels;

namespace AvaloniaDisciplesII
{
    public class GameWindowViewModel : ViewModelBase
    {
        public GameWindowViewModel(PageViewModel viewContext)
        {
            ViewContext = viewContext;
        }


        public PageViewModel ViewContext { get; }
    }
}
