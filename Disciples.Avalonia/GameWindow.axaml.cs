using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Disciples.Engine;
using DryIoc;

namespace Disciples.Avalonia
{
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            DataContext = ((App)Application.Current!).Container.Resolve<GameWindowViewModel>();

            Activated += OnActivated;

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
            Renderer.DrawFps = true;
#endif
        }

        /// <summary>
        /// Обработать событие отображения окна.
        /// </summary>
        private void OnActivated(object? sender, EventArgs e)
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
    }
}
