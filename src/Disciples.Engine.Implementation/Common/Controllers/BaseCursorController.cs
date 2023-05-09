using System;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Platform.Managers;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <inheritdoc />
public abstract class BaseCursorController : ICursorController
{
    private readonly IInterfaceProvider _interfaceProvider;

    private CursorState _currentCursorState;

    /// <summary>
    /// Создать объект типа <see cref="BaseCursorController" />.
    /// </summary>
    protected BaseCursorController(IInterfaceProvider interfaceProvider)
    {
        _interfaceProvider = interfaceProvider;
    }

    /// <inheritdoc />
    public void SetCursorState(CursorState cursorState)
    {
        if (_currentCursorState == cursorState)
            return;

        switch (cursorState)
        {
            case CursorState.Default:
                SetDefaultCursorState();
                break;

            case CursorState.None:
                SetNoneCursorState();
                break;

            case CursorState.Enemy:
                break;
            case CursorState.Ally:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(cursorState), cursorState, null);
        }

        _currentCursorState = cursorState;
    }

    /// <summary>
    /// Установить состояние курсора по умолчанию.
    /// </summary>
    protected abstract void SetDefaultCursorState();

    /// <summary>
    /// Спрятать курсор.
    /// </summary>
    protected abstract void SetNoneCursorState();

    /// <summary>
    /// Получить значение курсора по умолчанию.
    /// </summary>
    protected IBitmap GetDefaultCursorBitmap()
    {
        return _interfaceProvider.GetImage("_CUDEFAUL");
    }
}