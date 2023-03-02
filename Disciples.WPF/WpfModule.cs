using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Managers;
using Disciples.WPF.Controllers;
using Disciples.WPF.Factories;
using Disciples.WPF.Managers;
using DryIoc;

namespace Disciples.WPF;

/// <summary>
/// Модуль для регистрации зависимостей WPF.
/// </summary>
public class WpfModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        // Регистрируем устройства ввода.
        containerRegistrator.Register<IInputManager, WpfInputManager>(Reuse.Singleton);

        // Регистрируем таймер.
        containerRegistrator.Register<IGameTimer, WpfGameTimer>(Reuse.Singleton);

        // Регистрируем фабрики.
        containerRegistrator.Register<IBitmapFactory, WpfBitmapFactory>();
        containerRegistrator.Register<IPlatformSceneObjectContainer, WpfSceneObjectContainer>();

        // Регистрируем View и ViewModel.
        containerRegistrator.Register<GameWindow>();
        containerRegistrator.Register<GameWindowViewModel>();
    }
}