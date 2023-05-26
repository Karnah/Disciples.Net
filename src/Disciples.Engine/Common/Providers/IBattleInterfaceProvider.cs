using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Провайдер для основных изображений на поле битвы.
/// </summary>
public interface IBattleInterfaceProvider : ISupportLoading
{
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
    /// Картинка левой панели с юнитами.
    /// </summary>
    IBitmap LeftPanel { get; }

    /// <summary>
    /// Картинка правой панели с юнитами.
    /// </summary>
    IBitmap RightPanel { get; }

    /// <summary>
    /// Картинка нижней панели.
    /// </summary>
    IBitmap BottomPanel { get; }

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
    /// Иконки для эффектов, воздействующих на юнита.
    /// </summary>
    IReadOnlyDictionary<UnitAttackType, IBitmap> UnitBattleEffectsIcon { get; }


    /// <summary>
    /// Иконки для кнопки переключения правой панели юнитов.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> ToggleRightButton { get; }

    /// <summary>
    /// Иконки для кнопки защиты.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> DefendButton { get; }

    /// <summary>
    /// Иконки для кнопки отступления.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> RetreatButton { get; }

    /// <summary>
    /// Иконки для кнопки ожидания.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> WaitButton { get; }

    /// <summary>
    /// Иконки для мгновенного завершения битвы.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> InstantResolveButton { get; }

    /// <summary>
    /// Иконки для автоматической битвы.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> AutoBattleButton { get; }

    /// <summary>
    /// Иконки для выхода из битвы и отображения интерфейса отряда.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> OpenSquadInventoryButton { get; }

    /// <summary>
    /// Иконки для выхода из битвы.
    /// </summary>
    IReadOnlyDictionary<SceneButtonState, IBitmap> ExitButton { get; }


    /// <summary>
    /// Получить анимацию рамки для юнита, которого можно атаковать.
    /// </summary>
    /// <param name="sizeSmall">Юнит занимает только одну клетку.</param>
    IReadOnlyList<Frame> GetUnitAttackBorder(bool sizeSmall);

    /// <summary>
    /// Получить анимацию рамки атаки для юнита, который атакует всё поле боя.
    /// </summary>
    IReadOnlyList<Frame> GetFieldAttackBorder();


    /// <summary>
    /// Получить анимацию рамки для юнита, который ходит в данный момент.
    /// </summary>
    /// <param name="sizeSmall">Юнит занимает только одну клетку.</param>
    IReadOnlyList<Frame> GetUnitSelectionBorder(bool sizeSmall);


    /// <summary>
    /// Получить анимацию исцеления для юнита, на которого можно наложить эффект.
    /// </summary>
    /// <param name="sizeSmall">Юнит занимает только одну клетку.</param>
    IReadOnlyList<Frame> GetUnitHealBorder(bool sizeSmall);

    /// <summary>
    /// Получить анимацию рамки исцеления для юнита, который накладывает эффект на всё поле боя.
    /// </summary>
    IReadOnlyList<Frame> GetFieldHealBorder();
}