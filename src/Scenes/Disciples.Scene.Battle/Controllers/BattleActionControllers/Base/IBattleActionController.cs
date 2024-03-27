namespace Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;

/// <summary>
/// Контроллер для действия на поле боя.
/// </summary>
internal interface IBattleActionController
{
    /// <summary>
    /// Признак, что действие завершено.
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
    /// Инициализировать действие.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Обновить состояние объектов на сцене перед обновлением.
    /// </summary>
    void BeforeSceneUpdate();

    /// <summary>
    /// Обновить состояние объектов на сцене после обновления.
    /// </summary>
    void AfterSceneUpdate();
}