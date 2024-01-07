using System.IO;
using Disciples.Resources.Sounds;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения звуков, которые соответствуют указанным юнитам/действиям.
/// </summary>
public class BattleSoundsMappingExtractor : SoundsMappingExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="BattleSoundsMappingExtractor" />.
    /// </summary>
    public BattleSoundsMappingExtractor() : base($"{Directory.GetCurrentDirectory()}\\Resources\\Sounds\\Battle.wdt")
    {
    }
}