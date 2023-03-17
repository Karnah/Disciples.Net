using Disciples.ResourceProvider;
using System.IO;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения изображений битвы.
/// </summary>
public class BattleImagesExtractor : ImagesExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="BattleImagesExtractor" />.
    /// </summary>
    public BattleImagesExtractor() : base($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Battle.ff")
    {
    }
}