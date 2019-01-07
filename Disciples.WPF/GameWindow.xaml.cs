using System.Windows;

namespace Disciples.WPF
{
    /// <summary>
    /// Окно игры.
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameWindow(GameWindowViewModel viewModel)
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }
    }
}