using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Поставщик информации о расах.
/// </summary>
public interface IRaceProvider : ISupportLoading
{
    /// <summary>
    /// Получить информацию о расе.
    /// </summary>
    Race GetRace(RaceType raceType);

    /// <summary>
    /// Получить изображения для расы.
    /// </summary>
    IBitmap GetRaceImage(RaceType raceType);
}