namespace Disciples.Resources.Images.Enums;

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
    /// Картинка или анимация.
    /// </summary>
    Image,

    /// <summary>
    /// Обычная кнопка.
    /// </summary>
    Button,

    /// <summary>
    /// Кнопка-переключатель из двух состояний.
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
    /// Список строк.
    /// </summary>
    TextListBox,

    /// <summary>
    /// Редактируемое текстовое поле.
    /// </summary>
    EditTextBox,
}