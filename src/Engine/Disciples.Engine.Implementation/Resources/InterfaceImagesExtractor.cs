using System.IO;
using Disciples.Resources.Images;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения изображения интерфейса.
/// </summary>
public class InterfaceImagesExtractor : ImagesExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="InterfaceImagesExtractor" />.
    /// </summary>
    public InterfaceImagesExtractor() : base($"{Directory.GetCurrentDirectory()}/Resources/interf/Interf.ff")
    {
    }
}