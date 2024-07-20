using Disciples.Resources.Sounds;
using System.IO;

namespace Disciples.Engine.Implementation.Resources;

/// <summary>
/// Класс для извлечения звуков битвы.
/// </summary>
public class BattleSoundsExtractor : SoundsExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="BattleSoundsExtractor" />.
    /// </summary>
    public BattleSoundsExtractor() : base($"{Directory.GetCurrentDirectory()}/Resources/Sounds/Battle.wdb")
    {
    }
}