using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDisciplesII
{
    public class GameWindow : Window
    {
        public GameWindow(GameWindowViewModel viewModel)
        {
            this.DataContext = viewModel;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
            Renderer.DrawFps = true;
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
