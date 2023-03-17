using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Провайдер для основных изображений на поле битвы.
/// </summary>
public interface IBattleInterfaceProvider : ISupportLoading
{
    /// <summary>
    /// Фон поля боя.
    /// </summary>
    /// <remarks>Фон может состоять из нескольких изображений, поэтому возвращается список.</remarks>
    IReadOnlyList<IBitmap> Battleground { get; }

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
    /// Задний фон для подробной информации о юните (раскрытый свиток).
    /// </summary>
    IBitmap UnitInfoBackground { get; }

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
    /// Иконки для эффектов, воздействующих на юнита.
    /// </summary>
    IDictionary<UnitBattleEffectType, IBitmap> UnitBattleEffectsIcon { get; }


    /// <summary>
    /// Иконки для кнопки переключения правой панели юнитов.
    /// </summary>
    IDictionary<SceneButtonState, IBitmap> ToggleRightButton { get; }

    /// <summary>
    /// Иконки для кнопки защиты.
    /// </summary>
    IDictionary<SceneButtonState, IBitmap> DefendButton { get; }

    /// <summary>
    /// Иконки для кнопки отступления.
    /// </summary>
    IDictionary<SceneButtonState, IBitmap> RetreatButton { get; }

    /// <summary>
    /// Иконки для кнопки ожидания.
    /// </summary>
    IDictionary<SceneButtonState, IBitmap> WaitButton { get; }

    /// <summary>
    /// Иконки для мгновенного завершения битвы.
    /// </summary>
    IDictionary<SceneButtonState, IBitmap> InstantResolveButton { get; }

    /// <summary>
    /// Иконки для автоматической битвы.
    /// </summary>
    IDictionary<SceneButtonState, IBitmap> AutoBattleButton { get; }


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