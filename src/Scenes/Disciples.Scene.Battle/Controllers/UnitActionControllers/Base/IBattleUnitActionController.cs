namespace Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;

/// <summary>
/// Контроллер для действия юнита на поле боя.
/// </summary>
internal interface IBattleUnitActionController
{
    /// <summary>
    /// Признак, что юнит завершил действие.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Признак, что ход должен перейти следующему юниту.
    /// </summary>
    /// <remarks>
    /// Использовать только, если <see cref="IsCompleted" /> == <see langword="true" />.
    /// </remarks>
    bool ShouldPassTurn { get; }

    /// <summary>
    /// Инициализировать действие юнита.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Обновить состояние объектов на сцене.
    /// </summary>
    void BeforeSceneUpdate();

    /// <summary>
    /// Обновить состояние объектов на сцене.
    /// </summary>
    void AfterSceneUpdate();
}