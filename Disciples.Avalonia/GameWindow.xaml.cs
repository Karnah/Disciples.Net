using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Disciples.Engine;

namespace Disciples.Avalonia
{
    /// <summary>
    /// Окно игры.
    /// </summary>
    public class GameWindow : Window
    {
        public GameWindow(GameWindowViewModel viewModel)
        {
            this.DataContext = viewModel;

            Activated += OnActivated;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
            Renderer.DrawFps = true;
#endif
        }

        private void OnActivated(object sender, EventArgs e)
        {
            var screen = this.Screens.ScreenFromVisual(this);

            // Рассчитываем реальные размер экрана и пропорционально растягиваем изображение.
            var scale = Math.Min(screen.WorkingArea.Width / GameInfo.OriginalWidth, screen.WorkingArea.Height / GameInfo.OriginalHeight);

            var field = this.Find<Grid>("Field");
            field.RenderTransform = new ScaleTransform(scale, scale);

            GameInfo.Width = scale * GameInfo.OriginalWidth;
            GameInfo.Height = scale * GameInfo.OriginalHeight;
            GameInfo.Scale = scale;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}