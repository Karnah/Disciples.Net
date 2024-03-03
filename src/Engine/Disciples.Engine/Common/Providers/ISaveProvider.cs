using System.Collections.Generic;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Поставщик информации о сейвах.
/// </summary>
public interface ISaveProvider
{
    /// <summary>
    /// Получить список сейвов.
    /// </summary>
    IReadOnlyList<Save> GetSaves(MissionType? missionType = null);
}