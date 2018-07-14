using AvaloniaDisciplesII.ViewModels;
using Engine;

namespace AvaloniaDisciplesII
{
    public class GameWindowViewModel : ViewModelBase
    {
        public GameWindowViewModel(PageViewModel viewContext)
        {
            ViewContext = viewContext;
        }


        public double Width => 800 * GameInfo.Scale;

        public double Height => 600 * GameInfo.Scale;

        public PageViewModel ViewContext { get; }
    }
}
