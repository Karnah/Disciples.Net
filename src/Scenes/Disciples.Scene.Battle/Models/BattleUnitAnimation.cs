﻿using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Вся информация об анимации юнита.
/// </summary>
internal class BattleUnitAnimation
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitAnimation" />.
    /// </summary>
    public BattleUnitAnimation(
        Dictionary<BattleUnitState, BattleUnitFrames> battleUnitFrames,
        BattleUnitTargetAnimation? targetAnimation,
        AnimationFrames deathFrames)
    {
        BattleUnitFrames = battleUnitFrames;
        TargetAnimation = targetAnimation;
        DeathFrames = deathFrames;
    }


    /// <summary>
    /// Кадры анимации для каждого состояния юнита.
    /// </summary>
    public Dictionary<BattleUnitState, BattleUnitFrames> BattleUnitFrames { get; }

    /// <summary>
    /// Информации об анимации, которая применяются в юниту-цели.
    /// </summary>
    public BattleUnitTargetAnimation? TargetAnimation { get; }

    /// <summary>
    /// Анимация смерти юнита.
    /// </summary>
    public AnimationFrames DeathFrames { get; }
}