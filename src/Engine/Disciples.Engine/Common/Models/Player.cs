﻿using System.Collections.Generic;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Информация об игроке.
/// </summary>
public class Player
{
    /// <summary>
    /// Уникальный идентификатор игрока.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Управляется ли игрок компьютером (ИИ).
    /// </summary>
    public bool IsComputer { get; init; }

    /// <summary>
    /// Раса игрока.
    /// </summary>
    public RaceType Race { get; init; }

    /// <summary>
    /// Отряды игрока.
    /// </summary>
    public List<PlayerSquad> Squads { get; init; } = new();
}