using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Platform.Managers;

/// <summary>
/// Менеджер для управления курсором.
/// </summary>
public interface ICursorController
{
    /// <summary>
    /// Установить изображение для курсора.
    /// </summary>
    void SetCursorState(CursorType cursorType);
}