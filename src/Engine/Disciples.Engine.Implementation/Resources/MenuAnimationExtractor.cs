using Disciples.Resources.Images;
using System.IO;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения ресурсов для анимаций меню.
/// </summary>
public class MenuAnimationExtractor : ImagesExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="MenuAnimationExtractor" />.
    /// </summary>
    public MenuAnimationExtractor() : base($"{Directory.GetCurrentDirectory()}\\Resources\\interf\\MenuAnim.ff")
    {
    }
}