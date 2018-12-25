using Disciples.Avalonia.ViewModels;

namespace Disciples.Avalonia
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
