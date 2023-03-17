using Disciples.ResourceProvider;
using System.IO;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения изображения больших портретов юнитов.
/// </summary>
public class UnitPortraitImagesExtractor : ImagesExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="UnitPortraitImagesExtractor" />.
    /// </summary>
    public UnitPortraitImagesExtractor() : base($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Events.ff")
    {
    }
}