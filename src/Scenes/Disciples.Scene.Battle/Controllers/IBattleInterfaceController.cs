using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер, который взаимодействует и управляет интерфейсом во время битвы.
/// </summary>
internal interface IBattleInterfaceController : ISupportLoading
{
    /// <summary>
    /// Инициализировать начальное состояние.
    /// </summary>
    void PreLoad();

    /// <summary>
    /// Обработать события перед обновлением сцены.
    /// </summary>
    void BeforeSceneUpdate();

    /// <summary>
    /// Обработать завершение обновлении сцены.
    /// </summary>
    void AfterSceneUpdate();

    /// <summary>
    /// Получить позицию юнита на поле боя.
    /// </summary>
    RectangleD GetBattleUnitPosition(BattleSquadPosition squadPosition, UnitSquadPosition unitPosition);

    #region События пользовательского ввода

    /// <summary>
    /// Событие выбора юнита.
    /// </summary>
    void BattleUnitSelected(BattleUnit battleUnit);

    /// <summary>
    /// Событие сброса выбора юнита.
    /// </summary>
    void BattleUnitUnselected(BattleUnit battleUnit);

    /// <summary>
    /// Клик ЛКМ на юните.
    /// </summary>
    void BattleUnitLeftMouseButtonClicked(BattleUnit battleUnit);

    /// <summary>
    /// Зажатая ПКМ на юните.
    /// </summary>
    void BattleUnitRightMouseButtonPressed(BattleUnit battleUnit);

    /// <summary>
    /// Клик ЛКМ на плейсхолдере вызова юнита.
    /// </summary>
    void SummonPlaceholderLeftMouseButtonClicked(SummonPlaceholder summonPlaceholder);

    /// <summary>
    /// Зажатая ПКМ на плейсхолдере вызова юнита.
    /// </summary>
    void SummonPlaceholderRightMouseButtonPressed(SummonPlaceholder summonPlaceholder);

    /// <summary>
    /// Событие выбора портрета юнита.
    /// </summary>
    void UnitPortraitSelected(UnitPortraitObject unitPortrait);

    /// <summary>
    /// Клик ЛКМ на портрете юнита.
    /// </summary>
    void UnitPortraitLeftMouseButtonClicked(UnitPortraitObject unitPortrait);

    /// <summary>
    /// Зажатая ПКМ портрете юнита.
    /// </summary>
    void UnitPortraitRightMouseButtonPressed(UnitPortraitObject unitPortrait);

    /// <summary>
    /// Зажатая ПКМ портрете юнита нижней панели.
    /// </summary>
    void BottomUnitPortraitRightMouseButtonPressed(BottomUnitPortraitObject bottomUnitPortrait);

    #endregion
}