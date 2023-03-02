using System.Windows;
using System.Windows.Controls;

using Disciples.Engine.Common.SceneObjects;

namespace Disciples.WPF.DataTemplateSelectors;

/// <summary>
/// Выбрать шаблон для отображения на сцене.
/// </summary>
public class SceneObjectTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Шаблон для изображения.
    /// </summary>
    public DataTemplate ImageTemplate { get; set; }

    /// <summary>
    /// Шаблон для текста.
    /// </summary>
    public DataTemplate TextTemplate { get; set; }

    /// <inheritdoc />
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is IImageSceneObject)
            return ImageTemplate;

        if (item is ITextSceneObject)
            return TextTemplate;

        return base.SelectTemplate(item, container);
    }
}