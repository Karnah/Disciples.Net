using System;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Platform.Managers;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <inheritdoc />
public abstract class BaseCursorController : ICursorController
{
    private readonly IInterfaceProvider _interfaceProvider;

    private CursorType _currentCursorType;

    /// <summary>
    /// Создать объект типа <see cref="BaseCursorController" />.
    /// </summary>
    protected BaseCursorController(IInterfaceProvider interfaceProvider)
    {
        _interfaceProvider = interfaceProvider;
    }

    /// <inheritdoc />
    public void SetCursorState(CursorType cursorType)
    {
        if (_currentCursorType == cursorType)
            return;

        switch (cursorType)
        {
            case CursorType.Default:
                SetDefaultCursorState();
                break;

            case CursorType.None:
                SetNoneCursorState();
                break;

            case CursorType.Enemy:
                break;
            case CursorType.Ally:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(cursorType), cursorType, null);
        }

        _currentCursorType = cursorType;
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