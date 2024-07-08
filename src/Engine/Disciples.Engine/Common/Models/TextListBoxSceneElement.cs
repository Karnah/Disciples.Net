using Disciples.Engine.Common.Enums;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Список строк.
/// </summary>
public class TextListBoxSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.TextListBox;

    /// <summary>
    /// Список колонок.
    /// </summary>
    public int ColumnCount { get; init; }

    /// <summary>
    /// Расстояние между элементами в строке.
    /// </summary>
    public int HorizontalSpacing { get; init; }

    /// <summary>
    /// Расстояние между строками.
    /// </summary>
    public int VerticalSpacing { get; init; }

    /// <summary>
    /// Название кнопки для прокрутки списка вверх.
    /// </summary>
    public string? ScrollUpButtonName { get; init; }

    /// <summary>
    /// Название кнопки для прокрутки списка вниз.
    /// </summary>
    public string? ScrollDownButtonName { get; init; }

    /// <summary>
    /// Название кнопки для прокрутки списка влево.
    /// </summary>
    public string? ScrollLeftButtonName { get; init; }

    /// <summary>
    /// Название кнопки для прокрутки списка вправо.
    /// </summary>
    public string? ScrollRightButtonName { get; init; }

    /// <summary>
    /// Название кнопки для прокрутки страницы вверх.
    /// </summary>
    public string? PageUpButtonName { get; init; }

    /// <summary>
    /// Название кнопки для прокрутки страницы вниз.
    /// </summary>
    public string? PageDownButtonName { get; init; }

    /// <summary>
    /// Название кнопки для двойного клика по элементу списка.
    /// </summary>
    public string? DoubleClickButtonName { get; init; }

    /// <summary>
    /// Стиль текста для выбранного элемента.
    /// </summary>
    public TextStyle? SelectedTextStyle { get; init; }

    /// <summary>
    /// Стиль текста для обычного элемента.
    /// </summary>
    public TextStyle? CommonTextStyle { get; init; }

    /// <summary>
    /// Изображение для выделенного элемента?
    /// </summary>
    /// <remarks>
    /// Не уверен, что корректное назначение. В ресурсах игры не используется.
    /// </remarks>
    public IBitmap? SelectionImageName { get; init; }

    /// <summary>
    /// Изображения для неактивного элемента?
    /// </summary>
    /// <remarks>
    /// Не уверен, что корректное назначение. В ресурсах игры не используется.
    /// </remarks>
    public IBitmap? UnselectedImageName { get; init; }

    /// <summary>
    /// Толщина границы.
    /// </summary>
    public int BorderSize { get; init; }

    /// <summary>
    /// Требуется ли создать фоновое изображение.
    /// </summary>
    public bool ShouldCreateBackgroundImage { get; init; }
}