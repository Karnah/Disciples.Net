using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Resources.Sounds.Models;

namespace Disciples.Scene.Battle.Providers;

/// <summary>
/// Поставщик ресурсов для сцены битвы.
/// </summary>
internal interface IBattleResourceProvider : ISupportLoading
{
    #region Анимации

    /// <summary>
    /// Получить анимацию для сцены битвы.
    /// </summary>
    /// <param name="animationName">Имя анимации.</param>
    AnimationFrames GetBattleAnimation(string animationName);

    /// <summary>
    /// Получить изображение для сцены битвы.
    /// </summary>
    /// <param name="frameName">Имя изображения.</param>
    IBitmap GetBattleBitmap(string frameName);

    #endregion

    #region Изображения

    /// <summary>
    /// Получить случайный фон для поля боя.
    /// </summary>
    /// <remarks>Фон может состоять из нескольких изображений, поэтому возвращается список.</remarks>
    IReadOnlyList<IBitmap> GetRandomBattleground();

    #endregion

    #region Звуки

    /// <summary>
    /// Звук смерти юнита.
    /// </summary>
    RawSound UnitDeathSound { get; }

    /// <summary>
    /// Получить звук по имени.
    /// </summary>
    RawSound? GetSound(string soundName);

    /// <summary>
    /// Получить звук для атаки.
    /// </summary>
    RawSound? GetAttackTypeSound(UnitAttackType attackType);

    #endregion
}