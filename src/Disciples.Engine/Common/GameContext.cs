using System.Collections.Generic;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common;

/// <summary>
/// Данные игры.
/// </summary>
public class GameContext
{
    /// <summary>
    /// Название саги.
    /// </summary>
    public string SagaName { get; init; } = null!;

    /// <summary>
    /// Описание саги.
    /// </summary>
    public string? SagaDescription { get; init; }

    /// <summary>
    /// Тип игры.
    /// </summary>
    public GameType GameType { get; init; }

    /// <summary>
    /// Тип миссии.
    /// </summary>
    public MissionType MissionType { get; init; }

    /// <summary>
    /// Номер хода.
    /// </summary>
    public int TurnNumber { get; init; }

    /// <summary>
    /// Уровень сложности.
    /// </summary>
    public DifficultyLevel DifficultyLevel { get; init; }

    /// <summary>
    /// Игроки.
    /// </summary>
    public IReadOnlyList<Player> Players { get; init; } = new List<Player>();
}