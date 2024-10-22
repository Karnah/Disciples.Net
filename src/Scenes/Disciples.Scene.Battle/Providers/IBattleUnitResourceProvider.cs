﻿using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Providers;

/// <summary>
/// Поставщик ресурсов для юнитов на поле боя.
/// </summary>
internal interface IBattleUnitResourceProvider : ISupportLoading
{
    /// <summary>
    /// Анимация для выделения юнита при его ходе.
    /// </summary>
    UnitAnimationFrames UnitTurnAnimationFrames { get; }

    /// <summary>
    /// Анимация для выделения юнита, когда его выбрали в качестве цели.
    /// </summary>
    UnitAnimationFrames UnitTargetAnimationFrames { get; }

    /// <summary>
    /// Анимация места, куда можно призвать юнита.
    /// </summary>
    AnimationFrames SummonPlaceholderAnimationFrames { get; }

    /// <summary>
    /// Анимация удаления призванного юнита.
    /// </summary>
    UnitAnimationFrames UnitUnsummonAnimationFrames { get; }

    /// <summary>
    /// Анимация исцеления вампиризмом.
    /// </summary>
    AnimationFrames DrainLifeHealAnimationFrames { get; }

    /// <summary>
    /// Получить изображение юнита.
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    IBitmap GetUnitFace(UnitType unitType);

    /// <summary>
    /// Получить изображение юнита для битвы (скруглённое изображение).
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    IBitmap GetUnitBattleFace(UnitType unitType);

    /// <summary>
    /// Получить большой портрет юнита (выводится в информации о юните).
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    IBitmap GetUnitPortrait(UnitType unitType);

    /// <summary>
    /// Получить набор анимаций юнита.
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    /// <param name="direction">Направление положения юнита.</param>
    BattleUnitAnimation GetBattleUnitAnimation(UnitType unitType, BattleDirection direction);

    /// <summary>
    /// Получить анимацию атаки, применяемую к юниту.
    /// </summary>
    AnimationFrames? GetAttackTypeAnimation(UnitAttackType effectAttackType, bool isSmallUnit);

    /// <summary>
    /// Получить анимацию для повышения уровня юнитом.
    /// </summary>
    AnimationFrames GetUnitLevelUpAnimation(UnitType unitType);

    /// <summary>
    /// Получить звуки юнита в битве.
    /// </summary>
    BattleUnitSounds GetBattleUnitSounds(UnitType unitType);
}