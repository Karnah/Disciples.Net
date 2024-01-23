using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Провайдер для основных изображений на поле битвы.
/// </summary>
public interface IBattleInterfaceProvider : ISupportLoading
{
    /// <summary>
    /// Интерфейс битвы.
    /// </summary>
    SceneInterface BattleInterface { get; }

    /// <summary>
    /// Интерфейс диалога с детальной информацией о юните.
    /// </summary>
    SceneInterface UnitDetailInfoInterface { get; }

    /// <summary>
    /// Фон поля боя.
    /// </summary>
    /// <remarks>Фон может состоять из нескольких изображений, поэтому возвращается список.</remarks>
    IReadOnlyList<IBitmap> Battleground { get; }

    /// <summary>
    /// Картинка-разделитель для панели юнитов.
    /// </summary>
    IBitmap PanelSeparator { get; }

    /// <summary>
    /// Иконка умершего обычного юнита.
    /// </summary>
    IBitmap DeathSkullSmall { get; }

    /// <summary>
    /// Иконка умершего большого юнита.
    /// </summary>
    IBitmap DeathSkullBig { get; }

    /// <summary>
    /// Иконка для обозначения юнита, чей уровень выше базового на 5-9.
    /// </summary>
    IBitmap BlueLevelIcon { get; }

    /// <summary>
    /// Иконка для обозначения юнита, чей уровень выше базового на 10-14.
    /// </summary>
    IBitmap OrangeLevelIcon { get; }

    /// <summary>
    /// Иконка для обозначения юнита, чей уровень выше базового на 15 и больше.
    /// </summary>
    IBitmap RedLevelIcon { get; }

    /// <summary>
    /// Иконка защиты юнита.
    /// </summary>
    IBitmap UnitPortraitDefendIcon { get; }

    /// <summary>
    /// Иконка сбежавшего юнита.
    /// </summary>
    IBitmap UnitPortraitRetreatedIcon { get; }

    /// <summary>
    /// Иконки для эффектов, воздействующих на юнита.
    /// </summary>
    IReadOnlyDictionary<UnitAttackType, IBitmap> UnitBattleEffectsIcon { get; }


    /// <summary>
    /// Получить анимацию рамки для юнита, которого можно атаковать.
    /// </summary>
    /// <param name="isSmallUnit">Юнит занимает только одну клетку.</param>
    AnimationFrames GetUnitAttackBorder(bool isSmallUnit);

    /// <summary>
    /// Получить анимацию рамки атаки для юнита, который атакует всё поле боя.
    /// </summary>
    AnimationFrames GetFieldAttackBorder();


    /// <summary>
    /// Получить анимацию рамки для юнита, который ходит в данный момент.
    /// </summary>
    /// <param name="isSmallUnit">Юнит занимает только одну клетку.</param>
    AnimationFrames GetUnitSelectionBorder(bool isSmallUnit);


    /// <summary>
    /// Получить анимацию исцеления для юнита, на которого можно наложить эффект.
    /// </summary>
    /// <param name="isSmallUnit">Юнит занимает только одну клетку.</param>
    AnimationFrames GetUnitHealBorder(bool isSmallUnit);

    /// <summary>
    /// Получить анимацию рамки исцеления для юнита, который накладывает эффект на всё поле боя.
    /// </summary>
    AnimationFrames GetFieldHealBorder();

    /// <summary>
    /// Получить плейсхолдеры для юнитов (сами юниты, их портреты или строки с жизнями).
    /// </summary>
    IReadOnlyDictionary<int, SceneElement> GetUnitPlaceholders(string pattern);
}