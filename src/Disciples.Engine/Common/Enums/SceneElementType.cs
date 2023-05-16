namespace Disciples.Engine.Common.Enums;

/// <summary>
/// Тип элемента сцены.
/// </summary>
public enum SceneElementType
{
    /// <summary>
    /// Текстовое поле.
    /// </summary>
    TextBlock,

    /// <summary>
    /// Статическая картинка.
    /// </summary>
    Image,

    /// <summary>
    /// Анимация.
    /// </summary>
    Animation,

    /// <summary>
    /// Обычная кнопка.
    /// </summary>
    Button,

    /// <summary>
    /// "Залипающая" кнопка.
    /// </summary>
    ToggleButton,

    /// <summary>
    /// Переключатель.
    /// </summary>
    RadioButton,

    /// <summary>
    /// Список.
    /// </summary>
    ListBox,

    /// <summary>
    /// Редактируемое текстовое поле.
    /// </summary>
    EditTextBox,
}