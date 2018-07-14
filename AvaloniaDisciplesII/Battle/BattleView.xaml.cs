using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleView : UserControl
    {
        public BattleView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
