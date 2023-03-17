using System.IO;
using Disciples.ResourceProvider;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения изображения лиц юнитов (маленьких портретов).
/// </summary>
public class UnitFaceImagesExtractor : ImagesExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="UnitFaceImagesExtractor" />.
    /// </summary>
    public UnitFaceImagesExtractor() : base($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Faces.ff")
    {
    }
}