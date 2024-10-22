﻿namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Направление юнита на поле боя.
/// </summary>
internal enum BattleDirection
{
    /// <summary>
    /// Юнит направлен лицом к игроку.
    /// </summary>
    /// <remarks>Все юниты в атакующем отряде в начале боя направлены лицом к игроку.</remarks>
    Face,

    /// <summary>
    /// Юнит направлен спиной к игроку.
    /// </summary>
    /// <remarks>Все юниты в защищающемся отряде в начале боя направлены спиной к игроку.</remarks>
    Back
}