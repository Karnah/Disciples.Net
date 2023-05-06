using System.Drawing;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.SceneObjects;

/// <summary>
/// Игровой объект, который отображает текст на сцене.
/// </summary>
public interface ITextSceneObject : ISceneObject
{
    /// <summary>
    /// Текст, который необходимо отобразить.
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Размер шрифта.
    /// </summary>
    double FontSize { get; }

    /// <summary>
    /// Жирный ли шрифт.
    /// </summary>
    bool IsBold { get; }

    /// <summary>
    /// Выравнивание текста по ширине.
    /// </summary>
    TextAlignment TextAlignment { get; }

    /// <summary>
    /// Цвет текста.
    /// </summary>
    Color Foreground { get; }
}