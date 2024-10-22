﻿using Disciples.Avalonia.Controllers;
using Disciples.Avalonia.Factories;
using Disciples.Avalonia.Managers;
using Disciples.Avalonia.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
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
        // Информация об игре.
        containerRegistrator.Register<AvaloniaGameInfo>(Reuse.Singleton);

        // Регистрируем устройства ввода.
        containerRegistrator.Register<IInputManager, AvaloniaInputManager>(Reuse.Singleton);
        containerRegistrator.Register<ICursorController, AvaloniaCursorController>(Reuse.Singleton);

        // Регистрируем таймер.
        containerRegistrator.Register<IGameTimer, AvaloniaGameTimer>(Reuse.Singleton);

        // Регистрируем фабрики.
        containerRegistrator.Register<IBitmapFactory, AvaloniaBitmapFactory>();
        containerRegistrator.Register<IPlatformSceneObjectContainer, AvaloniaSceneObjectContainer>();

        // Регистрация ViewModel.
        containerRegistrator.Register<GameWindowViewModel>();
    }
}