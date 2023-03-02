using Disciples.Avalonia.Factories;
using Disciples.Avalonia.Managers;
using Disciples.Engine.Base;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Managers;
using DryIoc;

namespace Disciples.Avalonia;

/// <summary>
/// Модуль для регистрации зависимостей Avalonia.
/// </summary>
public class AvaloniaModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        // Регистрируем устройства ввода.
        containerRegistrator.Register<IInputManager, AvaloniaInputManager>(Reuse.Singleton);

        // Регистрируем таймер.
        containerRegistrator.Register<IGameTimer, AvaloniaGameTimer>(Reuse.Singleton);

        // Регистрируем фабрики.
        containerRegistrator.Register<IBitmapFactory, AvaloniaBitmapFactory>();
        containerRegistrator.Register<ISceneFactory, AvaloniaSceneFactory>();

        // Регистрация ViewModel.
        containerRegistrator.Register<GameWindowViewModel>();
    }
}