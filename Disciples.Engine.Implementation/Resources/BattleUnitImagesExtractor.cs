using Disciples.ResourceProvider;
using System.IO;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения изображений юнитов в битве.
/// </summary>
public class BattleUnitImagesExtractor : ImagesExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitImagesExtractor" />.
    /// </summary>
    public BattleUnitImagesExtractor() : base($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\BatUnits.ff")
    {
    }
}